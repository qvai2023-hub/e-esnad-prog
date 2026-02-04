using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using QvApi.Test.Etask;

namespace QvApi.Test
{
    [TestClass]
    public class EvaluationTest
    {
        EtaskClient client;
        public EvaluationTest()
        {
            PermissiveCertificatePolicy.Enact("CN=localhost");
            client = new EtaskClient();
        }
        [TestMethod]
        public void TeleworkersEvaluation()
        {
            var response = client.GetTeleworkersEvaluation(Convert.ToDateTime("2014-12-24"), DateTime.Today, "20FD1EDA-B518-4A90-B8FA-6ECED70C1862");
            Assert.IsTrue(response != null);
        }
    }

    class PermissiveCertificatePolicy
    {
        string subjectName;
        static PermissiveCertificatePolicy currentPolicy;
        PermissiveCertificatePolicy(string subjectName)
        {
            this.subjectName = subjectName;
            ServicePointManager.ServerCertificateValidationCallback +=
                new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertValidate);
        }

        public static void Enact(string subjectName)
        {
            currentPolicy = new PermissiveCertificatePolicy(subjectName);
        }

        bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            if (cert.Subject == subjectName)
            {
                return true;
            }

            return false;
        }
    }
}
