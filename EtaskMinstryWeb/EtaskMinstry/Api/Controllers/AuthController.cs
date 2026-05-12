using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Auth;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Api.Services;
using EtaskMinstry.Models.Login;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API authentication endpoints. URL prefix: /api/v1/auth/
    ///   POST login    — username + password → access + refresh tokens
    ///   POST refresh  — rotate refresh token, mint new access token
    ///   POST logout   — revoke refresh token (and optional device token)
    /// </summary>
    public class AuthController : ApiController
    {
        private readonly AuthValidator _validator = new AuthValidator();
        private readonly JwtIssuer _issuer = new JwtIssuer();
        private readonly RefreshTokenService _refreshTokens = new RefreshTokenService();

        [HttpPost]
        public HttpResponseMessage Login(LoginRequest request)
        {
            if (request == null)
                return BuildError(HttpStatusCode.BadRequest, "بيانات الطلب غير صحيحة", "INVALID_REQUEST");

            var result = _validator.Validate(request.Username, request.Password);

            switch (result.Outcome)
            {
                case AuthValidator.Outcome.InvalidCredentials:
                    return BuildError(HttpStatusCode.Unauthorized,
                        "اسم المستخدم أو كلمة المرور غير صحيحة", "INVALID_CREDENTIALS");

                case AuthValidator.Outcome.WrongApplication:
                    // Same external response as bad creds — don't leak tenant info.
                    return BuildError(HttpStatusCode.Unauthorized,
                        "اسم المستخدم أو كلمة المرور غير صحيحة", "INVALID_CREDENTIALS");

                case AuthValidator.Outcome.AccountStopped:
                    return BuildError(HttpStatusCode.Forbidden,
                        "تم إيقاف الحساب", "ACCOUNT_STOPPED");
            }

            // Mirror web behavior (Q1=a): a successful employee login auto-checks-in.
            // UserAccountVM.Checkin is idempotent w.r.t. session flag, but inserts
            // a new Attendance row every call — same as the web's LoginETask path.
            if (result.UserTypeId == (int)LoggedUserType.Employee)
            {
                try
                {
                    new UserAccountVM().Checkin(result.UserId);
                }
                catch
                {
                    // Auto-checkin failure must not block the login. Mirrors the
                    // fact that the web's Checkin already swallows LastHeartbeat
                    // SQL errors.
                }
            }

            var access = _issuer.Issue(result.UserId, result.UserTypeId, result.CompanyId, result.FullName);
            string refreshPlain = _issuer.IssueRefreshToken();
            _refreshTokens.Store(result.UserId, result.UserTypeId, refreshPlain);

            var body = new TokenResponse
            {
                AccessToken = access.Token,
                RefreshToken = refreshPlain,
                ExpiresIn = access.ExpiresInSeconds,
                User = new UserSummaryDto
                {
                    UserId = result.UserId,
                    FullName = result.FullName,
                    Role = result.UserTypeId == (int)LoggedUserType.Company ? "Company" : "Employee",
                    CompanyId = result.CompanyId,
                    CompanyName = result.CompanyName,
                    IsStopped = false
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(body, "تم تسجيل الدخول بنجاح"));
        }

        [HttpPost]
        public HttpResponseMessage Refresh(RefreshRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BuildError(HttpStatusCode.BadRequest, "بيانات الطلب غير صحيحة", "INVALID_REQUEST");

            var lookup = _refreshTokens.FindByPlaintext(request.RefreshToken);
            if (lookup == null)
                return BuildError(HttpStatusCode.Unauthorized,
                    "رمز التحديث غير صالح", "INVALID_REFRESH");

            if (lookup.ExpiresAt <= DateTime.Now)
                return BuildError(HttpStatusCode.Unauthorized,
                    "رمز التحديث منتهي الصلاحية", "INVALID_REFRESH");

            if (lookup.IsRevoked)
            {
                // Token-theft detection: someone tried to use a revoked refresh
                // token. Revoke every active token for this user.
                _refreshTokens.RevokeAllForUser(lookup.UserId, "Compromised");
                return BuildError(HttpStatusCode.Unauthorized,
                    "تم إلغاء جميع الجلسات لهذا المستخدم", "REUSE_DETECTED");
            }

            // Look up the user fresh so the new access token reflects the
            // current name / company / active state.
            var profile = LoadProfile(lookup.UserId, lookup.UserTypeId);
            if (profile == null)
            {
                _refreshTokens.Revoke(lookup.Id, "AccountMissing");
                return BuildError(HttpStatusCode.Unauthorized,
                    "تم إيقاف الحساب", "ACCOUNT_STOPPED");
            }

            // Rotate.
            _refreshTokens.Revoke(lookup.Id, "Rotated");
            string newPlain = _issuer.IssueRefreshToken();
            var stored = _refreshTokens.Store(lookup.UserId, lookup.UserTypeId, newPlain, lookup.Id);
            _refreshTokens.LinkReplacement(lookup.Id, stored.Id);

            var access = _issuer.Issue(profile.UserId, profile.UserTypeId, profile.CompanyId, profile.FullName);

            var body = new TokenResponse
            {
                AccessToken = access.Token,
                RefreshToken = newPlain,
                ExpiresIn = access.ExpiresInSeconds,
                User = new UserSummaryDto
                {
                    UserId = profile.UserId,
                    FullName = profile.FullName,
                    Role = profile.UserTypeId == (int)LoggedUserType.Company ? "Company" : "Employee",
                    CompanyId = profile.CompanyId,
                    CompanyName = profile.CompanyName,
                    IsStopped = false
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(body));
        }

        [HttpPost]
        [JwtAuthorize]
        public HttpResponseMessage Logout(LogoutRequest request)
        {
            if (request != null && !string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var lookup = _refreshTokens.FindByPlaintext(request.RefreshToken);
                if (lookup != null && !lookup.IsRevoked)
                    _refreshTokens.Revoke(lookup.Id, "Logout");
            }

            // Slice 6: if the mobile app passes its FCM device token, mark
            // it inactive so we stop pushing to a logged-out device.
            if (request != null && !string.IsNullOrWhiteSpace(request.DeviceToken)
                && MvcApplication.userData != null)
            {
                try
                {
                    new EtaskMinstry.Api.Services.DeviceTokenService()
                        .Unregister(MvcApplication.userData.userId, request.DeviceToken);
                }
                catch
                {
                    // Logout must always succeed even if token cleanup fails.
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(null, "تم تسجيل الخروج"));
        }

        private HttpResponseMessage BuildError(HttpStatusCode status, string message, string code)
        {
            return Request.CreateResponse(status, ApiResponse.Fail(message, code));
        }

        /// <summary>
        /// Re-loads name/companyId/active state for refresh-token rotation.
        /// Returns null if the underlying account no longer exists or is inactive.
        /// </summary>
        private AuthValidator.Result LoadProfile(int userId, int userTypeId)
        {
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            if (userTypeId == (int)LoggedUserType.Employee)
            {
                var emp = uow.Employee.Get(filter: e => e.EmpID == userId, includeProperties: "Company")
                                      .FirstOrDefault();
                if (emp == null || emp.IsActive != true || emp.IsDeleted == true) return null;
                if (emp.Company != null && (emp.Company.IsActive != true || emp.Company.IsDeleted == true))
                    return null;

                return new AuthValidator.Result
                {
                    UserId = userId,
                    UserTypeId = userTypeId,
                    CompanyId = emp.CompanyID,
                    FullName = emp.Name,
                    CompanyName = emp.Company != null ? emp.Company.Name : null
                };
            }

            if (userTypeId == (int)LoggedUserType.Company)
            {
                var company = uow.Company.Get().FirstOrDefault(c => c.CompanyID == userId);
                if (company == null || company.IsActive != true || company.IsDeleted == true) return null;
                return new AuthValidator.Result
                {
                    UserId = userId,
                    UserTypeId = userTypeId,
                    CompanyId = userId,
                    FullName = company.Name,
                    CompanyName = company.Name
                };
            }

            return null;
        }
    }
}
