using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace EtaskMinstry
{
    public static class Extentions
    {
        public static string ToHijriArabicDate(this DateTime dt)
        {
            CultureInfo higri_format = new CultureInfo("ar-SA");
            higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
            return dt.ToString("ddd d MMM   yyyy", higri_format);
        }

        public static string ToGregArabicDate(this DateTime dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            return dt.ToString("ddd d MMM   yyyy", greg_format);
        }

        /// <summary>
        /// string date is date only without time
        /// </summary>
        /// <param name="stdate"></param>
        /// <returns></returns>
        public static string ToGregArabicDateOnly(this string stdate)
        {
            stdate = stdate.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");
           
            try
            {
                //DateTime dt = Convert.ToDateTime(stdate);
                DateTime dt = DateTime.ParseExact(stdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                higri_format.DateTimeFormat.Calendar = new GregorianCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// string date is date only without time
        /// </summary>
        /// <param name="stdate"></param>
        /// <returns></returns>
        public static string ToHijriArabicDateOnly(this string stdate)
        {
            stdate = stdate.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");

            try
            {
                //DateTime dt = Convert.ToDateTime(stdate);
                DateTime dt = DateTime.ParseExact(stdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch
            {
                return "";
            }
        }
        public static string ToGregArabicDate(this string stdate)
        {
            stdate = stdate.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");
            //DateTime dt = DateTime.ParseExact(sdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            try
            {
                DateTime dt = Convert.ToDateTime(stdate);
                higri_format.DateTimeFormat.Calendar = new GregorianCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch
            {
                return "";
            }
        }

        public static string ToHijriArabicDateTime(this DateTime dt)
        {
            CultureInfo higri_format = new CultureInfo("ar-SA");
            higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
            return dt.ToString("ddd d MMM   yyyy h:mm:ss tt", higri_format).Replace("AM", "صباحاً").Replace("PM", "مساءاً");
        }

        public static string ToGregArabicDateTime(this DateTime dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            return dt.ToString("ddd d MMM   yyyy h:mm:ss tt", greg_format).Replace("AM", "صباحاً").Replace("PM", "مساءاً");
        }
        public static string ToHijriArabicDate(this string sdt)
        {
            sdt = sdt.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");
            //DateTime dt = DateTime.ParseExact(sdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            try
            {
                DateTime dt = Convert.ToDateTime(sdt);
              //  dt.ToUniversalTime();
                higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch {
                return "";
            }
        }

        /// <summary>
        /// To Parse String From Hijri Date To Greg. Date In d/m/yyyy Format .
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToGregExact(this String dt)
        {
            return DateTime.ParseExact(dt.HijriToGregDate(), "d/M/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// To Parse String Greg Date To Greg. Date In d/m/yyyy Format .
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToGregExactformate(this String dt)
        {
            return DateTime.ParseExact(dt, "d/M/yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary> 
        /// To Convert Gerg. Date To Hijri Date in Format dd/M
        /// M/yyyy .
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Return String</returns>
        public static string ToHijriDate(this DateTime dt)
        {
            CultureInfo higri_format = new CultureInfo("ar-SA");
            higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
            return dt.ToString("dd/MM/yyyy", higri_format);
        }

        /// <summary> 
        /// To Convert Gerg. Date To Gerg Date in Format dd/M
        /// M/yyyy .
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>Return String</returns>
        public static string ToGregDate(this DateTime dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            //return dt.ToString("dd/MM/yyyy", greg_format);
            return dt.ToString("yyyy/MM/dd", greg_format);
        }

        public static string ToGregDate2(this DateTime? dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            //return dt.ToString("dd/MM/yyyy", greg_format);
            return dt.Value.ToString("yyyy/MM/dd", greg_format);
        }

        public static string ToGregDate3(this DateTime dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            return dt.ToString("dd/MM/yyyy", greg_format);
           // return dt.ToString("yyyy/MM/dd", greg_format);
        }
        public static string ToGregDatediff(this DateTime dt)
        {
            CultureInfo greg_format = new CultureInfo("ar-SA");
            greg_format.DateTimeFormat.Calendar = new GregorianCalendar();
            return dt.ToString("dd/MM/yyyy", greg_format);
            //return dt.ToString("yyyy/MM/dd", greg_format);
        }
        /// <summary>
        /// To Get Hijri Year .
        /// </summary>
        /// <param name="dt">Date</param>
        /// <returns> Year </returns>
        public static int GetHijriYear(this DateTime dt)
        {
            CultureInfo higri_format = new CultureInfo("ar-SA");
            higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
            return dt.ToString("yyyy", higri_format).IntParse();
        }

        public static string HijriToGregDate(this string value)
        {
            var umQra = new UmAlQuraCalendar();
            CultureInfo arCul = new CultureInfo("ar-SA");
            CultureInfo enCul = new CultureInfo("en-US");
            // string[] allFormats = { "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy" };
            arCul.DateTimeFormat.Calendar = umQra;

            if (string.IsNullOrEmpty(value))
                return null;

            DateTime tempDate = DateTime.ParseExact(value, "d/M/yyyy", arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);

            return tempDate.ToString("dd/MM/yyyy", enCul.DateTimeFormat);
        }

        public static DateTime? HijriToGregDates(this string value)
        {
            var umQra = new UmAlQuraCalendar();
            CultureInfo arCul = new CultureInfo("ar-SA");
            CultureInfo enCul = new CultureInfo("en-US");
            // string[] allFormats = { "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy" };
            arCul.DateTimeFormat.Calendar = umQra;

            if (string.IsNullOrEmpty(value))
                return null;

            DateTime tempDate = DateTime.ParseExact(value, "dd/MM/yyyy", arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);

            value = tempDate.ToString("dd/MM/yyyy", enCul.DateTimeFormat);


            if (string.IsNullOrEmpty(value))
                return null;

            string[] DateParts = value.Split('/'); // date format dd/MM/yyyy
            return new DateTime(int.Parse(DateParts[2]), int.Parse(DateParts[1]), int.Parse(DateParts[0]), new System.Globalization.GregorianCalendar());
        }
        /// <summary>
        /// convert to m/d/y
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        
        public static string ToGregDate(this string value)
        {
            var umQra = new UmAlQuraCalendar();
            CultureInfo arCul = new CultureInfo("ar-SA");
            CultureInfo enCul = new CultureInfo("en-US");
            // string[] allFormats = { "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy" };
            arCul.DateTimeFormat.Calendar = umQra;

            if (string.IsNullOrEmpty(value))
                return null;

            string[] splitedDate = value.Split('/');
            string temp = splitedDate[0];
            splitedDate[0] = splitedDate[1];
            splitedDate[1] = temp;

            DateTime tempDate = DateTime.ParseExact(string.Join("/",splitedDate), "M/d/yyyy", arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
            return tempDate.ToString("MM/dd/yyyy", enCul.DateTimeFormat);
        }

        /// <summary>
        /// try to parse string to int
        /// if parsing fail return zero
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IntParse(this string value)
        {
            int ret;
            int.TryParse(value, out ret);
            return ret;
        }

        public static T Clone<T>(this T value)
        {
            Type obj = value.GetType();
            object ret = Activator.CreateInstance(obj);

            PropertyInfo[] objProperties = obj.GetProperties();
            PropertyInfo[] retProperties = ret.GetType().GetProperties();

            for (int i = 0; i < objProperties.Length; i++)
			{
                retProperties[i].SetValue(ret, objProperties[i].GetValue(value));
			}

            return (T)ret;
        }

        public static string SubString(this string value, int Index)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length > Index)
                    result = value.Substring(0, Index) + "..";
                else
                    result = value;
            }
            return result;
        }

        //For Encrypt QueryString and RouteData
        public static string Encrypt(string plainText)
        {
            string key = "jdsg432387#";
            byte[] EncryptKey = { };
            byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
            EncryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByte = Encoding.UTF8.GetBytes(plainText);
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
            cStream.Write(inputByte, 0, inputByte.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray()).Replace('/', '_').Replace('+', '-');
        }
        //For Decrypt QueryString and RouteData
        public static string Decrypt(string encryptedText)
        {
            try
            {
                if (encryptedText.Contains("%"))
                {
                    encryptedText = encryptedText.Split('%')[0] + '=';
                }
                string key = "jdsg432387#";
                byte[] DecryptKey = { };
                byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
                byte[] inputByte = new byte[encryptedText.Length];
                encryptedText = encryptedText.Replace('_', '/').Replace('-', '+');
                DecryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByte = Convert.FromBase64String(encryptedText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(DecryptKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByte, 0, inputByte.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        //Compare Array To Validate Headers
        public static bool CompareArray(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }

            return true;
        }
        //Valid Headers
        public static Dictionary<string, byte[]> ValidHeaders(Dictionary<string, byte[]> FileHeader)
        {
            FileHeader.Add("JPG", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
            FileHeader.Add("JPEG", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
            FileHeader.Add("PNG", new byte[] { 0x89, 0x50, 0x4E, 0x47 });
            FileHeader.Add("PDF", new byte[] { 0x25, 0x50, 0x44, 0x46 });
            FileHeader.Add("DOC", new byte[] { 0xd0, 0xcf, 0x11, 0xe0 });
            FileHeader.Add("DOCX", new byte[] { 0x50, 0x4b, 0x03, 0x04 });
            FileHeader.Add("PPT", new byte[] { 0xd0, 0xcf, 0x11, 0xe0 });
            FileHeader.Add("PPTX", new byte[] { 0x50, 0x4b, 0x03, 0x04 });
            FileHeader.Add("PPS", new byte[] { 0xd0, 0xcf, 0x11, 0xe0 });
            FileHeader.Add("PPSX", new byte[] { 0x50, 0x4b, 0x03, 0x04 });
            FileHeader.Add("XLS", new byte[] { 0xd0, 0xcf, 0x11, 0xe0 });
            FileHeader.Add("XLSX", new byte[] { 0x50, 0x4b, 0x03, 0x04 });
            FileHeader.Add("MP4", new byte[] { 0x00, 0x00, 0x00, 0x20 });

            return FileHeader;
        }
        //Check Mime Type Server Side
        public static bool CheckMimeType(string MimeType)
        {
            if (ConfigurationManager.AppSettings["MimeTypes"].ToString().Contains(MimeType))
                return true;
            else
                return false;
        }

        //Validate ReCaptcha
        public static bool ValidateReCaptcha()
        {           
            string Response = HttpContext.Current.Request["g-recaptcha-response"];//Getting Response String Appned to Post Method

            bool Valid = false;
            //Request to Google Server 
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(" https://www.google.com/recaptcha/api/siteverify?secret=" + ConfigurationManager.AppSettings["RecaptchaSecretKey"] + "&response=" + Response);

            try
            {
                //Google recaptcha Responce 
                using (WebResponse wResponse = req.GetResponse())
                {

                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();
                        ReCahptchaResult data = js.Deserialize<ReCahptchaResult>(jsonResponse);// Deserialize Json 

                        Valid = Convert.ToBoolean(data.success);
                    }
                }

                return Valid;

            }
            catch (WebException ex)
            {
                throw ex;
            }


        }
        public class ReCahptchaResult
        {
            public string success { get; set; }

        }

        public static DateTime? GregDates(this string value)
        {
            var greg = new GregorianCalendar();
            CultureInfo arCul = new CultureInfo("ar-SA");
            CultureInfo enCul = new CultureInfo("en-US");
            // string[] allFormats = { "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy" };
            arCul.DateTimeFormat.Calendar = greg;

            if (string.IsNullOrEmpty(value))
                return null;

            DateTime tempDate = DateTime.ParseExact(value, "d/M/yyyy", arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);

            value = tempDate.ToString("dd/MM/yyyy", enCul.DateTimeFormat);


            if (string.IsNullOrEmpty(value))
                return null;

            string[] DateParts = value.Split('/'); // date format dd/MM/yyyy
            return new DateTime(int.Parse(DateParts[2]), int.Parse(DateParts[1]), int.Parse(DateParts[0]), new System.Globalization.GregorianCalendar());
        }
    
    
    }
}