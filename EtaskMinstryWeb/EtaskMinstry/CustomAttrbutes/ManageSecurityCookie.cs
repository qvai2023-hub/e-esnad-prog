using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EtaskMinstry.CustomAttrbutes
{
    public class ManageSecurityCookie
    {
        /// <summary>
        /// Set user Name and user Password in Cookie
        /// to remember thr user data 
        /// in next requists.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pass"></param>
        public static void SetCookie(string userName, string pass)
        {
            HttpCookie aCookie = new HttpCookie("userData");
            aCookie.Values.Add("userName", QvLib.Security.DataProtection.Encrypt(userName));
            aCookie.Values.Add("password", QvLib.Security.DataProtection.Encrypt(pass));
            aCookie.Expires = DateTime.Now.AddDays(1);
            HttpContext.Current.Response.Cookies.Add(aCookie);
        }

        /// <summary>
        /// Check for user Cookie
        /// and fill the settion by
        /// the current user ata if he has valid cookie.
        /// </summary>
        /// <returns></returns>
        public static bool CheckCookie()
        {
            bool result = false;
            if (HttpContext.Current.Request.Cookies["userData"] != null)
            {
                string userName = QvLib.Security.DataProtection.Decrypt(HttpContext.Current.Request.Cookies["userData"]["userName"]);
                string password = QvLib.Security.DataProtection.Decrypt(HttpContext.Current.Request.Cookies["userData"]["password"]);
                
            }
            return result;
        }

        /// <summary>
        /// delete cookei.
        /// </summary>
        public static void DeleteCookei()
        {
            if (HttpContext.Current.Request.Cookies["userData"] != null)
            {
                HttpCookie myCookie = new HttpCookie("userData");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                HttpContext.Current.Response.Cookies.Add(myCookie);
            }

        
            if (HttpContext.Current.Request.Cookies["ASP.NET_SessionId"] != null)
            {
                // delete cookie "ASP.NET_SessionId"
                HttpCookie SessionCookie = new HttpCookie("ASP.NET_SessionId", "212");
                HttpContext.Current.Response.Cookies.Add(SessionCookie);
            }
            if (HttpContext.Current.Request.Cookies[".ASPXAUTH"] != null)
            {
                // delete cookie "ASP.NET_SessionId"
                HttpCookie SessionCookie = new HttpCookie(".ASPXAUTH", "2344");
                HttpContext.Current.Response.Cookies.Remove(".ASPXAUTH");
            }
         
        }
    }
}