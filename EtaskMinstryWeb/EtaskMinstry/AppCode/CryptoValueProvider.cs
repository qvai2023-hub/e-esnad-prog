using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;

namespace EtaskMinstry.AppCode
{

    #region original
    public class CryptoValueProvider : IValueProvider
    {
        RouteData routeData = null;
        Dictionary<string, string> dictionary = null;

        public CryptoValueProvider()
        {
        }
        public CryptoValueProvider(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public bool ContainsPrefix(string prefix)
        {
            if (this.routeData.Values["id"] == null)
            {
                return false;
            }
            //has problem as the query string values not arrived correctly to the action methods
            this.dictionary = Crypto.Decrypt(this.routeData.Values["id"].ToString());

            return this.dictionary.ContainsKey(prefix.ToUpper());
        }

        public ValueProviderResult GetValue(string key)
        {
            ValueProviderResult result = null;
            if (this.dictionary != null)
            {
                result = new ValueProviderResult(this.dictionary[key.ToUpper()],
                    this.dictionary[key.ToUpper()], CultureInfo.CurrentCulture);
            }
            return result;
        }

    }
    public static class Crypto
    {
        public static string Encrypt(Dictionary<string, string> keyValue)
        {
            // encrypt query string key value pair
            //Encryption.Encrypt(keyValue[(0));
            return "";
        }

        public static Dictionary<string, string> Decrypt(string encryptedText)
        {
            // decrypt encrypted query string into key value pair


            return null;
        }
    }
    public class CryptoValueProviderFactory : System.Web.Mvc.ValueProviderFactory
    {
        public override System.Web.Mvc.IValueProvider GetValueProvider(System.Web.Mvc.ControllerContext controllerContext)
        {

            return new CryptoValueProvider(controllerContext.RouteData);


        }
    }
    public class CryptoValueProviderAttribute : System.Web.Mvc.FilterAttribute, System.Web.Mvc.IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            //  var x = filterContext.Controller.ValueProvider.GetValue("s").AttemptedValue;
            ValueProviderCollection y = (ValueProviderCollection)filterContext.Controller.ValueProvider;
            // QueryStringValueProvider dddf = (QueryStringValueProvider)y[4];
            //   [3] = {System.Web.Mvc.QueryStringValueProvider}
            // NameValueCollectionValueProvider sss = (NameValueCollectionValueProvider)dddf;
            // var sssdd = (Dictionary<string, string>)sss;
            filterContext.Controller.ValueProvider = new CryptoValueProvider(filterContext.RouteData);
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //var queryStringQ = Server.UrlDecode(filterContext.HttpContext.Request.QueryString["q"]);
            //if (!string.IsNullOrEmpty(queryStringQ))
            //{
            //    // Decrypt query string value
            //    var queryParams = DecryptionMethod(queryStringQ);
            //}
            ////////////////////////////////
            

        }
      

    }
}


    #endregion

#region another exapmle

//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
//public class EncryptedActionParameterAttribute : System.Web.Mvc.ActionFilterAttribute
//{
//    public override void OnActionExecuting(ActionExecutingContext filterContext)
//    {

//        Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();
//        if (HttpContext.Current.Request.QueryString.Get("q") != null)
//        {
//            string encryptedQueryString = HttpContext.Current.Request.QueryString.Get("q");
//            string decrptedString = Decrypt(encryptedQueryString.ToString());
//            string[] paramsArrs = decrptedString.Split('?');

//            for (int i = 0; i < paramsArrs.Length; i++)
//            {
//                string[] paramArr = paramsArrs[i].Split('=');
//                decryptedParameters.Add(paramArr[0], Convert.ToInt32(paramArr[1]));
//            }
//        }
//        for (int i = 0; i < decryptedParameters.Count; i++)
//        {
//            filterContext.ActionParameters[decryptedParameters.Keys.ElementAt(i)] = decryptedParameters.Values.ElementAt(i);
//        }
//        base.OnActionExecuting(filterContext);

//    }

//    private string Decrypt(string encryptedText)
//    {
//        string key = "jdsg432387#";
//        byte[] DecryptKey = { };
//        byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
//        byte[] inputByte = new byte[encryptedText.Length];

//        DecryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
//        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
//        inputByte = Convert.FromBase64String(encryptedText);
//        MemoryStream ms = new MemoryStream();
//        CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(DecryptKey, IV), CryptoStreamMode.Write);
//        cs.Write(inputByte, 0, inputByte.Length);
//        cs.FlushFinalBlock();
//        System.Text.Encoding encoding = System.Text.Encoding.UTF8;
//        return encoding.GetString(ms.ToArray());
//    }
//}

#endregion
