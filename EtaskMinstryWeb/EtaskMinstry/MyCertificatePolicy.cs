using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EtaskMinstry
{


    public class MyCertificatePolicy
    {
        [ObsoleteAttribute("CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static ICertificatePolicy CertificatePolicy { get; set; }
    }


    public static class SSLValidator
    {
        private static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
                                                  SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public static void OverrideValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                OnValidateCertificate;
            ServicePointManager.Expect100Continue = true;
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public  string GetCertificateExpiry(string Url)
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://" + Url);
            WebResponse Response = Request.GetResponse();
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            return Request.ServicePoint.Certificate.GetExpirationDateString();
        }


    }


    //public static class WebRequest
    //{


    //    public bool CreateWebRequestObject(string Url)
    //    {
    //        try
    //        {
    //            var WebRequest = (HttpWebRequest)System.Net.WebRequest.Create(Url);

    //            if (this.IgnoreCertificateErrors)
    //                ServicePointManager.CertificatePolicy = delegate { return true; };
    //        }
    //    }

    //}
}
