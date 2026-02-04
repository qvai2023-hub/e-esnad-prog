using EtaskMinstry;
using EtaskMinstry.AppCode;
using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Models.Login;
using EtaskMinstry.AppCode;
using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TaskManagementModel;
using System.Web.SessionState;

namespace EtaskMinstry.Controllers
{

    //[RequireHttps]
    public class SecurityController : Controller
    {
        /// <summary>
        /// Get
        /// the opening login function.
        /// </summary> 
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult Login(string returnUrl = "")
        {
            //check if the user has session redirect to default page.
            if (MvcApplication.userData != null)
            {
                if (MvcApplication.userData.isAuthorized)
                {
                    if (MvcApplication.userData.isCompany)
                        return RedirectToAction("Index", "DashBoard", new { area = "Company" });
                    else
                        return RedirectToAction("Index", "DashBoardEmp", new { area = "Employee" });

                }
                else
                {
                    ModelState.AddModelError("LoginField", "اسم مستخدم او كلمة مرور غير صحيحة");
                    ViewBag.ReturnUrl = returnUrl;
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            if (returnUrl == "/Admin/Company")
                return RedirectToAction("AdminLogin");
            else
                return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="
        /// User"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM tempUser, string returnUrl = "")
        {

            if (ModelState.IsValid)
            {
                UserAccountVM user = new UserAccountVM();

                if (user.LoginETask(tempUser.UserName, tempUser.Password))
                {
               
                    //Clear return Url
                    //To enable Company to relogin again when typing an unauthorized page url.
                    //To enable Employee to relogin again when typing an unauthorized page url.
                    if ((returnUrl.ToLower().Contains("company") && MvcApplication.userData.isCompany == false)|| (returnUrl.ToLower().Contains("employee") && MvcApplication.userData.isCompany))
                    {
                        returnUrl = string.Empty;
                    }
                 
                    Generallog.LoginORLogout(ActionType.Login);
                    return Redirect(returnUrl != string.Empty ? returnUrl : MvcApplication.userData.isCompany ? "/Company/DashBoard" : "/Employee/DashBoardEmp");
                }
                else
                {
                    ModelState.AddModelError("LoginField", "اسم مستخدم او كلمة مرور غير صحيحة");
                }
            }
            else
            {
                ModelState.AddModelError("LoginField", " ");
            }

            return View();
        }

        /// <summary>
        /// GET For ADMIN Login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        /// 
        public ActionResult AdminLogin(string returnUrl = "")
        {

            //check if the user has session redirect to default page.
            if (MvcApplication.userData != null)
            {
                if (MvcApplication.userData.isAuthorized)
                {

                    return RedirectToAction("Index", "Company", new { area = "Admin" });
                }

                else
                {
                    ModelState.AddModelError("LoginField", "اسم مستخدم او كلمة مرور غير صحيحة");
                    ViewBag.ReturnUrl = returnUrl;
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST For ADMIN Login
        /// </summary>
        /// <param name="tempUser"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdminLogin( LoginVM tempUser, string returnUrl = "")
        {

            if (ModelState.IsValid)
            {
                UserAccountVM user = new UserAccountVM();
                if (user.LoginAdmin(tempUser.UserName, tempUser.Password))
                {
                    //log.
                    //Generallog.LoginORLogout(ActionType.Login);
                    return Redirect(returnUrl != string.Empty ? returnUrl : "/Admin/Company");
                }
                else
                {
                    ModelState.AddModelError("LoginField", "اسم مستخدم او كلمة مرور غير صحيحة");
                }
            }
            else
            {
                ModelState.AddModelError("LoginField", "اسم مستخدم او كلمة مرور غير صحيحة");
            }

            return View();
        }




        /// <summary>
        /// logout the user and emty his session.
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            if (@EtaskMinstry.MvcApplication.userData != null)
            {
                if ( @EtaskMinstry.MvcApplication.userData.UserTypeId == (int)LoggedUserType.Employee)
                {
                    UserAccountVM user = new UserAccountVM();
                    user.Checkout(@EtaskMinstry.MvcApplication.userData.userId);
                }

                //log--->logout
                Generallog.LoginORLogout(ActionType.LogOut);
                //MvcApplication.userData = null;
                FormsAuthentication.SignOut();

                //delete cookies
                ManageSecurityCookie.DeleteCookei();
                //delete seesion
                Session.Abandon();
                Session.Clear();
                //MvcApplication.userData = null;
            }
            return RedirectToAction("Login");
        }

        /// <summary>
        /// logout the user and emty his session.
        /// </summary>
        /// <returns></returns>
        public ActionResult AdminLogout()
        {
            //log--->logout
            //Generallog.LoginORLogout(ActionType.LogOut);
            //MvcApplication.userData = null;
            FormsAuthentication.SignOut();

            //delete cookies
            ManageSecurityCookie.DeleteCookei();
            //delete seesion
            Session.Abandon();
            Session.Clear();
            //MvcApplication.userData = null;
            return RedirectToAction("AdminLogin");
        }
        /// <summary>
        /// redirect to UnAuthorize page.
        /// </summary>
        /// <returns></returns>
        public ActionResult StopedUser()
        {
            return View();
        }


        [HttpPost]
        public Boolean UpdateConnectionID(string ConnectionID)
        {
            UnitOfWork _unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);
            if (MvcApplication.userData != null)
            {
                var userData = MvcApplication.userData;
                int userId = userData.userId;
                var SIGNAL_R_SESSION = _unitOfWork.SIGNAL_R_SESSIONs.Get(i => i.UserID == userId).FirstOrDefault();
                if (SIGNAL_R_SESSION == null)
                {
                    _unitOfWork.SIGNAL_R_SESSIONs.Insert(new SIGNAL_R_SESSION()
                    {
                        ConnectionID = ConnectionID,
                        UserTypeID = userData.isCompany ? (int)LoggedUserType.Company : (int)LoggedUserType.Employee,
                        UserID = userData.userId

                    });
                }
                else
                {
                    SIGNAL_R_SESSION.ConnectionID = ConnectionID;
                    _unitOfWork.SIGNAL_R_SESSIONs.Update(SIGNAL_R_SESSION);
                }
                _unitOfWork.Save();
                MvcApplication.userData.NotificationClientID = ConnectionID;
                MvcApplication.userData = MvcApplication.userData;
            }
            return true;
        }



        [HttpPost]
        public void UpdateNotification(int ID)
        {
            var userData = MvcApplication.userData;
            var usertype = MvcApplication.userData.isCompany ? LoggedUserType.Company : LoggedUserType.Employee;
            UnitOfWork _unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);
            var notifyObject = _unitOfWork.NotificationCollections.Get(i => i.Notification.ID == ID && i.UserTypeID == (int)usertype && !i.IsSeen && i.InstanceID == userData.userId).FirstOrDefault();
            if (notifyObject != null)
            {
                notifyObject.IsSeen = true;
                notifyObject.SeenDate = DateTime.Now;
                _unitOfWork.Save();
            }
        }

        /// <summary>
        /// Forget Password
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgetPasswoed()
        {
            return View();
        }

        /// <summary>
        /// Get Password.
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgetPasswoed(string Email)
        {
            var sent_result = new EtaskMinstry.Models.Login.Security().ForgetPawword(Email);
            ViewBag.DisplayBackButton = true;
            if (sent_result.HasValue)
            {
                if (sent_result.Value)
                {
                    ViewBag.DisplayBackButton = false;
                    ViewBag.Result = "<label style='color:Green;'> تم ارسال كلمة المرور , قم بفحص بريدك الإلكترونى </label>";
                }
                else
                    ViewBag.Result = "<label style='color:red;'> تعذر الارسال حاول تأكد من البريد الإلكترونى وحاول مرة اخرى </label>";
            }
            else
            {

                ViewBag.Result = "<label style='color:red;'>هذا البريد غير مسجل فى النظام </label>";
            }
            return View("ForgetPasswordConfirmation");
        }

        public ActionResult ChangePassword()
        {

            return View();
        }
        [HttpPost]
        public JsonResult ChangePassword(string OldPassword, string NewPassword)
        {
            UserAccountVM user = new UserAccountVM();

            return Json(user.ChangePassowrd(OldPassword, NewPassword));
        }
        public ActionResult NewAdminLogin()
        {
            return View();
        }
    }
}
