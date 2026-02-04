using EtaskMinstry.AppCode;
using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry;
using EtaskMinstry.AppCode;
using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace EtaskMinstry
{
    public class EmployeeAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = (UserData)MvcApplication.userData;
            if (user != null)
            { 
                user.isAuthorized = !user.isCompany && user.status ==  (int)userStatus.active && !user.isCompany && user.companyStatus == (int)userStatus.active;
               // return false;
                return user.isAuthorized;// return true if user is active and his data founded in current session
            }
            else if (ManageSecurityCookie.CheckCookie())
            {
                user = (UserData)MvcApplication.userData;
                user.isAuthorized = user.status == (int)userStatus.active && !user.isCompany;
                return user.isAuthorized;//return true if user is active and his data stored in cookie.
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// handel Unauthorized user
        /// eather redirect the user to the login page if he doesn,t logged in
        /// or to Unauthorized page if he has no permission over curren action.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new Http401Result();
            }
            else
            {
                var user = (UserData)MvcApplication.userData;
                if (user != null)
                {
                    if (!user.isCompany)
                    {
                        //if emoloyee or his Company not active or stoped return user to inActive page
                        //if (user  .status == (int)userStatus.stoped || user.status == (int)userStatus.notActive ||
                        //    user.companyStatus == (int)userStatus.stoped || user.companyStatus == (int)userStatus.notActive)
                        //{
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                            {
                                action = "StopedUser",
                                controller = "Security",
                                area = ""
                            }));
                        //}
                    }
                    else
                    {
                        base.HandleUnauthorizedRequest(filterContext);
                    }
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "Security",
                        area = ""
                    }));
                }
               
            }
        }


        private class Http401Result : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                // Set the response code to 401.
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Response.Write("AuthorizationLostPleaseLogOutAndLogInAgainToContinue");
                context.HttpContext.Response.End();
            }
        }
    }
}