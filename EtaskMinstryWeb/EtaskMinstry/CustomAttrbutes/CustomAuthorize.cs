using EtaskMinstry.AppCode;
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

namespace EtaskMinstry.CustomAttrbutes
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        { 
            var user = (UserData)MvcApplication.userData;
            if (user != null)
            {
                user.isAuthorized = user.status == (int)userStatus.active;
                return user.isAuthorized; //return true if user is active and his data founded in current session
            }
            else if (ManageSecurityCookie.CheckCookie())
            {
                user = (UserData)MvcApplication.userData;
                user.isAuthorized = user.status == (int)userStatus.active;
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
            var user = (UserData)MvcApplication.userData;
            if (user != null)
            {
                //if user is not active or stoped return user to inActive page.
                if (user.status == (int)userStatus.notActive || user.status == (int)userStatus.stoped ||
                    ((!user.isCompany) && ((user.companyStatus == (int)userStatus.notActive) || (user.companyStatus == (int)userStatus.stoped)))) 
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "StopedUser",
                        controller = "Security",
                        area = ""
                    }));
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
}