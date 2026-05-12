using System;
using System.Configuration;
using System.Linq;
using EtaskMinstry.AppCode;
using TaskManagementModel;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Credentials + tenant + active-account validator for the Mobile API.
    ///
    /// Mirrors the EF queries inside Models/Login/UserAccountVM.cs::LoginETask
    /// WITHOUT writing to MvcApplication.userData (session). The API auth
    /// flow needs validation only — session population happens later in
    /// JwtAuthorizeAttribute on every authenticated request.
    ///
    /// Why a wrapper instead of overwriting LoginETask:
    ///   per CLAUDE.md "Mobile API Layer" → reuse, don't rewrite policy.
    ///   LoginETask has the side effect of mutating session + auto-checkin
    ///   which we want to control explicitly from AuthController.
    /// </summary>
    public class AuthValidator
    {
        public enum Outcome
        {
            Success,
            InvalidCredentials,
            AccountStopped,
            WrongApplication
        }

        public class Result
        {
            public Outcome Outcome { get; set; }
            public int UserId { get; set; }
            public int UserTypeId { get; set; }   // 2 = Employee, 3 = Company
            public int? CompanyId { get; set; }
            public string FullName { get; set; }
            public string CompanyName { get; set; }
        }

        public Result Validate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return new Result { Outcome = Outcome.InvalidCredentials };

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            string hashed = QvLib.Security.DataProtection.Encrypt(password);

            // Same predicate as LoginETask (exclude Admin)
            var user = uow.UserAccount.Get()
                .FirstOrDefault(u =>
                    u.UserName == username
                    && u.Password == hashed
                    && u.UserTypeID != (int)LoggedUserType.Admin);

            if (user == null)
                return new Result { Outcome = Outcome.InvalidCredentials };

            // Tenant gate (ApplicationName must match user.IsTelesak)
            bool? appIsTelesak = QvLib.Security.ApplicationSettings.IsApplicationTelesak();
            if (appIsTelesak == null)
                return new Result { Outcome = Outcome.WrongApplication };

            bool userIsTelesak = user.IsTelesak ?? false;
            if (appIsTelesak.Value != userIsTelesak)
                return new Result { Outcome = Outcome.WrongApplication };

            if (user.EmpID != null)
            {
                int empId = user.EmpID.Value;
                var emp = uow.Employee.Get(filter: e => e.EmpID == empId, includeProperties: "Company")
                                      .FirstOrDefault();
                if (emp == null || emp.IsActive != true || emp.IsDeleted == true)
                    return new Result { Outcome = Outcome.AccountStopped };

                if (emp.Company != null && (emp.Company.IsActive != true || emp.Company.IsDeleted == true))
                    return new Result { Outcome = Outcome.AccountStopped };

                return new Result
                {
                    Outcome = Outcome.Success,
                    UserId = empId,
                    UserTypeId = (int)LoggedUserType.Employee,
                    CompanyId = user.CompanyID,
                    FullName = emp.Name,
                    CompanyName = emp.Company != null ? emp.Company.Name : null
                };
            }

            // Company login
            int companyId = user.CompanyID ?? 0;
            var company = uow.Company.Get().FirstOrDefault(c => c.CompanyID == companyId);
            if (company == null || company.IsActive != true || company.IsDeleted == true)
                return new Result { Outcome = Outcome.AccountStopped };

            return new Result
            {
                Outcome = Outcome.Success,
                UserId = companyId,
                UserTypeId = (int)LoggedUserType.Company,
                CompanyId = companyId,
                FullName = company.Name,
                CompanyName = company.Name
            };
        }
    }
}
