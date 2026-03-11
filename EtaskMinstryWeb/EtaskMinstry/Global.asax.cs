using EtaskMinstry.AppCode;
using EtaskMinstry.Models;
using EtaskMinstry.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace EtaskMinstry
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
            }
        }
        public static string IpAddress
        {
            get
            {
                return GetUser_IP();
            }
        }

        public static string UserAgent
        {
            get
            {
                return GetUser_Agent();
            }
        }

        public static bool IsGregDate
        {
            get
            {
                if (int.Parse(System.Configuration.ConfigurationManager.AppSettings["ISGreg"].ToString()) == 1)
                {
                    return true;
                }
                else
                    return false;
            }
        }
        public static UserData userData
        {
            set
            {
                if (value != null)
                {
                    object[] sessionValues = { value, IpAddress, UserAgent };
                    HttpContext.Current.Session["User"] = sessionValues;
                }
                else
                    userData = value;
            }
            get
            {

                if (HttpContext.Current.Session["User"] != null)
                {

                    object[] sessionValues = (object[])HttpContext.Current.Session["User"];
                    return (UserData)sessionValues[0];
                }
                else
                    return null;

            }
        }
        protected static string GetUser_IP()
        {
            string VisitorsIPAddr = string.Empty;
            if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
            }
            return VisitorsIPAddr;
        }
        protected static string GetUser_Agent()
        {
            string UserAgent = string.Empty;
            if (HttpContext.Current.Request.UserAgent != null)
            {
                UserAgent = HttpContext.Current.Request.UserAgent;
            }
            return UserAgent;
        }
        protected void Application_Start()
        {
            //  ValueProviderFactories.Factories.Insert(0, new CryptoValueProviderFactory());
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Start the auto-checkout background job
            // This job runs every 15 minutes to close orphaned attendance sessions
            AutoCheckoutJob.Start();
        }

        protected void Application_End()
        {
            // Stop the auto-checkout background job
            AutoCheckoutJob.Stop();
        }

        /// <summary>
        /// This event is raised when ASP.NET runtime is ready to acquire the Session state of the current HTTP request.
        /// This event raised just before session-specific data is retrieved for the client and is used to populate Session Collection for current request.
        /// </summary>
        protected void Application_AcquireRequestState()
        {
            if (System.Web.HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session["User"] != null)
                {
                    object[] sessionValues = (object[])HttpContext.Current.Session["User"];

                    string sessionUserData = Convert.ToString(sessionValues[0]);
                    string sessionIpAddress = Convert.ToString(sessionValues[1]);
                    string sessionUserAgent = Convert.ToString(sessionValues[2]);
                    if (sessionUserData == null || sessionUserAgent != UserAgent)//hijacking attack attempt!
                    {
                        Session.RemoveAll();
                        Session.Clear();
                        Session.Abandon();
                        Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-30);
                        Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
                    }
                }
            }
        }
        void Session_Start(object sender, EventArgs e)
        {
            try
            {
                if (Request.IsSecureConnection == true)
                {
                    Response.Cookies["ASP.NET_SessionId"].Secure = true;
                }
            }
            catch (Exception)
            {
            }
        }
        public static void ConfigureAntiForgeryTokens()
        {
            // Rename the Anti-Forgery cookie from "__RequestVerificationToken" to "f".
            // This adds a little security through obscurity and also saves sending a
            // few characters over the wire.
            //AntiForgeryConfig.CookieName = "f";
            // If you have enabled SSL. Uncomment this line to ensure that the Anti-Forgery
            // cookie requires SSL to be sent accross the wire.
            //AntiForgeryConfig.RequireSsl = true;

        }
        protected void Application_BeginRequest()
        {
           // SSLValidator.OverrideValidation();
         //  var xx = SSLValidator.GetCertificateExpiry("proj.etask-proj.com");
            //switch (Request.Url.Scheme)
            //{
            //    case "https":
            //        Response.AddHeader("Strict-Transport-Security", "max-age=300");
            //        break;
            //    case "http":
            //        var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
            //        Response.Status = "301 Moved Permanently";
            //        Response.AddHeader("Location", path);
            //        break;
            //}

            ////switch (Request.Url.Scheme)
            ////{
            ////    case "https":
            ////        var path = "http://" + Request.Url.Host + Request.Url.PathAndQuery;
            ////        Response.Status = "301 Moved Permanently";
            ////        Response.AddHeader("Location", path);
            ////        break;
            ////    case "http":
            ////        //var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
            ////        //Response.Status = "301 Moved Permanently";
            ////        //Response.AddHeader("Location", path);
            ////        break;
            ////}


            // Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US");
            //if (HttpContext.Current.Request.IsSecureConnection.Equals(0) && HttpContext.Current.Request.IsLocal.Equals(0))
            //    Response.Redirect("https://" + Request.ServerVariables + HttpContext.Current.Request.RawUrl);

            //  var WebRequest = (HttpWebRequest)System.Net.WebRequest.Create("http://");
            //ServicePointManager.CertificatePolicy = (System.Net.ICertificatePolicy)new MyCertificatePolicy();
            //try
            //{
            //    WebRequest myRequest = WebRequest.Create("http://");
            //    WebResponse myResponse = myRequest.GetResponse();
            //    // ProcessResponse(myResponse);
            //    myResponse.Close();
            //}
            //// Catch any exceptions
            //catch (WebException e)
            //{
            //    if (e.Status == WebExceptionStatus.TrustFailure)
            //    {
            //        // Code for handling security certificate problems goes here.
            //    }
            //}

          
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();

            // Handle HTTP errors
            if (exc.GetType() == typeof(HttpException))
            {
                // The Complete Error Handling Example generates
                // some errors using URLs with "NoCatch" in them;
                // ignore these here to simulate what would happen
                // if a global.asax handler were not implemented.
                if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
                    return;
            }

            // For other kinds of errors give the user some information
            // but stay on the default page
            Response.Write("<h2>Global Page Error</h2>\n");
            Response.Write(
                "<p>" + exc.Message + "</p>\n");
            Response.Write("Return to the <a href='Default.aspx'>" +
                "Default Page</a>\n");

        }
        
    }
}