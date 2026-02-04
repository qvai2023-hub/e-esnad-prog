using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EtaskMinstry.AppCode
{
    public static class QVSecurity
    {
        public static bool IsMatch(string strPaswordtoTest, string strHash)
        {
            bool isMatch = false;
            return isMatch;
        }

        public static String GetPasswordHash(string strPassword)
        {
            string strHash = string.Empty;
            return strHash;
        }

        public static string CalculateSHA1(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
        }

       public static string CalculateMD5(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MD5CryptoServiceProvider cryptoTransformMD5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformMD5.ComputeHash(buffer)).Replace("-", "");
        }

       public static string CalculateSHA256(string text)
       {
           byte[] buffer = Encoding.UTF8.GetBytes(text);
           SHA256CryptoServiceProvider cryptoTransformSHA256 = new SHA256CryptoServiceProvider();
           return BitConverter.ToString(cryptoTransformSHA256.ComputeHash(buffer)).Replace("-", "");
       }


        /// <summary>
        /// Function To get mac address of Computer 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="host"></param>
        /// <param name="mac"></param>
        /// <param name="length"></param>
        /// <returns></returns>
       [DllImport("Iphlpapi.dll")]
       private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
       [DllImport("Ws2_32.dll")]
       private static extern Int32 inet_addr(string ip);
       public static void GetMacAddress()
       {
           try
           {
               string userip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
               Int32 ldest = inet_addr(userip);
               Int32 lhost = inet_addr("");
               Int64 macinfo = new Int64();
               Int32 len = 6;
               int res = SendARP(ldest, 0, ref macinfo, ref len);
               string mac_src = macinfo.ToString("X");
               if (mac_src == "0")
               {
                   return;
               }

               while (mac_src.Length < 12)
               {
                   mac_src = mac_src.Insert(0, "0");
               }

               string mac_dest = "";

               for (int i = 0; i < 11; i++)
               {
                   if (0 == (i % 2))
                   {
                       if (i == 10)
                       {
                           mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                       }
                       else
                       {
                           mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                       }
                   }
               }
           }
           catch
           {  }
       }               
    }
}