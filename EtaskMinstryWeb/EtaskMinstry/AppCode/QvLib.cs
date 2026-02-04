using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Mail;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using QvLib.Enumerators;
using System.Web.Hosting;
using QvLib.QVUtil;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Net.Mime;
using TaskManagementModel;






namespace QvLib
{
	namespace Menu
	{
		# region NavigationPath
		public class NavigationPath
		{

			private static Dictionary<string, string> links;

			/// <summary>
			/// Links of the navigation path <Link Text,Link Url>
			/// </summary>
			public static Dictionary<string, string> Links
			{
				get { return links; }
				set { links = value; }
			}
		}
		# endregion

	}

	namespace QVWeb
	{
		#region  Subscrib
		public class Subscrib
		{

			public static string GenrateConfrimLink(string link, string query, string queryKey)
			{
				string str = link + "?" + queryKey + "=" + StringOperations.Encrypt(query);
				return str;
			}
		}
		#endregion
	}

	namespace Enumerators
	{
		public enum ServiceResponse
		{
			MessageSent,
			MessageNotSent
		}
		public enum MediaType
		{
			Image,
			Movie,
			FLV,
			Flash,
			Audio,
			Real,
			Unknown,
		}
		public enum Lang { en, ar }
		public enum EditorType { Html, Text }
		public enum EditorTheme { advanced, simple }

		public enum ImageKind
		{
			Article = 0, Album = 1, News = 2, Sponser = 3, Media = 4, Chanel = 5, programlogo = 6, programBanner = 7, Shot = 8,
			ChanelBanner = 9
				, Games = 10, ProgramShot = 11, EpisodeShot = 12, Banner = 13, EpisodeLogo = 14, Interviewer = 15, InterviewerBanner = 16,
			Employee = 17, SoftWare = 18, Committee = 19, Mosque = 20, History = 21, Attestation = 22, TopArticle = 23, Link = 24,
			participant = 25, Exhipit = 26, Guest = 27, Question = 28

		}
		public enum ImageMouseKind
		{
			Blue = 0, Brown = 1, Gray = 2, Green = 3, Red = 4
		}


		public struct MimeTypes
		{
			public static List<string> Types
			{
				get
				{
					string[] types = { "application/envoy evy",
								 "application/fractals fif",
								 "application/futuresplash spl",
								 "application/hta hta",
								 "application/internet-property-stream acx",
								 "application/mac-binhex40 hqx",
								 "application/msword doc",
								 "application/msword dot",
								 "application/octet-stream *",
								 "application/octet-stream bin",
								 "application/octet-stream class",
								 "application/octet-stream dms",
								 "application/octet-stream exe",
								 "application/octet-stream lha",
								 "application/octet-stream lzh",
								 "application/oda oda",
								 "application/olescript axs",
								 "application/pdf pdf",
								 "application/pics-rules prf",
								 "application/pkcs10 p10",
								 "application/pkix-crl crl",
								 "application/postscript ai"
								 ,"application/postscript eps",
								 "application/postscript ps",
								 "application/rtf rtf","application/set-payment-initiation setpay",
								 "application/set-registration-initiation setreg",
								 "application/vnd.ms-excel xla","application/vnd.ms-excel xlc",
								 "application/vnd.ms-excel xlm","application/vnd.ms-excel xls",
								 "application/vnd.ms-excel xlt","application/vnd.ms-excel xlw",
								 "application/vnd.ms-outlook msg","application/vnd.ms-pkicertstore sst",
								 "application/vnd.ms-pkiseccat cat","application/vnd.ms-pkistl stl",
								 "application/vnd.ms-powerpoint pot","application/vnd.ms-powerpoint pps"
								 ,"application/vnd.ms-powerpoint ppt","application/vnd.ms-project mpp",
								 "application/vnd.ms-works wcm","application/vnd.ms-works wdb",
								 "application/vnd.ms-works wks","application/vnd.ms-works wps",
								 "application/winhlp hlp","application/x-bcpio bcpio",
								 "application/x-cdf cdf","application/x-compress z",
								 "application/x-compressed tgz","application/x-cpio cpio",
								 "application/x-csh csh","application/x-director dcr",
								 "application/x-director dir","application/x-director dxr",
								 "application/x-dvi dvi","application/x-gtar gtar",
								 "application/x-gzip gz","application/x-hdf hdf",
								 "application/x-internet-signup ins","application/x-internet-signup isp",
								 "application/x-iphone iii","application/x-javascript js",
								 "application/x-latex latex","application/x-msaccess mdb",
								 "application/x-mscardfile crd","application/x-msclip clp",
								 "application/x-msdownload dll","application/x-msmediaview m13",
								 "application/x-msmediaview m14","application/x-msmediaview mvb",
								 "application/x-msmetafile wmf","application/x-msmoney mny",
								 "application/x-mspublisher pub","application/x-msschedule scd",
								 "application/x-msterminal trm","application/x-mswrite wri",
								 "application/x-netcdf cdf","application/x-netcdf nc",
								 "application/x-perfmon pma","application/x-perfmon pmc",
								 "application/x-perfmon pml","application/x-perfmon pmr",
								 "application/x-perfmon pmw","application/x-pkcs12 p12",
								 "application/x-pkcs12 pfx","application/x-pkcs7-certificates p7b",
								 "application/x-pkcs7-certificates spc","application/x-pkcs7-certreqresp p7r",
								 "application/x-pkcs7-mime p7c","application/x-pkcs7-mime p7m",
								 "application/x-pkcs7-signature p7s","application/x-sh sh",
								 "application/x-shar shar","application/x-shockwave-flash swf",
								 "application/x-stuffit sit","application/x-sv4cpio sv4cpio",
								 "application/x-sv4crc sv4crc","application/x-tar tar",
								 "application/x-tcl tcl","application/x-tex tex","application/x-texinfo texi",
								 "application/x-texinfo texinfo","application/x-troff roff",
								 "application/x-troff t","application/x-troff tr",
								 "application/x-troff-man man","application/x-troff-me me",
								 "application/x-troff-ms ms","application/x-ustar ustar",
								 "application/x-wais-source src","application/x-x509-ca-cert cer",
								 "application/x-x509-ca-cert crt","application/x-x509-ca-cert der",
								 "application/ynd.ms-pkipko pko","application/zip zip","audio/basic au",
								 "audio/basic snd","audio/mid mid","audio/mid rmi","audio/mpeg mp3",
								 "audio/x-aiff aif","audio/x-aiff aifc","audio/x-aiff aiff",
								 "audio/x-mpegurl m3u","audio/x-pn-realaudio ra","audio/x-pn-realaudio ram",
								 "audio/x-wav wav","image/bmp bmp","image/cis-cod cod",
								 "image/gif gif","image/ief ief","image/jpeg jpe","image/jpeg jpeg",
								 "image/jpeg jpg","image/pipeg jfif","image/svg+xml svg","image/tiff tif",
								 "image/tiff tiff","image/x-cmu-raster ras","image/x-cmx cmx","image/x-icon ico",
								 "image/x-portable-anymap pnm","image/x-portable-bitmap pbm","image/x-portable-graymap pgm"
								 ,"image/x-portable-pixmap ppm","image/x-rgb rgb","image/x-xbitmap xbm","image/x-xpixmap xpm","image/x-xwindowdump xwd","message/rfc822 mht","message/rfc822 mhtml","message/rfc822 nws","text/css css","text/h323 323","text/html htm","text/html html","text/html stm","text/iuls uls","text/plain bas","text/plain c","text/plain h","text/plain txt","text/richtext rtx","text/scriptlet sct","text/tab-separated-values tsv","text/webviewhtml htt","text/x-component htc","text/x-setext etx","text/x-vcard vcf","video/mpeg mp2","video/mpeg mpa","video/mpeg mpe","video/mpeg mpeg","video/mpeg mpg","video/mpeg mpv2","video/quicktime mov","video/quicktime qt","video/x-la-asf lsf","video/x-la-asf lsx","video/x-ms-asf asf","video/x-ms-asf asr","video/x-ms-asf asx","video/x-msvideo avi","video/x-sgi-movie movie","x-world/x-vrml flr","x-world/x-vrml vrml","x-world/x-vrml wrl","x-world/x-vrml wrz","x-world/x-vrml xaf","x-world/x-vrml xof" };
					return new List<string>(types);
				}

			}
		}


	}

	namespace Structs
	{
		public struct KeyValue
		{
			public string strName;
			public string strID;
			public KeyValue(string ID, string Name)
			{
				strName = Name;
				strID = ID;

			}
		}
	}

	namespace SMS
	{



		# region ServiceProvider
		public abstract class ServiceProvider
		{
			protected string userName;

			public string UserName
			{
				get { return userName; }
				set { userName = value; }
			}

			protected string password;

			public string Password
			{
				get { return password; }
				set { password = value; }
			}
			protected string urlString;

			public string UrlString
			{
				get { return urlString; }
				set { urlString = value; }
			}
			protected int? port;
			public int? Port
			{
				get { return port; }
				set { port = value; }
			}
			protected string server;
			public string Server
			{
				get { return server; }
				set { server = value; }
			}
			protected string type;
			public string Type
			{
				get { return type; }
				set { type = value; }
			}
			protected string fromNumber;
			public string FromNumber
			{
				get { return fromNumber; }
				set { fromNumber = value; }
			}
			protected bool sendReport;
			public bool SendReport
			{
				get { return sendReport; }
				set { sendReport = value; }
			}
			private IEnumerable dataSource;

			public IEnumerable DataSource
			{
				get { return dataSource; }
				set { dataSource = value; }
			}

			private string phonePropertyName;

			public string PhonePropertyName
			{
				get { return phonePropertyName; }
				set { phonePropertyName = value; }
			}
			public static string stringToHex(string s)
			{
				string hex = "";
				foreach (char c in s)
				{
					int tmp = c;
					hex += String.Format("{0:x4}", Convert.ToUInt32(tmp.ToString()));
				}
				return hex;
			}
			public abstract List<string> SendSMS(string Message);
			//public abstract ServiceResponse SendBulkSMS(string Message);
			public abstract ServiceResponse SendSMS(string PhoneNumber, string Message);

			public abstract string SendSMSTest(string PhoneNumber, string Message);
			public abstract List<List<string>> SendBulkSMSTest(string Message, string smsTypr, string strLogPath, ref int iError, int iCount);
		}
		# endregion

		# region ClickatellProvider
		public class ClickatellProvider : ServiceProvider
		{
			public ClickatellProvider()
			{
				userName = "Hamada273";
				password = "4452145A";
				//Client ID : BAE190
			}
			public override List<List<string>> SendBulkSMSTest(string message, string smsTypr, string strLogPath, ref int iError, int iCount)
			{
				iError = 0;
				return new List<List<string>>();
			}
			public override string SendSMSTest(string PhoneNumber, string Message)
			{
				return "";
			}
			public override List<string> SendSMS(string Message)
			{
				List<string> lisStr = new List<string>();
				foreach (string strNum in DataSource)
				{
					WebClient client = new WebClient();
					// Add a user agent header in case the requested URI contains a query.
					client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR1.0.3705;)");
					client.QueryString.Add("user", userName);
					client.QueryString.Add("password", password);
					client.QueryString.Add("api_id", "3189666");
					client.QueryString.Add("to", strNum);
					client.QueryString.Add("text", Message);
					string baseurl = "http://api.clickatell.com/http/sendmsg";
					Stream data = client.OpenRead(baseurl);
					StreamReader reader = new StreamReader(data);
					string s = reader.ReadToEnd();
					data.Close();
					reader.Close();
					s = s.Split(':')[0];
					if (s != "ID")
					{
						lisStr.Add(strNum);
					}
				}
				return lisStr;
			}

			public override ServiceResponse SendSMS(string PhoneNumber, string Message)
			{
				WebClient client = new WebClient();
				// Add a user agent header in case the requested URI contains a query.
				client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR1.0.3705;)");
				client.QueryString.Add("user", userName);
				client.QueryString.Add("password", password);
				client.QueryString.Add("api_id", "3189666");
				client.QueryString.Add("to", PhoneNumber);
				client.QueryString.Add("text", Message);
				string baseurl = "http://api.clickatell.com/http/sendmsg";
				Stream data = client.OpenRead(baseurl);
				StreamReader reader = new StreamReader(data);
				string s = reader.ReadToEnd();
				data.Close();
				reader.Close();
				s = s.Split(':')[0];
				if (s == "ID")
				{

					return ServiceResponse.MessageSent;
				}
				return ServiceResponse.MessageNotSent;
			}
		}
		#endregion

		# region BulksmsProvider
		public class BulksmsProvider : ServiceProvider
		{


			public BulksmsProvider()
			{
				//userName = "DMF";
				userName = "bela";
				password = "123456";
			}

			public override List<List<string>> SendBulkSMSTest(string Message, string smsTypr, string strLogPath, ref int iError, int iCount)
			{
				iError = 0;
				return new List<List<string>>();
			}
			public override string SendSMSTest(string PhoneNumber, string Message)
			{
				return "";
			}
			public override List<string> SendSMS(string Message)
			{
				List<string> lisStr = new List<string>();
				String PhoneNumbers = String.Empty;


				foreach (string strNum in DataSource)
				{
					PhoneNumbers
						+= "," + strNum;
				}
				PhoneNumbers = PhoneNumbers.Remove(0, 1);
				//return lisStr;



				UriBuilder urlBuilder = new UriBuilder();


				urlBuilder.Scheme = "http";
				urlBuilder.Host = "usa.bulksms.com";
				urlBuilder.Port = 5567;
				urlBuilder.Path = "/eapi/submission/send_sms/2/2.0";


				string data = "";
				data += "username=" + HttpUtility.UrlEncode(userName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&sender=BulkSms";
				data += "&message=" + stringToHex(Message);
				//UniCode
				data += "&dca=16bit";
				data += "&want_report=1";
				data += "&msisdn=" + PhoneNumbers;


				urlBuilder.Query = string.Format(data);
				try
				{
					var httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
					httpReq.Method = "POST";
					var httpResponse = (HttpWebResponse)(httpReq.GetResponse());


					var input = new StreamReader(httpResponse.GetResponseStream());

					String res = input.ReadToEnd();

					String[] parts = res.Split('|');

					String statusCode = parts[0];
					String statusString = parts[1];


					if (!statusCode.Equals("0"))
					{

					}
					else
					{
						var s = parts[2];

					}
					httpResponse.Close();


				}
				catch //(WebException e)
				{

				}


				return lisStr;
			}

			public override ServiceResponse SendSMS(string PhoneNumber, string Message)
			{


				UriBuilder urlBuilder = new UriBuilder();
				ServiceResponse returnValue;

				urlBuilder.Scheme = "http";
				urlBuilder.Host = "usa.bulksms.com";
				urlBuilder.Port = 5567;
				urlBuilder.Path = "/eapi/submission/send_sms/2/2.0";


				string data = "";
				data += "username=" + HttpUtility.UrlEncode(userName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&sender=BulkSms";
				data += "&message=" + stringToHex(Message);
				//UniCode
				data += "&dca=16bit";
				data += "&want_report=1";
				data += "&msisdn=" + PhoneNumber;


				urlBuilder.Query = string.Format(data);
				try
				{
					HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
					httpReq.Method = "POST";
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());


					StreamReader input = new StreamReader(httpResponse.GetResponseStream());

					String res = input.ReadToEnd();

					String[] parts = res.Split('|');

					String statusCode = parts[0];
					String statusString = parts[1];
					httpResponse.Close();

					if (!statusCode.Equals("0"))
					{
						returnValue = ServiceResponse.MessageNotSent;
					}
					else
					{
						returnValue = ServiceResponse.MessageSent;
					}



				}
				catch //(WebException e)
				{
					returnValue = ServiceResponse.MessageNotSent;
				}

				return returnValue;


			}

		}

		# endregion

		# region RouteSmsServiceProvider
		public class RouteSmsServiceProvider : ServiceProvider
		{
			public RouteSmsServiceProvider()
			{
				userName = "moltqa";
				password = "abdel";

			}
			# region GetErrorMessage
			private string GetMessage(string errorNumber)
			{
				string sOut = "";
				switch (errorNumber)
				{
					//Error In SMS URL 
					case "1":
						sOut = "خطأ غير معروف من فضلك راجع المدير";
						break;
					//Insufficient Credit 
					case "1025":
						sOut = "الرصيد غير كافي";
						break;
					//Internal Error
					case "1710":
						sOut = "خطأداخلى فى الخادم";
						break;
					//User validation failed
					case "1709":
						sOut = "بيانات خطأ";
						break;
					//Invalid value for DLR field
					case "1708":
						sOut = "تقرير البيانات خطأ";
						break;
					//Invalid Source (Sender)
					case "1707":
						sOut = "رقم الإرسال غير صحيح";
						break;
					// Invalid Destination
					case "1706":
						sOut = "الرقم المرسل له غير صحيح";
						break;
					//Invalid Message
					case "1705":
						sOut = "خطأ فى الرسالة";
						break;
					//Invalid value in type field
					case "1704":
						sOut = "خطأ فى نوع الرسالة";
						break;
					//Invalid value in username or password field
					case "1703":
						sOut = "اسم المستخدم أو كلمة المرور غير صحيحة";
						break;
					// Invalid URl Error
					case "1702":
						sOut = "رابط الإرسال غير صحيح";
						break;
					//Success
					case "1701":
						sOut = "تم الإرسال بنجاح";
						break;
					default:
						break;
				}

				return sOut;
			}

			public string GetBalance()
			{
				try
				{
					UriBuilder urlBuilder = new UriBuilder();
					urlBuilder.Scheme = "http";
					urlBuilder.Host = Server == null ? "smpp1.routesms.com" : Server;
					urlBuilder.Port = Port.HasValue ? 8000 : Port.Value;
					if (Server.ToLower().Contains("smpp5"))
					{
						urlBuilder.Path = "/LabrocServlet/fetchcredits";
					}
					else
					{
						urlBuilder.Path = "/CreditCheck/checkcredits";
					}

					string data = "";
					data += "username=" + HttpUtility.UrlEncode(UserName == null ? userName : UserName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
					data += "&password=" + HttpUtility.UrlEncode(Password == null ? password : Password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
					urlBuilder.Query = string.Format(data);
					HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
					httpReq.Method = "POST";
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());

					string file = input.ReadToEnd().ToLower();
					file = file.Split(':')[1];

					return file;
				}
				catch (Exception ex)
				{
					return "-1";
				}
			}

			# endregion

			public override List<string> SendSMS(string Message)
			{
				List<string> lisStr = new List<string>();
				foreach (string strNum in DataSource)
				{
					try
					{

						WebClient client = new WebClient();
						StreamReader sr = new StreamReader(client.OpenRead(" http://smpp1.routesms.com:8080/bulksms/" + UrlString + "?username=" + userName + "&password=" + password + "&type=2&dlr=0&destination=%2B{PhoneNumber}&source=91234567891&message={Message}".Replace("{PhoneNumber}", strNum).Replace("{Message}", stringToHex(Message))));
						string file = sr.ReadToEnd().ToLower();
						file = file.Split('|')[0];
						if (file != "1701")
						{
							lisStr.Add(strNum);
						}
					}
					catch
					{
						lisStr.Add(strNum);
					}
				}
				return lisStr;
			}

			public override ServiceResponse SendSMS(string PhoneNumber, string Message)
			{
				UriBuilder urlBuilder = new UriBuilder();
				urlBuilder.Scheme = "http";
				urlBuilder.Host = Server ?? "smpp1.routesms.com";
				urlBuilder.Port = Port.HasValue ? Port.Value : 8000;
				urlBuilder.Path = "/bulksms/" + UrlString;
				string data = "";
				data += "username=" + HttpUtility.UrlEncode(UserName == null ? userName : UserName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(Password == null ? password : Password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&source=" + FromNumber == null ? "920105794064" : FromNumber;
				data += "&message=" + stringToHex(Message);
				//UniCode
				data += "&type=" + Type == null ? "2" : Type;
				data += "&dlr=" + (SendReport ? "1" : "0");
				data += "&destination=" + PhoneNumber;
				urlBuilder.Query = string.Format(data);
				HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
				httpReq.Method = "POST";
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());

					string file = input.ReadToEnd().ToLower();
					file = file.Split('|')[0];

					if (file == "1701")
					{
						return ServiceResponse.MessageSent;
					}
				}
				catch
				{ }

				return ServiceResponse.MessageNotSent;
			}

			public override string SendSMSTest(string PhoneNumber, string Message)
			{


				UriBuilder urlBuilder = new UriBuilder();
				urlBuilder.Scheme = "http";
				urlBuilder.Host = Server ?? "smpp1.routesms.com";
				urlBuilder.Port = Port.HasValue ? Port.Value : 8000;
				urlBuilder.Path = "/bulksms/" + UrlString;
				string data = "";
				data += "username=" + HttpUtility.UrlEncode(userName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&type=" + (Type == null ? "2" : Type);
				data += "&dlr=" + (SendReport ? "1" : "0");
				data += "&destination=" + PhoneNumber;
				data += "&source=" + (FromNumber ?? "920105794064");
				data += "&message=" + stringToHex(Message);
				//UniCode



				urlBuilder.Query = string.Format(data);
				HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
				httpReq.Method = "POST";
				string sOut = "";
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());
					string file = input.ReadToEnd().ToLower();
					file = file.Split('|')[0];
					sOut = GetMessage(file);
				}
				catch
				{
					sOut = GetMessage("1");
				}
				return sOut;
			}
			public int SendSMSTestErrorNo(string PhoneNumber, string Message)
			{


				UriBuilder urlBuilder = new UriBuilder();
				urlBuilder.Scheme = "http";
				urlBuilder.Host = Server ?? "smpp1.routesms.com";
				urlBuilder.Port = Port.HasValue ? Port.Value : 8000;
				urlBuilder.Path = "/bulksms/" + UrlString;
				string data = "";
				data += "username=" + HttpUtility.UrlEncode(userName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&type=" + (Type == null ? "2" : Type);
				data += "&dlr=" + (SendReport ? "1" : "0");
				data += "&destination=" + PhoneNumber;
				data += "&source=" + (FromNumber ?? "920105794064");
				data += "&message=" + stringToHex(Message);
				//UniCode



				urlBuilder.Query = string.Format(data);
				HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
				httpReq.Method = "POST";
				int sOut = -1;
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());
					string file = input.ReadToEnd().ToLower();
					file = file.Split('|')[0];
					sOut = Convert.ToInt32(file);
				}
				catch
				{
					sOut = Convert.ToInt32("1");
				}
				return sOut;
			}

			public List<string> SendBulkSMS(string Message, string Type)
			{
				List<string> lisStr = new List<string>();
				String PhoneNumbers = String.Empty;


				foreach (string strNum in DataSource)
				{
					PhoneNumbers
						+= "," + strNum;
				}
				PhoneNumbers = PhoneNumbers.Remove(0, 1);


				UriBuilder urlBuilder = new UriBuilder();
				urlBuilder.Scheme = "http";
				urlBuilder.Host = "smpp1.routesms.com";
				urlBuilder.Port = (Port.HasValue) ? 8000 : Port.Value;
				urlBuilder.Path = "/bulksms/" + UrlString;
				string data = "";
				data += "username=" + HttpUtility.UrlEncode(UserName == null ? userName : UserName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(Password == null ? password : Password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&source=92" + (FromNumber ?? "920105794064") + "&dlr=0";
				data += "&message=" + (Type == "0" ? Message : stringToHex(Message));
				//UniCode
				data += "&type=" + Type;
				data += "&destination=" + PhoneNumbers;
				urlBuilder.Query = string.Format(data);
				HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString(), false));
				httpReq.Method = "POST";
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());

					string file = input.ReadToEnd().ToLower();
					string[] massage = file.Split(',');
					for (int i = 0; i < massage.Length; i++)
					{
						if (!massage[i].Contains("1701"))
						{

							lisStr.Add(massage[i].Substring(massage[i].IndexOf('|') + 1));
						}
					}
				}
				catch
				{
					lisStr.Add(GetMessage("1"));
				}

				return lisStr;
			}

			public override List<List<string>> SendBulkSMSTest(string Message, string smsTypr, string strLogPath, ref int iError, int iCount)
			{

				String PhoneNumbers = String.Empty;

				iError = 0;
				foreach (string strNum in DataSource)
				{
					PhoneNumbers
						+= "," + strNum;
				}
				PhoneNumbers = PhoneNumbers.Remove(0, 1);


				UriBuilder urlBuilder = new UriBuilder();
				urlBuilder.Scheme = "http";
				urlBuilder.Host = Server ?? "smpp1.routesms.com";
				urlBuilder.Port = Port.HasValue ? Port.Value : 8000;
				urlBuilder.Path = "/bulksms/" + UrlString;
				string data = "";
				data += "username=" + HttpUtility.UrlEncode(UserName ?? userName, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&password=" + HttpUtility.UrlEncode(Password ?? password, System.Text.Encoding.GetEncoding("ISO-8859-1"));
				data += "&source=" + (FromNumber ?? "920105794064");
				data += "&dlr=" + (SendReport ? "1" : "0");
				data += "&message=" + stringToHex(Message);
				//UniCode
				data += "&type=" + (smsTypr ?? "2");
				data += "&destination=" + PhoneNumbers;
				urlBuilder.Query = string.Format(data);
				HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(urlBuilder.ToString()));
				httpReq.Method = "POST";
				List<List<string>> listStrFail = new List<List<string>>();
				List<string> lstStr = new List<string>();
				try
				{
					HttpWebResponse httpResponse = (HttpWebResponse)(httpReq.GetResponse());
					StreamReader input = new StreamReader(httpResponse.GetResponseStream());

					string file = input.ReadToEnd().ToLower();
					string[] massage = file.Split(',');

					IO.Logme("     ", strLogPath);

					for (int i = 0; i < massage.Length; i++)
					{
						IO.Logme(massage[i] + " | " + iCount, strLogPath);
						if (!massage[i].Split('|')[0].Contains("1701"))
						{
							lstStr.Add(massage[i].Split('|')[1].Split(':')[0]);
							lstStr.Add(GetMessage(massage[i].Split('|')[0]));
							if (massage[i].Split('|')[0].Contains("1025"))
							{
								iError = 1025;
							}
							listStrFail.Add(lstStr);
						}
					}
				}
				catch
				{
					listStrFail.Add(new List<string> { GetMessage("1") });
				}

				return listStrFail;
			}

		}
		# endregion
	}

	namespace Extenstions
	{
		# region ObjectUtil
		public static class ObjectUtil
		{


			public static object CreateInstance(string objectName, params object[] constructorArgs)
			{
				Type ObjectType = Type.GetType(objectName);

				return ObjectType.GetConstructor(new Type[] { }).Invoke(constructorArgs);
			}

			public static Type GetType(string objectName)
			{
				return Type.GetType(objectName);
			}


			public static System.Object GetObjectProperty(string mFullPath)
			{
				object Target = null;

				foreach (string partition in mFullPath.Split('\\'))
				{
					if (Target == null)
					{
						Target = CreateInstance(partition, new object[] { });
					}
					else
					{
						Target = Target.GetPropertyByName(partition);

					}
				}
				return Target;
			}

		}
		# endregion

		# region Property
		public class Property
		{
			private string name;

			public string Name
			{
				get { return name; }
				set { name = value; }
			}
			private object value;

			public object Value
			{
				get { return this.value; }
				set { this.value = value; }
			}

			private bool isReadOnly;

			public bool IsReadOnly
			{
				get { return isReadOnly; }
				set { isReadOnly = value; }
			}
		}
		# endregion

		# region ObjectExtention
		public static class ObjectExtention
		{
			public static T GetRandom<T>(this IList<T> list)
			{

				if (list.Count == 0)
				{
					return default(T);
				}
				return list[new Random().Next(0, list.Count)];
			}

			public static MediaType GetType(string FileName)
			{
				string extension = FileName.Substring(FileName.LastIndexOf('.') + 1).ToLower();
				foreach (var item in Enum.GetNames(typeof(MediaType)))
				{
					if (ConfigurationManager.AppSettings[item + "Extensions"] != null && ConfigurationManager.AppSettings[item + "Extensions"].ToLower().Contains(extension))
					{
						return (MediaType)Enum.Parse(typeof(MediaType), item);
					}
				}
				return MediaType.Unknown;
			}
			public static Object GetPropertyByName(this Object mObject, string mPropertyName)
			{
				Type MyType = mObject.GetType();
				return MyType.GetProperty(mPropertyName.Trim()).GetValue(mObject, new object[] { });
			}
			public static Object GetObjectInRel(this Object mObject, string mPropertyName)
			{
				if (mPropertyName.Contains('.'))
				{
					object nObj = mObject;
					foreach (string str in mPropertyName.Split('.'))
					{
						nObj = nObj.GetPropertyByName(str);
					}
					return nObj;
				}
				else
				{
					return mObject.GetPropertyByName(mPropertyName);
				}
			}
			public static List<Extenstions.Property> GetProperties(this Object mObject)
			{
				Type MyType = mObject.GetType();
				var properties = new List<Extenstions.Property>();
				var allproperties = MyType.GetProperties();
				foreach (var property in allproperties)
				{
					properties.Add(new Extenstions.Property() { Name = property.Name, Value = mObject.GetPropertyByName(property.Name), IsReadOnly = !property.CanWrite });
				}
				return properties;
			}

			public static void SetPropertyByName(this Object mObject, string mPropertyName, Object mValue)
			{
				Type MyType = mObject.GetType();
				MyType.GetProperty(mPropertyName).SetValue(mObject, mValue, new object[] { });
			}


			public static void SetPropertyByNameWithTypeCheck(this Object mObject, string mPropertyName, Object mValue)
			{
				mObject.SetPropertyByName(mPropertyName, mObject.GetType().GetProperty(mPropertyName).PropertyType.MakeThisType(mValue));
			}

			public static object CallMethodByName(this object mObject, string MethodName, params object[] argsRest)
			{

				List<Type> mTypes = new List<Type>();
				foreach (Object Param in argsRest)
				{
					mTypes.Add(Param.GetType());
				}


				Type MyType = mObject.GetType();
				MethodInfo Mi = MyType.GetMethod(MethodName, mTypes.ToArray());
				return Mi.Invoke(mObject, argsRest);
			}

			public static object CallMethodByName(this Type mType, string MethodName, params object[] argsRest)
			{

				List<Type> mTypes = new List<Type>();
				foreach (Object Param in argsRest)
				{
					mTypes.Add(Param.GetType());
				}


				MethodInfo Mi = mType.GetMethod(MethodName, mTypes.ToArray());
				return Mi.Invoke(null, argsRest);
			}

			private static object MakeThisType(this Type mType, object Other)
			{
				if (string.IsNullOrEmpty(Other.ToString()))
				{
					return null;
				}
				if (mType.IsGenericType)
				{
					mType = Nullable.GetUnderlyingType(mType);
				}
				if (mType.Name == "Guid")
				{
					return new Guid(Other.ToString());
				}
				return Convert.ChangeType(Other, mType);
			}

			public static IList<object> AsGenericIList(this object obj)
			{
				List<object> result = new List<object>();
				foreach (object row in (obj as IEnumerable))
				{
					result.Add(row);
				}
				return result;
			}

			public static bool HasPropertyOfName(this object mObject, string mPropertyName)
			{
				Type MyType = mObject.GetType();
				return MyType.GetProperty(mPropertyName) != null;
			}



			public static object As(this object mObject, Type mType)
			{
				object result = mType.GetConstructor(new Type[] { }).Invoke(new object[] { });
				foreach (var property in result.GetProperties())
				{
					if (mObject.HasPropertyOfName(property.Name))
					{
						result.SetPropertyByName(property.Name, mObject.GetPropertyByName(property.Name));
					}
				}
				return result;
			}

			public static void MakePropertiesEqualTo(this object mObject, object obj)
			{
				foreach (var property in mObject.GetProperties())
				{
					try
					{
						if (obj.HasPropertyOfName(property.Name) && mObject.HasPropertyOfName(property.Name) && !property.IsReadOnly && mObject.GetType().GetProperty(property.Name).CanWrite)
						{
							mObject.SetPropertyByName(property.Name, obj.GetPropertyByName(property.Name));
						}
					}
					catch
					{
						continue;
					}
				}
			}

		}
		# endregion

		# region StringExtention
		public static class StringExtention
		{

			public static string SupName(this String str, int i)
			{
				int istr = str.Length;
				if (istr > i)
				{
					str = str.Substring(0, i) + "..";
				}
				return str;
			}

			public static string SupNameWithSpace(this String str, int i)
			{
				int istr = str.Length;
				if (istr > i)
				{
					str = str.Substring(0, str.Substring(0, i).LastIndexOf(" ")) + " ...";
				}
				return str;
			}

			public static string SupNameSpaceNo(this String str, int i)
			{
				string[] lst = str.Trim().Replace("   ", " ").Replace("  ", " ").Split(' ');
				int length = 0;
				if (lst.Length  > i)
				{

					for (int y = 0; y <i ;y++ )
					{
						length += lst[y].Length + 1;
					}
					str = str.Substring(0,  length) + " ...";
				}
				return str;
			}

			public static string ToString(this NameValueCollection values)
			{
				string result = string.Empty;
				foreach (var key in values.AllKeys)
				{
					result += key + "=" + values[key];
				}
				return result;
			}
			public static string ToSubString(this string value, int Index)
			{
				string result = string.Empty;
				if (value.Length > Index)
					result = value.Substring(0, Index) + "..";
				else
					result = value + "...";
				return result;
			}


			public static string Multiply(this string mObject, int times)
			{
				string result = "";
				for (int i = 0; i < times; i++)
				{
					result += mObject;
				}
				return result;
			}

			public static string ReplacePlaceHolders(this string mObject, object obj)
			{
				if (obj != null)
				{
					foreach (var property in obj.GetProperties())
					{
						if (mObject.Contains(property.Name))
						{
							if (property.Value == null)
							{
								mObject = mObject.Replace("{" + property.Name + "}", string.Empty);
							}
							else
							{
								mObject = mObject.Replace("{" + property.Name + "}", property.Value.ToString());
							}
						}
					}
				}
				return mObject;
			}
		}
		# endregion



		# region JSONHelper
		public static class JSONHelper
		{
			public static string ToJSON(this object obj)
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer();

				return serializer.Serialize(obj);
			}

			public static object JSONToObject<ObjectType>(string strObject)
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer();

				return serializer.Deserialize<ObjectType>(strObject);
			}

			public static string ToJSON(this object obj, int recursionDepth)
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				serializer.RecursionLimit = recursionDepth;
				return serializer.Serialize(obj);
			}
		}

		# endregion

	}

	namespace QVUtil
	{


		# region String
		public class StringOperations
		{
			public static string Encrypt(string Str)
			{
				string serial = Str;
				StringBuilder st = new StringBuilder();
				for (int i = 0; i < serial.Length; i++)
				{
					st.Append(Convert.ToInt32(serial[i]).ToString() + "-");
				}
				string str = st.ToString();
				return str.Remove(str.LastIndexOf('-'));
			}
			public static string Decrypt(string str)
			{
				string serial = str;
				string[] arString = str.Split('-');
				StringBuilder st = new StringBuilder();
				for (int i = 0; i < arString.Length; i++)
				{
					int iCast = int.Parse(arString[i]);
					char chStr = (char)iCast;
					st.Append(chStr);
				}
				return st.ToString();
			}


			public static string StripHTML(string source)
			{
				try
				{
					string result;

					// Remove HTML Development formatting
					// Replace line breaks with space
					// because browsers inserts space
					result = source.Replace("\r", " ");
					// Replace line breaks with space
					// because browsers inserts space
					result = result.Replace("\n", " ");
					// Remove step-formatting
					result = result.Replace("\t", string.Empty);
					// Remove repeating spaces because browsers ignore them
					result = Regex.Replace(result,
																		  @"( )+", " ");

					// Remove the header (prepare first by clearing attributes)
					result = Regex.Replace(result,
							 @"<( )*head([^>])*>", "<head>",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"(<( )*(/)( )*head( )*>)", "</head>",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 "(<head>).*(</head>)", string.Empty,
							 RegexOptions.IgnoreCase);

					// remove all scripts (prepare first by clearing attributes)
					result = Regex.Replace(result,
							 @"<( )*script([^>])*>", "<script>",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"(<( )*(/)( )*script( )*>)", "</script>",
							 RegexOptions.IgnoreCase);
					//result = System.Text.RegularExpressions.Regex.Replace(result,
					//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
					//         string.Empty,
					//         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"(<script>).*(</script>)", string.Empty,
							 RegexOptions.IgnoreCase);

					// remove all styles (prepare first by clearing attributes)
					result = Regex.Replace(result,
							 @"<( )*style([^>])*>", "<style>",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"(<( )*(/)( )*style( )*>)", "</style>",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 "(<style>).*(</style>)", string.Empty,
							 RegexOptions.IgnoreCase);

					// insert tabs in spaces of <td> tags
					result = Regex.Replace(result,
							 @"<( )*td([^>])*>", "\t",
							 RegexOptions.IgnoreCase);

					// insert line breaks in places of <BR> and <LI> tags
					result = Regex.Replace(result,
							 @"<( )*br( )*>", "\r",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"<( )*li( )*>", "\r",
							 RegexOptions.IgnoreCase);

					// insert line paragraphs (double line breaks) in place
					// if <P>, <DIV> and <TR> tags
					result = Regex.Replace(result,
							 @"<( )*div([^>])*>", "\r\r",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"<( )*tr([^>])*>", "\r\r",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"<( )*p([^>])*>", "\r\r",
							 RegexOptions.IgnoreCase);

					// Remove remaining tags like <a>, links, images,
					// comments etc - anything that's enclosed inside < >
					result = Regex.Replace(result,
							 @"<[^>]*>", string.Empty,
							 RegexOptions.IgnoreCase);

					// replace special characters:
					result = Regex.Replace(result,
							 @" ", " ",
							 RegexOptions.IgnoreCase);

					result = Regex.Replace(result,
							 @"&bull;", " * ",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&lsaquo;", "<",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&rsaquo;", ">",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&trade;", "(tm)",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&frasl;", "/",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&lt;", "<",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&gt;", ">",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&copy;", "(c)",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 @"&reg;", "(r)",
							 RegexOptions.IgnoreCase);
					// Remove all others. More can be added, see
					// http://hotwired.lycos.com/webmonkey/reference/special_characters/
					result = Regex.Replace(result,
							 @"&(.{2,6});", string.Empty,
							 RegexOptions.IgnoreCase);

					// for testing
					//System.Text.RegularExpressions.Regex.Replace(result,
					//       this.txtRegex.Text,string.Empty,
					//       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

					// make line breaking consistent
					result = result.Replace("\n", "\r");

					// Remove extra line breaks and tabs:
					// replace over 2 breaks with 2 and over 4 tabs with 4.
					// Prepare first to remove any whitespaces in between
					// the escaped characters and remove redundant tabs in between line breaks
					result = Regex.Replace(result,
							 "(\r)( )+(\r)", "\r\r",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 "(\t)( )+(\t)", "\t\t",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 "(\t)( )+(\r)", "\t\r",
							 RegexOptions.IgnoreCase);
					result = Regex.Replace(result,
							 "(\r)( )+(\t)", "\r\t",
							 RegexOptions.IgnoreCase);
					// Remove redundant tabs
					result = Regex.Replace(result,
							 "(\r)(\t)+(\r)", "\r\r",
							 RegexOptions.IgnoreCase);
					// Remove multiple tabs following a line break with just one tab
					result = Regex.Replace(result,
							 "(\r)(\t)+", "\r\t",
							 RegexOptions.IgnoreCase);
					// Initial replacement target string for line breaks
					string breaks = "\r\r\r";
					// Initial replacement target string for tabs
					string tabs = "\t\t\t\t\t";
					for (int index = 0; index < result.Length; index++)
					{
						result = result.Replace(breaks, "\r\r");
						result = result.Replace(tabs, "\t\t\t\t");
						breaks = breaks + "\r";
						tabs = tabs + "\t";
					}

					// That's it.
					return result;
				}
				catch
				{

					return source;
				}
			}
			public static String SanitizeUserInput(String text)
			{
				if (String.IsNullOrEmpty(text))
					return String.Empty;

				String rxPattern = "<(?>\"[^\"]*\"|'[^']*'|[^'\">])*>";
				Regex rx = new Regex(rxPattern);
				String output = rx.Replace(text, String.Empty);

				return output;
			}
			public static string CleanDataNR(string str)
			{
				return str.ToLower().Replace("\n", " # ").Replace("\r", " @ ");


			}


			public static string CheckHtml(string source, string titleClass, string bulletClass)
			{
				string result = source;
				if (result.Contains('#') || result.Contains('{') || result.Contains('}') || result.Contains('*') || result.Contains('[') || result.Contains(']'))
				{
					result = DisplayAsHtml(source, titleClass, bulletClass);
				}

				return result;
			}

			public static string DisplayAsHtml(string source, string titleClass, string bulletClass)
			{
				var result = source;
				result = result.Replace("{", "<span class='" + titleClass + "'>").Replace("}", "</span>");
				result = result.Replace("\r\n", "<br/>");

				result = result.Replace("*", "<span class='" + bulletClass + "'></span>&nbsp;");

				try
				{
					while (result.Contains('['))
					{

						int index = result.IndexOf('[');
						string s = result.Substring(index + 1, (result.IndexOf(']') - index - 1));
						string replace;
						if (s.Contains('@'))
						{
							replace = "<a href='mailto:" + s + "'>" + s + "</a>";
						}
						else if (s.Contains('$'))
						{
							string[] linkData = s.Split('$');
							string linkUrl = linkData[1].ToLower().Replace("http://", "");

							replace = "<a target='_blank' href='http://" + linkUrl + "'>" + linkData[0] + "</a>";

						}
						else
						{
							string linkText = s.Replace("http://", "");
							replace = "<a href='http://" + linkText + "'>" + linkText + "</a>";
						}
						result = result.Replace("[" + s + "]", replace);



					}
				}
				catch
				{
					result = result.Replace("[", "").Replace("]", "").Replace("$", "");

				}



				return result;
			}


			public static string DisplayAsHtml(string source, string titleClass, string bulletClass, string LinkCalss)
			{
				var result = source;
				result = result.Replace("{", "<span class='" + titleClass + "'>").Replace("}", "</span>");
				result = result.Replace("\r\n", "<br/>");

				result = result.Replace("*", "<span class='" + bulletClass + "'></span>&nbsp;");

				try
				{
					while (result.Contains('['))
					{

						int index = result.IndexOf('[');
						string s = result.Substring(index + 1, (result.IndexOf(']') - index - 1));
						string replace;
						if (s.Contains('@'))
						{
							replace = "<a  class='" + LinkCalss + "' href='mailto:" + s + "'>" + s + "</a>";
						}
						else if (s.Contains('$'))
						{
							string[] linkData = s.Split('$');
							string linkUrl = linkData[1].ToLower().Replace("http://", "");

							replace = "<a  class='" + LinkCalss + "' href='http://" + linkUrl.ToLower().Replace("local", HttpContext.Current.Request.Url.Authority) + "'>" + linkData[0] + "</a>";

						}
						else
						{
							string linkText = s.Replace("http://", "");
							replace = "<a class='" + LinkCalss + "' href='http://" + linkText + "'>" + linkText + "</a>";
						}
						result = result.Replace("[" + s + "]", replace);



					}
				}
				catch
				{
					result = result.Replace("[", "").Replace("]", "").Replace("$", "");

				}



				return result;
			}

		}


		# endregion

		# region Date
		public class Date
		{
			private static HttpContext cur = HttpContext.Current;

			private static int startGreg = 1900;
			private static int endGreg = 2100;
			private static string[] allFormats = { "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy", "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy" };
			private static CultureInfo arCul = new CultureInfo("ar-SA");
			private static CultureInfo enCul = new CultureInfo("en-US");
			private static UmAlQuraCalendar umalqura = new UmAlQuraCalendar();
			private static GregorianCalendar g = new GregorianCalendar();



			/// <summary>
			/// Check if string is hijri date and then return true 
			/// </summary>
			/// <param name="hijri"></param>
			/// <returns></returns>
			public static bool IsHijri(string hijri)
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				if (hijri.Length <= 0)
				{

					//cur.Trace.Warn("IsHijri Error: Date String is Empty");
					return false;
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(hijri, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					if (tempDate.Year >= startGreg && tempDate.Year <= endGreg)
						return true;
					else
						return false;
				}
				catch (Exception ex)
				{
					//cur.Trace.Warn("IsHijri Error :" + hijri.ToString() + "\n" + ex.Message);
					return false;
				}

			}
			/// <summary>
			/// Check if string is Gregorian date and then return true 
			/// </summary>
			/// <param name="greg"></param>
			/// <returns></returns>
			public static bool IsGreg(string greg)
			{
				if (greg.Length <= 0)
				{

					cur.Trace.Warn("IsGreg :Date String is Empty");
					return false;
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(greg, allFormats, enCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					if (tempDate.Year >= startGreg && tempDate.Year <= endGreg)
						return true;
					else
						return false;
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("IsGreg Error :" + greg.ToString() + "\n" + ex.Message);
					return false;
				}

			}

			/// <summary>
			/// Return Formatted Hijri date string 
			/// </summary>
			/// <param name="date"></param>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string FormatHijri(string date, string format)
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				if (date.Length <= 0)
				{

					cur.Trace.Warn("Format :Date String is Empty");
					return "";
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(date, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return tempDate.ToString(format, arCul.DateTimeFormat);

				}
				catch (Exception ex)
				{
					cur.Trace.Warn("Date :\n" + ex.Message);
					return "";
				}

			}
			/// <summary>
			/// Returned Formatted Gregorian date string
			/// </summary>
			/// <param name="date"></param>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string FormatGreg(string date, string format)
			{
				if (date.Length <= 0)
				{

					cur.Trace.Warn("Format :Date String is Empty");
					return "";
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(date, allFormats, enCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return tempDate.ToString(format, enCul.DateTimeFormat);

				}
				catch (Exception ex)
				{
					cur.Trace.Warn("Date :\n" + ex.Message);
					return "";
				}

			}
			/// <summary>
			/// Return Today Gregorian date and return it in yyyy/MM/dd format
			/// </summary>
			/// <returns></returns>
			public static string GDateNow()
			{
				try
				{
					return DateTime.Now.ToString("yyyy/MM/dd", enCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("GDateNow :\n" + ex.Message);
					return "";
				}
			}
			/// <summary>
			/// Return formatted today Gregorian date based on your format
			/// </summary>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string GDateNow(string format)
			{
				try
				{
					return DateTime.Now.ToString(format, enCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("GDateNow :\n" + ex.Message);
					return "";
				}
			}

			/// <summary>
			/// Return Today Hijri date and return it in yyyy/MM/dd format
			/// </summary>
			/// <returns></returns>
			public static string HDateNow()
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				try
				{
					return DateTime.Now.ToString("yyyy/MM/dd", arCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("HDateNow :\n" + ex.Message);
					return "";
				}
			}
			/// <summary>
			/// Return formatted today hijri date based on your format
			/// </summary>
			/// <param name="format"></param>
			/// <returns></returns>

			public static string HDateNow(string format)
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				try
				{
					return DateTime.Now.ToString(format, arCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("HDateNow :\n" + ex.Message);
					return "";
				}

			}

          
			/// <summary>
			/// Convert Hijri Date to it's equivalent Gregorian Date
			/// </summary>
			/// <param name="hijri"></param>
			/// <returns></returns>
			public static string HijriToGreg(string hijri)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				if (hijri.Length <= 0)
				{

					cur.Trace.Warn("HijriToGreg :Date String is Empty");
					return "";
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(hijri, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return tempDate.ToString("yyyy/MM/dd", enCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("HijriToGreg :" + hijri.ToString() + "\n" + ex.Message);
					return "";
				}
			}

			public static DateTime? HijriToGregDate(string hijri)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				if (hijri.Length <= 0)
				{

					cur.Trace.Warn("HijriToGreg :Date String is Empty");
					return null;
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(hijri, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return tempDate;
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("HijriToGreg :" + hijri.ToString() + "\n" + ex.Message);
					return null;
				}
			}
			/// <summary>
			/// Convert Hijri Date to it's equivalent Gregorian Date and return it in specified format
			/// </summary>
			/// <param name="hijri"></param>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string HijriToGreg(string hijri, string format)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				if (hijri.Length <= 0)
				{

					cur.Trace.Warn("HijriToGreg :Date String is Empty");
					return "";
				}
				try
				{
					DateTime tempDate = DateTime.ParseExact(hijri, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return tempDate.ToString(format, enCul.DateTimeFormat);

				}
				catch (Exception ex)
				{
					cur.Trace.Warn("HijriToGreg :" + hijri.ToString() + "\n" + ex.Message);
					return "";
				}
			}

			/// <summary>
			/// Convert Gregoian Date to it's equivalent Hijir Date
			/// </summary>
			/// <param name="greg"></param>
			/// <returns></returns>
			public static string GregToHijri(DateTime greg)
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				return greg.ToString("yyyy/MM/dd", arCul.DateTimeFormat);
			}
			/// <summary>
			/// Convert Gregoian Date to it's equivalent Hijir Date
			/// </summary>
			/// <param name="greg"></param>
			/// <returns></returns>
			public static string GregToHijri(string greg)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				if (greg.Length <= 0)
				{
					return "";
				}

				DateTime tempDate = DateTime.ParseExact(greg, allFormats, enCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
				return tempDate.ToString("yyyy/MM/dd", arCul.DateTimeFormat);


			}
			/// <summary>
			/// Convert Hijri Date to it's equivalent Gregorian Date and return it in specified format
			/// </summary>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string GregToHijri(DateTime greg, string format)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				try
				{
					return greg.ToString(format, arCul.DateTimeFormat);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("GregToHijri :" + greg + "\n" + ex.Message);
					return "";
				}
			}
			/// <summary>
			/// Convert Hijri Date to it's equivalent Gregorian Date and return it in specified format
			/// </summary>
			/// <param name="greg"></param>
			/// <param name="format"></param>
			/// <returns></returns>
			public static string GregToHijri(string greg, string format)
			{
				arCul.DateTimeFormat.Calendar = umalqura;

				if (greg.Length <= 0)
				{

					cur.Trace.Warn("GregToHijri :Date String is Empty");
					return ""; 
				}
				try
				{

					DateTime tempDate = DateTime.ParseExact(greg, allFormats, enCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					string str= tempDate.ToString(format, arCul.DateTimeFormat);
					return tempDate.ToString(format, arCul.DateTimeFormat);

				}
				catch (Exception ex)
				{
					cur.Trace.Warn("GregToHijri :" + greg.ToString() + "\n" + ex.Message);
					return "";
				}
			}

			public static string GregToHijri2(DateTime gregdt, string format)
			{
				string greg = gregdt.ToShortDateString();
				arCul.DateTimeFormat.Calendar = umalqura;

				if (greg.Length <= 0)
				{

					cur.Trace.Warn("GregToHijri :Date String is Empty");
					return "";
				}
				try
				{

					DateTime tempDate = DateTime.ParseExact(greg, allFormats, enCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					string str = tempDate.ToString(format, arCul.DateTimeFormat);
					return tempDate.ToString(format, arCul.DateTimeFormat);

				}
				catch (Exception ex)
				{
					cur.Trace.Warn("GregToHijri :" + greg.ToString() + "\n" + ex.Message);
					return "";
				}
			}
			/// <summary>
			/// Return Gregrian Date Time as digit stamp
			/// </summary>
			/// <returns></returns>
			public static string GTimeStamp()
			{
				return GDateNow("yyyyMMddHHmmss");
			}
			/// <summary>
			/// Return Hijri Date Time as digit stamp
			/// </summary>
			/// <returns></returns>
			public static string HTimeStamp()
			{
				return HDateNow("yyyyMMddHHmmss");
			}

			/// <summary>
			/// Compare if the Hijri date between  other financial year and return True if within , False in not within
			/// </summary>
			/// <param name="dt">Data Table contain start date and end date ,,start date is the first column in table , end date is the second column in table</param>
			/// <param name="fromDate"></param>
			/// <param name="endDate"></param>
			/// <returns></returns>


			/// <summary>
			/// Compare two instaces of string date and return indication of thier values 
			/// </summary>
			/// <param name="d1"></param>
			/// <param name="d2"></param>
			/// <returns>positive d1 is greater than d2,negative d1 is smaller than d2, 0 both are equal</returns>
			public static int Compare(string d1, string d2)
			{
				arCul.DateTimeFormat.Calendar = umalqura;
				try
				{
					DateTime date1 = DateTime.ParseExact(d1, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					DateTime date2 = DateTime.ParseExact(d2, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
					return DateTime.Compare(date1, date2);
				}
				catch (Exception ex)
				{
					cur.Trace.Warn("Compare :" + "\n" + ex.Message);
					return -1;
				}

			}

			public static DateTime hijritodate(string hijri)
			{
				return DateTime.ParseExact(hijri, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);

			}

			public static DateTime? ConvertDate(string Date)
			{
				var dateValues = Date.Split('/');

				DateTime returnDate = Convert.ToDateTime(dateValues[1] + "/" + dateValues[2] + "/" + dateValues[0]);
				return returnDate;
			}

			public static DateTime? ConvertStringToDate(string Date)
			{
				var dateValues = Date.Split('-');

				DateTime returnDate = Convert.ToDateTime(dateValues[1] + "/" + dateValues[2] + "/" + dateValues[0]);
				return returnDate;
			}


			public static string ColorData(string data, string keyWord, bool isColored, string className)
			{

				if (isColored)
				{
					return Regex.Replace(data, keyWord, "<span class='" + className + "'>" + keyWord + "</span>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				}
				else
				{
					return data;
				}
			}

			public static DateTime? ParseDateTime(string dateTimeValue)
			{
				if (string.IsNullOrEmpty(dateTimeValue))
					return null;

				DateTime output = new DateTime();
				if (DateTime.TryParse(dateTimeValue, out output))
					return output;
				else
					return null;
			}
			public static string GetHijriDate()
			{
				CultureInfo higri_format = new CultureInfo("ar-SA");
				higri_format.DateTimeFormat.Calendar = new HijriCalendar();
				//  return DateTime.Now.ToString("dd/MM/yyyy", higri_format);
				return DateTime.Now.ToString("ddd d MMM   yyyy", higri_format);
			}
			public static string GetHijriDate(DateTime dt)
			{
				CultureInfo higri_format = new CultureInfo("ar-SA");
				higri_format.DateTimeFormat.Calendar = new HijriCalendar();
				//  return DateTime.Now.ToString("dd/MM/yyyy", higri_format);
				return dt.ToString("ddd d MMM   yyyy", higri_format);
			}

			/// <summary>
			/// Creats from to date logic.
			/// </summary>
			/// <param name="start">Start Date</param>
			/// <param name="end">End Date</param>
			/// <param name="isHijri">Flage indicats if you want Hijri date or not</param>
			/// <returns></returns>
			public static string HijriDateFromToLogic(DateTime start, DateTime end)
			{
				System.Text.StringBuilder sFormat = new StringBuilder();
				System.Text.StringBuilder eFormat = new StringBuilder();

				string sYear = string.Empty;
				string eYear = string.Empty;
				string sMonth = string.Empty;
				string eMonth = string.Empty;

				string fromExpression = "من";
				string toExpression = "إلى";
				string space = " ";

				sYear = GregToHijri(start, "yyyy");
				eYear = GregToHijri(end, "yyyy");
				sMonth = GregToHijri(start, "MM");
				eMonth = GregToHijri(end, "MM");

				sFormat.Append("d" + space);
				eFormat.Append("d" + space);

				if (sMonth != eMonth)
				{
					sFormat.Append("MMMM" + space);
					eFormat.Append("MMMM" + space);
				}
				else
				{
					eFormat.Append("MMMM" + space);
				}

				if (sYear != eYear)
				{
					if (sMonth == eMonth) { sFormat.Append("MMMM" + space); }
					if (GregToHijri(start, "yyyy") == GregToHijri(end, "yyyy"))
					{
						eFormat.Append("yyyy");
					}
					else
					{
						sFormat.Append("yyyy");
						eFormat.Append("yyyy");
					}
				}
				else
				{
					eFormat.Append("yyyy");
				}

				return fromExpression + space + GregToHijri(start, sFormat.ToString()) + space + toExpression + space + GregToHijri(end, eFormat.ToString());

			}
		}

		# endregion

		# region Image
		public class QVImage
		{
			public static void ResizeImage(Stream SourceImage, string DestImage, int OreginalDestWidth, int OreginalDestHeight, System.Drawing.Color FillColor, bool Transparent)
			{


				int DestWidth = 0;
				int DestHeight = 0;

				System.Drawing.Bitmap mImage = new System.Drawing.Bitmap(SourceImage);
				int Width = mImage.Width;


				if (((float)mImage.Width / (float)mImage.Height) > ((float)OreginalDestWidth / (float)OreginalDestHeight))
				{
					DestWidth = OreginalDestWidth;
					// Width is constant 
					DestHeight = OreginalDestWidth * mImage.Height / mImage.Width;

				}
				else
				{
					DestHeight = OreginalDestHeight;
					//Height is constant
					DestWidth = OreginalDestHeight * mImage.Width / mImage.Height;
				}





				int Height = mImage.Height;
				//int DestHeight2  =DestWidth  * Height / Width  ; 

				System.Drawing.Bitmap newImage = new System.Drawing.Bitmap(DestWidth, DestHeight);

				System.Drawing.Graphics G = System.Drawing.Graphics.FromImage(newImage);
				if (!Transparent)
					G.FillRectangle(new System.Drawing.SolidBrush(FillColor), new System.Drawing.Rectangle(0, 0, DestWidth, DestWidth));
				G.DrawImage(mImage, new System.Drawing.Rectangle(0, 0, DestWidth, DestHeight), new System.Drawing.Rectangle(0, 0, Width, Height), System.Drawing.GraphicsUnit.Pixel);
				newImage.Save(DestImage);
				G.Dispose();
				G = null;
				newImage.Dispose();
				newImage = null;

			}
			public static void GetThumbnail(byte[] imageBytes, int ImageSize, string DestImage, int maxWidth, int maxHeight, bool fixedSizeBackground)
			{
				using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(imageBytes)))
				{

					float fltRes = image.HorizontalResolution / 3;
					if (fltRes > 50)
						fltRes = 50;
					if (maxWidth < 1)
						maxWidth = image.Width;
					if (maxHeight < 1)
						maxHeight = image.Height;
					if (ImageSize > 120000)
					{
						if (image.Width > maxWidth || image.Height > maxHeight)
						{
							double widthRatio = (double)image.Width / maxWidth;
							double heightRatio = (double)image.Height / maxHeight;
							double ratio = Math.Max(widthRatio, heightRatio);
							int thumbWidth = (int)(image.Width / ratio);
							int thumbHeight = (int)(image.Height / ratio);
							if (fixedSizeBackground)
							{
								using (Bitmap bitmap = new Bitmap(maxWidth, maxHeight, PixelFormat.Format24bppRgb))
								{
									bitmap.SetResolution(fltRes, fltRes);

									using (Graphics graphics = Graphics.FromImage(bitmap))
									{
										graphics.SmoothingMode = SmoothingMode.HighQuality;
										graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
										graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
										graphics.FillRectangle(new System.Drawing.SolidBrush(Color.Transparent), new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight));
										int x = (maxWidth - thumbWidth) / 2;
										int y = (maxHeight - thumbHeight) / 2;
										graphics.DrawImage(image, x, y, thumbWidth, thumbHeight);
										bitmap.Save(DestImage);
									}
								}
							}
							else
							{
								using (Bitmap bitmap = new Bitmap(thumbWidth, thumbHeight, PixelFormat.Format24bppRgb))
								{
									bitmap.SetResolution(fltRes, fltRes);

									using (Graphics graphics = Graphics.FromImage(bitmap))
									{
										graphics.SmoothingMode = SmoothingMode.HighQuality;
										graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
										graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
										graphics.FillRectangle(new System.Drawing.SolidBrush(Color.Transparent), new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight));
										graphics.DrawImage(image, 0, 0, thumbWidth, thumbHeight);
										bitmap.Save(DestImage);
									}
								}

							}
						}
						else
						{
							using (Bitmap bitmap = new Bitmap(maxWidth, maxHeight, PixelFormat.Format24bppRgb))
							{
								bitmap.SetResolution(fltRes, fltRes);

								using (Graphics graphics = Graphics.FromImage(bitmap))
								{
									graphics.SmoothingMode = SmoothingMode.HighQuality;
									graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
									graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
									graphics.FillRectangle(new System.Drawing.SolidBrush(Color.Transparent), new System.Drawing.Rectangle(0, 0, maxWidth, maxHeight));
									graphics.DrawImage(image, 0, 0, maxWidth, maxHeight);
									bitmap.Save(DestImage);
								}
							}
						}
					}
					else
					{
						//image.Save(DestImage);
						Bitmap bitmap = new Bitmap(image, maxWidth, maxHeight);
						bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
						bitmap.Save(DestImage);
					}
				}
			}

		}
		# endregion

		# region NullValues
		public class NullValues
		{

			public static string NullCheck(object sender)
			{
				if (sender == null || sender.ToString() == string.Empty || sender.ToString() == "")
					return "&nbsp; &nbsp;";
				else
					return sender.ToString();
			}

			public static string ValueOrNotAvailable(string text, bool isHtml)
			{
				if (string.IsNullOrEmpty(text))
				{
					if (isHtml)

						return "<center>غير متاح</center>";
					else
						return "غير متاح";
				}
				else
					return text;
			}
			public static string ValueOrNotAvailable(string text, string cssClass)
			{
				if (string.IsNullOrEmpty(text))
				{
					return "<center class='" + cssClass + "'>غير متاح</center>";
				}
				else
					return text;
			}

			public static string GetImg(string strImgPath, string strLang)
			{
				string strImg = strImgPath.Split('/').LastOrDefault();
				if (strLang == "Arabic")
					return string.IsNullOrEmpty(strImg) ? "Default_Ar.jpg" : IO.IsFileExist(strImgPath) ? strImg : "Default_Ar.jpg";
				else
					return string.IsNullOrEmpty(strImg) ? "Default.jpg" : IO.IsFileExist(strImgPath) ? strImg : "Default.jpg";
			}


		}


		# endregion

		# region Numeric
		public class Numeric
		{
			public static int? ParseInt(object integer)
			{
				if (integer == null)
					return null;
				if (string.IsNullOrEmpty(integer.ToString()))
					return null;

				int output = 0;
				if (int.TryParse(integer.ToString(), out output))
					return output;
				else
					return null;
			}

			public static decimal? ParseDecimal(object Decimal)
			{
				if (Decimal == null)
					return null;
				if (string.IsNullOrEmpty(Decimal.ToString()))
					return null;

				decimal output = 0;
				if (decimal.TryParse(Decimal.ToString(), out output))
					return output;
				else
					return null;
			}

			public static int? ParseInt(string integer)
			{
				if (string.IsNullOrEmpty(integer))
					return null;

				int output = 0;
				if (int.TryParse(integer, out output))
					return output;
				else
					return null;
			}


			public static double? ParseDouble(string doubleValue)
			{
				if (string.IsNullOrEmpty(doubleValue))
					return null;

				double output = 0;
				if (double.TryParse(doubleValue, out output))
					return output;
				else
					return null;
			}


			public static bool IsNumeric(string strToCheck)
			{
				return Regex.IsMatch(strToCheck, "^\\d+(\\.\\d+)?$");
			}
		}


		# endregion

		#region Sql
		public class Sql
		{
			/// <summary>
			/// Clean any harmful charecters from searchstring
			/// </summary>
			/// <param name="searchString">Text to clean.</param>
			/// <returns></returns>
			public static string CleanSearchString(string searchString)
			{
				if (string.IsNullOrEmpty(searchString))
					return null;

				// Correct wild card replacements
				searchString = searchString.Replace("*", "%");

				// Strip any html or xml characters
				searchString = Regex.Replace(searchString, "<[^>]+>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);

				// Remove known bad SQL characters
				searchString = Regex.Replace(searchString, "--|;|'|\"", " ", RegexOptions.Compiled | RegexOptions.Multiline);

				// Finally remove any extra spaces from the string
				searchString = Regex.Replace(searchString, " {1,}", " ", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

				return searchString;

			}

			/// <summary>
			/// Protect single quote character from stripping out strings.
			/// </summary>
			/// <param name="text">Text to encode.</param>
			/// <returns></returns>
			public static string SQLEncode(string text)
			{
				return text.Replace("'", "''");
			}

			/// <summary>
			/// protext text from sql injection
			/// </summary>
			/// <param name="text"></param>
			/// <returns></returns>
			public static string EscapeSql(string text)
			{
				string result = text;
				result = result.Replace("/*", "").Replace("*/", "").Replace("UNION", "").Replace(";", "\\;").Replace("'", "&amp;rsquo;").Replace("\"\"", "&amp;quot;").Replace("\\", "\\\\");
				return result;
			}


		}
		#endregion

		# region Mime
		public class Mime
		{
			[System.Runtime.InteropServices.DllImport("urlmon.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
			static extern int FindMimeFromData(IntPtr pBC, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzUrl, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer, int cbSize, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzMimeProposed, int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

			public static string getMimeFromFile(HttpPostedFile file)
			{
				IntPtr mimeout;

				int MaxContent = (int)file.ContentLength;
				if (MaxContent > 4096) MaxContent = 4096;

				byte[] buf = new byte[MaxContent];
				file.InputStream.Read(buf, 0, MaxContent);
				int result = FindMimeFromData(IntPtr.Zero, file.FileName, buf, MaxContent, null, 0, out mimeout, 0);

				if (result != 0)
				{
					System.Runtime.InteropServices.Marshal.FreeCoTaskMem(mimeout);
					return "";
				}

				string mime = System.Runtime.InteropServices.Marshal.PtrToStringUni(mimeout);
				System.Runtime.InteropServices.Marshal.FreeCoTaskMem(mimeout);

				return mime.ToLower();
			}
		}
		# endregion


		# region LogEvent
		public class LogEvent
		{

			public static void LogToEventLog(string msg)
			{

				StreamWriter SW;
				// HostingEnvironment.MapPath
				SW = File.AppendText(HostingEnvironment.MapPath("/Log.txt"));
				SW.WriteLine(DateTime.Now.ToString() + ":" + msg);
				SW.Close();

			}
		}

		# endregion


		# region ResourceFile
		public class ResourceFile
		{

			public static string GetGlobalResourceValue(string key)
			{
				return HttpContext.GetGlobalResourceObject("GeneralMessage", key).ToString();
			}
			public static string GetValue(string key, string resourceFile)
			{
				return "";
				//return HttpContext.GetGlobalResourceObject(resourceFile, key).ToString();
			}
		}
		# endregion

		#region Files
		public class IO
		{
			public static void Logme(string body, string strPath)
			{


				if (File.Exists(strPath))
				{
					StreamWriter strFile = new StreamWriter(strPath, true);
					strFile.WriteLine(DateTime.Now.ToString() + " : " + body);
					strFile.Close();
				}
				else
				{
					File.WriteAllText(strPath, body);
				}
			}

			public static void StartLog(string strPath)
			{
				File.WriteAllText(strPath, "------بسم الله الرحمن الرحيم");
			}
			public static MediaType GetType(string FileName)
			{
				string extension = FileName.Substring(FileName.LastIndexOf('.') + 1).ToLower();
				foreach (var item in Enum.GetNames(typeof(MediaType)))
				{
					if (ConfigurationManager.AppSettings[item + "Extensions"] != null && ConfigurationManager.AppSettings[item + "Extensions"].ToLower().Contains(extension))
					{
						return (MediaType)Enum.Parse(typeof(MediaType), item);
					}
				}
				return MediaType.Unknown;
			}

			public static string GetUniqueFileName(string fileName)
			{
				return System.Guid.NewGuid().ToString() + "_" + fileName.Replace(' ', '+');
			}

			public static void SaveFile(FileUpload fileUpload, string location, string fileName)
			{

				fileUpload.SaveAs(HttpContext.Current.Server.MapPath("~/") + location + fileName);
			}

			public static void DeleteFileWithPath(string location, string fileName, string path)
			{
				string filePath = MapPath(fileName, path + "/" + location);
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
			}

			public static void DeleteFile(string location, string fileName)
			{
				string filePath = HttpContext.Current.Server.MapPath("~/") + location + fileName;
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
			}

			public static void DeleteFile(string fileName)
			{
				string filePath = HttpContext.Current.Server.MapPath(fileName);
				try
				{
					File.Delete(filePath);
				}
				catch
				{
				}
			}


			public static bool IsFileExist(string fileName)
			{
				string filePath = HttpContext.Current.Server.MapPath(fileName);
				if (File.Exists(filePath))
				{
					return true;
				}
				return false;
			}

			public static bool IsFileExistWithPath(string fileName, string path)
			{
				string filePath = MapPath(fileName, path);
				if (File.Exists(filePath))
				{
					return true;
				}
				return false;
			}

			public static string MapPath(string fileName, string path)
			{
				fileName = fileName.Replace("/", "\\");
				if (fileName.Length > 0)
					if (fileName[0] == '~')
						fileName = fileName.Remove(0, 2);
				return path + "\\" + fileName;
			}

			public static List<string> GetFilesInFolder(string sourceDir, string extension)
			{
				// Process the list of files found in the directory. 
				string[] fileEntries = Directory.GetFiles(sourceDir);
				List<string> filebath = new List<string>();
				foreach (string fileName in fileEntries)
				{
					if (System.IO.Path.GetExtension(fileName).ToLower() == extension)
					{
						filebath.Add(fileName);
					}
				}
				return filebath;
			}
			public static List<string> GetFilesNameInFolder(string sourceDir, string extension)
			{
				// Process the list of files found in the directory. 
				List<string> filebath = new List<string>();
				if (Directory.Exists(sourceDir))
				{
					string[] fileEntries = Directory.GetFiles(sourceDir);
					foreach (string fileName in fileEntries)
					{
						if (extension.ToLower().Split(',').Contains(System.IO.Path.GetExtension(fileName).ToLower().Replace(".", "")))
						{
							filebath.Add(System.IO.Path.GetFileName(fileName));
						}
					}
				}
				return filebath;
			}
			public static List<string> GetFilesInFolder(string sourceDir)
			{
				// Process the list of files found in the directory. 
				List<string> filebath = new List<string>();
				if (Directory.Exists(sourceDir))
				{
					string[] fileEntries = Directory.GetFiles(sourceDir);
					foreach (string fileName in fileEntries)
					{
						filebath.Add(fileName);
					}
				}
				return filebath;
			}

			public static bool IsValidImageFile(string fileName)
			{
				bool isValid = false;
				string fileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1);
				string[] imageExtenstions = ConfigurationManager.AppSettings["ImageExtensions"].Split(',');
				foreach (string extension in imageExtenstions)
				{
					isValid |= (string.Compare(fileExtension.Trim(), extension.Trim(), true) == 0);
				}
				return isValid;
			}
			public static bool IsValidVedioAudioFile(string fileName)
			{
				bool isValid = false;
				string fileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1);
				string[] imageExtenstions = ConfigurationManager.AppSettings["MovieExtensions"].Split(',');
				foreach (string extension in imageExtenstions)
				{
					isValid |= (string.Compare(fileExtension.Trim(), extension.Trim(), true) == 0);
				}
				return isValid;
			}

			public static bool IsValidFile(string fileName, string Extension)
			{
				bool isValid = false;
				string fileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1);


				isValid = (string.Compare(fileExtension.Trim(), Extension, true) == 0);

				return isValid;
			}


			public static int? GetHttpFileSize(string ftp)
			{
				try
				{
					System.Net.WebRequest req = System.Net.HttpWebRequest.Create(ftp);
					req.Method = "HEAD";
					System.Net.WebResponse resp = req.GetResponse();
					return QvLib.QVUtil.Numeric.ParseInt(resp.Headers.Get("Content-Length"));
				}
				catch (Exception)
				{

					return null;
				}
			}

			public static string GetFileExtension(string fileName)
			{
				return Path.GetExtension(fileName);
			}
			public static long GetFileSize(string fileName)
			{
				FileInfo f = new FileInfo(HttpContext.Current.Server.MapPath(fileName));
				return f.Length;
			}


			public static string ImageExtensions
			{
				get { return ConfigurationManager.AppSettings["ImageExtensions"].ToLower(); }
			}
			public static string MovieExtensions
			{
				get { return ConfigurationManager.AppSettings["MovieExtensions"].ToLower(); }
			}
			public static string FlashExtensions
			{
				get { return ConfigurationManager.AppSettings["FlashExtensions"].ToLower(); }
			}
			public static string FlVExtensions
			{
				get { return ConfigurationManager.AppSettings["FlVExtensions"].ToLower(); }
			}

			/// <summary>
			/// The type of the media
			/// </summary>


		}
		#endregion

		#region AppSetting
		public class AppSetting
		{

			public static string GetAppSetting(string name)
			{
				if (System.Configuration.ConfigurationManager.AppSettings[name] != null)
					return System.Configuration.ConfigurationManager.AppSettings[name].ToString();
				else
					return "";
			}

		}
		#endregion



	}

	namespace MultiMedia
	{
		# region HtmlObjectTag
		public class HtmlObjectTag
		{

			public static string GetReal(string FileUrl, int Width, int Height)
			{
				//"<object id='rvocx' classid='clsid:CFCDAA03-8BE4-11cf-B84B-0020AFBBCCFA' width='" + Width + "' height='" + Height + "'><param name='src' value='" + FileUrl + "'><param name='autostart' value='true'><param name='controls' value='imagewindow'><param name='console' value='video'><param name='loop' value='true'><embed src='" + FileUrl + "' width='" + Width + "' height='" + Height + "' loop='true' type='audio/x-pn-realaudio-plugin' controls='imagewindow' console='video' autostart='true'></embed></object>";
				return "<OBJECT classid='clsid:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA' height=" + Height + " id=video1 style='LEFT: 0px; TOP: 0px' width=" + Width + " VIEWASTEXT><PARAM NAME='_ExtentX' VALUE='7276'><PARAM NAME='_ExtentY' VALUE='1588'><PARAM NAME='autostart' VALUE='1'><PARAM NAME='SHUFFLE' VALUE='0'><PARAM NAME='PREFETCH' VALUE='0'><PARAM NAME='NOLABELS' VALUE='0'><PARAM NAME='SRC' VALUE='" + FileUrl + "' ref><PARAM NAME='CONTROLS' VALUE='ControlPanel,StatusBar'><PARAM NAME='CONSOLE' VALUE='Clip1'><PARAM NAME='LOOP' VALUE='0'><PARAM NAME='NUMLOOP' VALUE='0'><PARAM NAME='CENTER' VALUE='0'><PARAM NAME='MAINTAINASPECT' VALUE='0'><PARAM NAME='BACKGROUNDCOLOR' VALUE='#000000'> <embed src='" + FileUrl + "' type='audio/x-pn-realaudio-plugin' CONSOLE='Clip1'CONTROLS='ControlPanel,StatusBar' HEIGHT='57' WIDTH='100%' AUTOSTART='true'></OBJECT> ";
			}

			public static string GetAudio(string FileUrl, int Width, int Height)
			{
				return "<object id='MediaPlayer' width='" + Width + "' height='" + Height + "' classid='CLSID:22D6F312-B0F6-11D0-94AB-0080C74C7E95' standby='Loading Windows Media Player components...' type='application/x-oleobject'><param name='FileName' value='" + FileUrl + "' /><param name='autostart' value='true' /><param name='ShowControls' value='true' /><param name='ShowStatusBar' value='false' /><param name='ShowDisplay' value='false' /><embed type='application/x-mplayer2' src='" + FileUrl + "' name='MediaPlayer' width='" + Width + "' height='" + Height + "' showcontrols='1' showstatusbar='0' showdisplay='0' autostart='1'></embed></object>";
			}

			public static string GetImage(string FileUrl, int Width, int Height)
			{
				return "<Image   width='" + Width + "' height='" + Height + "' src='" + FileUrl + "' />";
			}

			public static string GetFlash(string FileUrl, int Width, int Height)
			{
				return "<object width='" + Width + "' height='" + Height + "'> <param name='movie' value='" + FileUrl + "'><embed src='" + FileUrl + "' width='" + Width + "' height='" + Height + "' ></embed></object>";
			}

			public static string GetFLV(string FileUrl, int Width, int Height)
			{
				return "<object width='" + Width + "' height='" + Height + "'><param name='movie' value='/SWF/flvplayer.swf'></param><param name='allowFullScreen' value='true'></param><embed src='/SWF/flvplayer.swf' type='application/x-shockwave-flash' allowfullscreen='true' width='" + Width + "' height='" + Height + "' flashvars='vdo=" + FileUrl + "&autoplay=true&sound=80&mylogo=/App_Themes/Layout/Images/logo.png&buffer=3'></embed></object>";
			}

			public static string GetMovieClip(string FileUrl, int Width, int Height)
			{
				return "<object id='MediaPlayer' width='" + Width + "' height='" + Height + "' classid='CLSID:22D6F312-B0F6-11D0-94AB-0080C74C7E95' standby='Loading Windows Media Player components...' type='application/x-oleobject'><param name='FileName' value='" + FileUrl + "' /><param name='autostart' value='true' /><param name='ShowControls' value='true' /><param name='ShowStatusBar' value='false' /><param name='ShowDisplay' value='false' /><embed type='application/x-mplayer2' src='" + FileUrl + "' name='MediaPlayer' width='" + Width + "' height='" + Height + "' showcontrols='1' showstatusbar='0' showdisplay='0' autostart='1'></embed></object>";
			}

		}
		# endregion

		# region ConvertMedia
		public class Converter
		{
			public static bool ReturnVideo(string fileName, string covertionType, string inputFolder, string outputFolder, string thumbFolder)
			{
				try
				{
					string namewithoutext = System.IO.Path.GetFileNameWithoutExtension(fileName);
					string ext = System.IO.Path.GetExtension(fileName);

					if (ext.ToLower() != ".flv")
					{
						string cmd = " -i \"" + inputFolder + "\\" + fileName + "\" \"" + outputFolder + "\\" + namewithoutext + "." + covertionType + "\"";
						ConvertNow(cmd);
					}
					string imgargs = " -i \"" + outputFolder + "\\" + namewithoutext + "." + covertionType + "\" -f image2 -ss 1 -vframes 1 -s 200x200 -an \"" + thumbFolder + "\\" + namewithoutext + ".jpg" + "\"";
					ConvertNow(imgargs);
					return true;
				}
				catch
				{
					return false;
				}
			}

			public static bool ReturnVideo(string fileName, string covertionType, string inputFolder, string outputFolder)
			{
				try
				{
					string namewithoutext = System.IO.Path.GetFileNameWithoutExtension(fileName);
					string ext = System.IO.Path.GetExtension(fileName);

					if (ext.ToLower() != "." + covertionType.ToLower())
					{
						string cmd = " -i \"" + inputFolder + "\\" + fileName + "\" \"" + outputFolder + "\\" + namewithoutext + "." + covertionType + "\"";
						ConvertNow(cmd);
					}

					return true;
				}
				catch
				{
					return false;
				}
			}

			private static void ConvertNow(string cmd)
			{
				string exepath;
				string AppPath = HttpContext.Current.Request.PhysicalApplicationPath;
				//Get the application path
				exepath = AppPath + "Bin\\ffmpeg\\ffmpeg.exe";
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = exepath;
				//Path of exe that will be executed, only for "filebuffer" it will be "flvtool2.exe"
				proc.StartInfo.Arguments = cmd;
				//The command which will be executed
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;
				proc.StartInfo.RedirectStandardOutput = false;
				proc.Start();
				while (proc.HasExited == false)
				{

				}
			}
		}
		# endregion
	}

	namespace Google
	{
		# region GoogleMap

		public class GoogleMap
		{
			public static string GetGooglekey()
			{
				if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("qvisionweb.com"))
				{
					return "ABQIAAAA7t7ZQ5CgoEnVH0FThJpXvBQe4wV0Lr00Q_xBjvpQEBTocjoreBTJ1NrctsbaCa7IogBucAjR5bZllw";
				}

				else if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
				{

					return "ABQIAAAA7t7ZQ5CgoEnVH0FThJpXvBQe4wV0Lr00Q_xBjvpQEBTocjoreBTJ1NrctsbaCa7IogBucAjR5bZllw";
				}
				else if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("qvision-web.com"))
				{

					return "ABQIAAAA7t7ZQ5CgoEnVH0FThJpXvBQkjxjZoX7YdG-5OqqGeudVmidU5xSNg_qiRoZGvFywH4tyPd0JzH4J8w";
				}
				else
				{
					return "ABQIAAAA7t7ZQ5CgoEnVH0FThJpXvBQe4wV0Lr00Q_xBjvpQEBTocjoreBTJ1NrctsbaCa7IogBucAjR5bZllw";
				}

			}
		}
		# endregion
	}

	namespace Security
	{
		# region DataProtection
		public class DataProtection
		{
			private static byte[] key = { };
			private static byte[] IV = { 0X12, 0X34, 0X56, 0X78, 0X90, 0XAB, 0XCD, 0XEF };
			private DataProtection()
			{
				//
				// TODO: Add constructor logic here
				//
			}
			public static string Encrypt(string strencrypt)
			{
				string skey = "33439650";
				try
				{
					key = System.Text.Encoding.UTF8.GetBytes(skey.Substring(0, 8));
					DESCryptoServiceProvider des = new DESCryptoServiceProvider();
					byte[] inputByteArray = Encoding.UTF8.GetBytes(strencrypt);
					MemoryStream ms = new MemoryStream();
					CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
					cs.Write(inputByteArray, 0, inputByteArray.Length);
					cs.FlushFinalBlock();
					return Convert.ToBase64String(ms.ToArray());
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			public static string Encrypt(string stringToEncrypt, string SEncryptionKey)
			{
				try
				{
					key = System.Text.Encoding.UTF8.GetBytes(SEncryptionKey);
					DESCryptoServiceProvider des = new DESCryptoServiceProvider();
					byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
					MemoryStream ms = new MemoryStream();
					CryptoStream cs = new CryptoStream(ms,
					  des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
					cs.Write(inputByteArray, 0, inputByteArray.Length);
					cs.FlushFinalBlock();
					return Convert.ToBase64String(ms.ToArray());
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}

			public static string Decrypt(string stringToDecrypt, string sEncryptionKey)
			{
				byte[] inputByteArray = new byte[stringToDecrypt.Length + 1];
				try
				{
					key = System.Text.Encoding.UTF8.GetBytes(sEncryptionKey);
					DESCryptoServiceProvider des = new DESCryptoServiceProvider();
					inputByteArray = Convert.FromBase64String(stringToDecrypt);
					MemoryStream ms = new MemoryStream();
					CryptoStream cs = new CryptoStream(ms,
					  des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
					cs.Write(inputByteArray, 0, inputByteArray.Length);
					cs.FlushFinalBlock();
					System.Text.Encoding encoding = System.Text.Encoding.UTF8;
					return encoding.GetString(ms.ToArray());
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}
			public static string Decrypt(string strdecrypt)
			{
				string skey = "33439650";
				byte[] inputByteArray = new byte[strdecrypt.Length + 1];
				try
				{
					key = System.Text.Encoding.UTF8.GetBytes(skey.Substring(0, 8));
					DESCryptoServiceProvider des = new DESCryptoServiceProvider();
					inputByteArray = Convert.FromBase64String(strdecrypt);
					MemoryStream ms = new MemoryStream();
					CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
					cs.Write(inputByteArray, 0, inputByteArray.Length);
					cs.FlushFinalBlock();
					System.Text.Encoding encoding = System.Text.Encoding.UTF8;
					return encoding.GetString(ms.ToArray());
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
		}
		# endregion
	}

	# region Mail
    public partial class Mail
    {
        private static UnitOfWork _unitOfWork;

        public Mail()
        {
          _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }
        
        /// <summary>
        /// Hold Setting Object
        /// </summary>
        public static Setting Settings = _unitOfWork.Settings.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
        
        /// <summary>
        /// Load Mail Template 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string LoadMailTemplate(string path)
        {
            string strPath = HttpContext.Current.Server.MapPath(path);
            return File.ReadAllText(strPath);


        }




        /// <summary>
        /// SendMail
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool SendMail(string subject, string to, string body)
        {
            return SendMail(subject, to, body, Settings.FromEmail);
        }



        public static bool SendMail(string subject, string to, string body, string from)
        {
            bool success = true;
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                MailAddress fromAddress = new MailAddress(from.Trim().ToString());
                message.From = fromAddress;
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                smtpClient.Host = Settings.ServerName.ToString();
                smtpClient.Port = Settings.PortNo;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(Settings.UserName.Trim().ToString(), Settings.Password.Trim().ToString());
                smtpClient.EnableSsl = Settings.SSL;
                smtpClient.Send(message);

            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }



        /// <summary>
        /// SendMailWithCC
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="body"></param>
        /// <param name="from"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static bool SendMailWithCC(string subject, string to, string body, string from, string cc)
        {
            return QVMail.SendMailWithCC(Settings.ServerName, Settings.UserName, Settings.Password, Settings.PortNo, Settings.SSL, subject, to, body, from, "/MailTemplate/ReportTemplate.htm", cc);

        }

        /// <summary>
        /// SendMailWithoutTemplate
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="body"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool SendMailWithoutTemplate(string subject, string to, string body, string from)
        {
            return QVMail.SendMail(Settings.ServerName, Settings.UserName, Settings.Password, Settings.PortNo, Settings.SSL, subject, to, body, from, "/MailTemplate/ReportTemplate.htm");

        }

        /// <summary>
        /// SendMailWithAttcement
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="body"></param>
        /// <param name="from"></param>
        /// <param name="cc"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static bool SendMailWithAttcement(string subject, string to, string body, string from, string cc, string FilePath)
        {
            return QVMail.SendMailWithAttcement(Settings.ServerName, Settings.UserName, Settings.Password, Settings.PortNo, Settings.SSL, subject, to, body, Settings.FromEmail, "/MailTemplate/ReportTemplate.htm", cc, FilePath);

        }

        /// <summary>
        /// SendMailWithAttcement
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <param name="body"></param>
        /// <param name="from"></param>
        /// <param name="stream"></param>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static bool SendMailWithAttcement(string subject, string to, string body, string from, Stream stream, string Filename)
        {
            return QVMail.SendMailWithAttcement(Settings.ServerName, Settings.UserName, Settings.Password, Settings.PortNo, Settings.SSL, subject, to, body, Settings.FromEmail, "/MailTemplate/ReportTemplate.htm", stream, Filename);
        }
    }

	public class QVMail
	{

		public static string LoadMailTemplate(string path)
		{
			string strPath = HttpContext.Current.Server.MapPath(path);
			StreamReader strLetter = new StreamReader(strPath);
			string strFinalLetter = string.Empty;

			while (strLetter.Peek() != -1)
			{
				strFinalLetter = strLetter.ReadToEnd();
			}
			return strFinalLetter;
		}

		public static string LoadMailTemplateInGlobal(string path)
		{
			StreamReader strLetter = new StreamReader(path);
			string strFinalLetter = string.Empty;

			while (strLetter.Peek() != -1)
			{
				strFinalLetter = strLetter.ReadToEnd();
			}
			return strFinalLetter;
		}

		public static string LoadMailTemplate(string path, HttpContext ServerContext)
		{
			string strPath = ServerContext.Server.MapPath(path);
			StreamReader strLetter = new StreamReader(strPath);
			string strFinalLetter = string.Empty;

			while (strLetter.Peek() != -1)
			{
				strFinalLetter = strLetter.ReadToEnd();
			}
			return strFinalLetter;
		}

		public static bool TestMailSettings(string ServerName, string UserName, string Password, string fromEmail, string toMail, int PortNo, out string errorMessage)
		{
			try
			{
				SmtpClient objSmtpClient = new SmtpClient();
				objSmtpClient.Host = ServerName;
				objSmtpClient.Port = PortNo;
				objSmtpClient.Credentials = new System.Net.NetworkCredential(UserName, Password);
				objSmtpClient.Send(fromEmail, toMail, "Testing Mail Service......", "Administrator test......");

				errorMessage = "";
				return true;
			}
			catch (Exception ex)
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				errorMessage = ex.Message;
				return false;
			}
		}

		public static bool SendMailWithoutTemplate(string ServerName, string UserName, string Password, int PortNo,
		 bool SSL, string subject, string to, string body, string from)
		{

			bool success = true;
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			SmtpClient smtpClient = new SmtpClient();
			try
			{

				MailAddress fromAddress = new MailAddress(from.Trim().ToString());
				message.From = fromAddress;
				message.To.Add(to);

				message.Subject = subject;
				message.IsBodyHtml = true;
				//message.CC.Add(cc);
				message.Body = body;
				smtpClient.Host = ServerName.ToString();
				smtpClient.Port = PortNo;
				//smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Trim().ToString(), Password.Trim().ToString());
				smtpClient.EnableSsl = SSL;
				smtpClient.Send(message);

			}
			catch (Exception ex)
			{
				success = false;
			}

			return success;
		}


		public static bool SendMail(string ServerName, string UserName, string Password, int PortNo,
		   bool SSL, string subject, string to, string body, string from, string TemplatePath)
		{

			bool success = true;
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			SmtpClient smtpClient = new SmtpClient();
			try
			{

				MailAddress fromAddress = new MailAddress(from.Trim().ToString());
				message.From = fromAddress;
				message.To.Add(to);

				message.Subject = subject;
				message.IsBodyHtml = true;
				//message.CC.Add(cc);
				if (TemplatePath.Trim() != "")
				{
					message.Body = QVMail.LoadMailTemplate(TemplatePath).Replace("{body}", body);
				}
				else
				{
					message.Body = body;
				}
				smtpClient.Host = ServerName.ToString();
				smtpClient.Port = PortNo;
				//smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Trim().ToString(), Password.Trim().ToString());
				smtpClient.EnableSsl = SSL;
				smtpClient.Send(message);

			}
			catch (Exception ex)
			{
				success = false;
			}

			return success;
		}

		public static bool SendMail(string serverName, string userName, string password, int portNo, bool ssl, string subject, string to, string body, string from)
		{

			bool success = true;
			var message = new System.Net.Mail.MailMessage();
			var smtpClient = new SmtpClient();

			try
			{
				var fromAddress = new MailAddress(from.Trim());
				message.From = fromAddress;
				message.To.Add(to);
				message.Subject = subject;
				message.IsBodyHtml = true;
				message.Body = body+" <br/> <a href='%%UNSUBSCRIBE%%'>لمنع وصول أى بريد منا إلىكم برجاء الضغط هنا</a>";
				smtpClient.Host = serverName;
				smtpClient.Port = portNo;
				smtpClient.Credentials = new System.Net.NetworkCredential(userName.Trim(), password.Trim());
				smtpClient.EnableSsl = ssl;
				smtpClient.Send(message);
			}
			catch (Exception e)
			{
				success = false;
			}
			return success;
		}

		public static bool SendMailInlineAttachment(string ServerName, string UserName, string Password, int PortNo,
	  bool SSL, string subject, string to, string body, string from, string strLogPath, HttpServerUtility serv, int iMId)
		{

			bool success = true;
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			SmtpClient smtpClient = new SmtpClient();
			try
			{

				MailAddress fromAddress = new MailAddress(from.Trim().ToString());
				message.From = fromAddress;
				message.To.Add(to);

				message.Subject = subject;
				message.IsBodyHtml = true;
				//message.CC.Add(cc);

				message.Body = AddInlineAttach(message, body, serv);
				List<string> filebath = QvLib.QVUtil.IO.GetFilesInFolder(serv.MapPath("~/DataFiles/" + iMId + "/"));
				if (filebath.Count() > 0)
				{
					System.Net.Mail.Attachment attachFile = new System.Net.Mail.Attachment(filebath[0]);
					message.Attachments.Add(attachFile);
				}

				smtpClient.Host = ServerName.ToString();
				smtpClient.Port = PortNo;
				//smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Trim().ToString(), Password.Trim().ToString());
				smtpClient.EnableSsl = SSL;
				//if (CheckSMSSettingCounter())
				smtpClient.Send(message);
				//else success = false;
				IO.Logme(to + " | " + "Succes", strLogPath);
			}
			catch (Exception ex)
			{
				IO.Logme(to + " | " + "Fail  " + ex.Message.ToString(), strLogPath);
				success = false;
			}
			//if (success)
			//{
			//    IncrementSMSSettingCounter();
			//}
			return success;
		}
		private static string AddInlineAttach(MailMessage mail, string strbody, HttpServerUtility serv)
		{

			while (true)
			{
				if (strbody.IndexOf("/userfiles") > 0)
				{
					int iIndex = strbody.IndexOf("/userfiles");
					string strPath = "";
					for (int i = iIndex; i < strbody.Length; i++)
					{
						if (strbody[i] == '"')
						{
							strPath = strbody.Substring(iIndex, i - iIndex);
							break;
						}
					}
					string attachmentPath = serv.MapPath(strPath);
					string strExtension = QVUtil.IO.GetFileExtension(strPath);
					string contentID = Path.GetFileName(attachmentPath).Replace(".", "") + "@zofm";
					strbody = strbody.Replace(strPath, "cid:" + contentID);
					// create the INLINE attachment
					System.Net.Mail.Attachment inline = new System.Net.Mail.Attachment(attachmentPath);
					inline.ContentDisposition.Inline = true;
					inline.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
					inline.ContentId = contentID;
					inline.ContentType.MediaType = "image/" + strExtension.Replace(".", "").Trim();
					inline.ContentType.Name = Path.GetFileName(attachmentPath);
					mail.Attachments.Add(inline);
				}
				else
				{
					break;
				}
			}

			return strbody;
		}

		public static bool SendMailWithCC(string ServerName, string UserName, string Password, int PortNo,
		   bool SSL, string subject, string to, string body, string from, string TemplatePath, string cc)
		{
			bool success = true;
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			SmtpClient smtpClient = new SmtpClient();
			try
			{

				MailAddress fromAddress = new MailAddress(from.Trim().ToString());
				message.From = fromAddress;
				message.To.Add(to);


				message.Subject = subject;
				message.IsBodyHtml = true;
				message.CC.Add(cc);
				message.Body = QVMail.LoadMailTemplate(TemplatePath).Replace("{body}", body);
				smtpClient.Host = ServerName.ToString();
				smtpClient.Port = PortNo;
				//smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Trim().ToString(), Password.Trim().ToString());
				smtpClient.EnableSsl = SSL;
				smtpClient.Send(message);

			}
			catch (Exception ex)
			{
				success = false;
			}

			return success;
		}



		public static bool SendMailWithAttcement(string ServerName, string UserName, string Password, int PortNo,
		   bool SSL, string subject, string to, string body, string from, string TemplatePath, string cc, string FilePath)
		{
			bool success = true;
			System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
			SmtpClient smtpClient = new SmtpClient();
			try
			{

				MailAddress fromAddress = new MailAddress(from.Trim().ToString());
				message.From = fromAddress;
				message.To.Add(to);
				System.Net.Mail.Attachment attachFile = new System.Net.Mail.Attachment(FilePath);
				message.Attachments.Add(attachFile);
				message.Subject = subject;
				message.IsBodyHtml = true;
				message.CC.Add(cc);
				message.Body = QVMail.LoadMailTemplate(TemplatePath).Replace("{body}", body);
				smtpClient.Host = ServerName.ToString();
				smtpClient.Port = PortNo;
				//smtpClient.UseDefaultCredentials = true;
				smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Trim().ToString(), Password.Trim().ToString());
				smtpClient.EnableSsl = SSL;
				smtpClient.Send(message);

			}
			catch (Exception ex)
			{
				success = false;
			}

			return success;
		}

		public static bool SendMailWithAttcement(string ServerName, string UserName, string Password, int PortNo,
		   bool SSL, string subject, string to, string body, string from, string TemplatePath, Stream stream, string Filename)
		{
			bool success = true;

			SmtpClient smtpClient = new SmtpClient();
			smtpClient.Host = ServerName;
			smtpClient.Port = PortNo;
			smtpClient.EnableSsl = SSL;
			smtpClient.Credentials = new System.Net.NetworkCredential(UserName, Password);



			try
			{
				MailMessage mail = new MailMessage();
				mail.From = new MailAddress(from, from);
				mail.To.Add(to);
				mail.Subject = subject;


				mail.Attachments.Add(new System.Net.Mail.Attachment(stream, Filename));
				mail.Body = QVMail.LoadMailTemplate(TemplatePath).Replace("{body}", body);
				mail.IsBodyHtml = true;

				smtpClient.Send(mail);
			}
			catch (Exception ex)
			{
				success = false;
			}

			return success;
		}
	}

	# endregion

	namespace App
	{
		# region PrayerTimes
		public static class PrayerTimes
		{
			/// <summary>
			/// Get the prayer times of the Mekka city in a dictionary with 7 items which are {اليوم,الفجر,الشروق,الظهر,المغرب,العشاء}
			/// </summary>
			/// <returns></returns>
			public static Dictionary<string, string> GetPrayerTimes()
			{
				if (HttpContext.Current.Cache["PrayerTimes"] != null)
				{
					return (Dictionary<string, string>)HttpContext.Current.Cache["PrayerTimes"];
				}
				WebClient client = new WebClient();
				StreamReader sr = new StreamReader(client.OpenRead("http://www.islamicfinder.net/prayer_service.php?country=saudi_arabia&city=makkah&state=&zipcode=&latitude=21.4200&longitude=39.8300&timezone=3&HanfiShafi=1&pmethod=4&fajrTwilight1=10&fajrTwilight2=10&ishaTwilight=10&ishaInterval=30&dayLight=0&page_background=&table_background=&table_lines=&text_color=&lang=arabic"));
				byte[] buffer = new byte[] { };

				string file = sr.ReadToEnd().ToLower();
				Dictionary<string, string> result = new Dictionary<string, string>();
				while (file.Contains("<font color=\"#000000\">"))
				{
					string key = GetFirstStringInside(ref file, "<font color=\"#000000\">", "</font>");
					if (key.Contains("<b>"))
					{
						key = GetFirstStringInside(ref key, "<b>", "</b>");
					}

					string value = GetFirstStringInside(ref file, "<font color=\"#000000\">", "</font>");
					if (value.Contains("<b>"))
					{
						value = GetFirstStringInside(ref value, "<b>", "</b>");
					}
					result.Add(key, value);
				}
				HttpContext.Current.Cache["PrayerTimes"] = result;
				return result;
			}
			public static bool CheckURl(string strUrl)
			{
				Uri urlCheck = new Uri(strUrl);
				WebRequest request = WebRequest.Create(urlCheck);
				request.Timeout = 15000;
				WebResponse response;
				try
				{
					response = request.GetResponse();
					return true;
				}
				catch (Exception)
				{
					return false; //url does not exist
				}
			}
			public static bool CheckPrytimeURl()
			{
				Uri urlCheck = new Uri("http://www.islamicfinder.net/prayer_service.php?country=saudi_arabia&city=makkah&state=&zipcode=&latitude=21.4200&longitude=39.8300&timezone=3&HanfiShafi=1&pmethod=4&fajrTwilight1=10&fajrTwilight2=10&ishaTwilight=10&ishaInterval=30&dayLight=0&page_background=&table_background=&table_lines=&text_color=&lang=arabic");
				WebRequest request = WebRequest.Create(urlCheck);
				request.Timeout = 15000;
				WebResponse response;
				try
				{
					response = request.GetResponse();
					return true;
				}
				catch (Exception)
				{
					return false; //url does not exist
				}
			}
			public static List<KeyValuePair<string, string>> GetPrayerTimess()
			{
				if (HttpContext.Current.Cache["PrayerTimes"] != null)
				{
					return (List<KeyValuePair<string, string>>)HttpContext.Current.Cache["PrayerTimes"];
				}
				WebClient client = new WebClient();
				List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

				StreamReader sr = new StreamReader(client.OpenRead("http://www.islamicfinder.net/prayer_service.php?country=saudi_arabia&city=makkah&state=&zipcode=&latitude=21.4200&longitude=39.8300&timezone=3&HanfiShafi=1&pmethod=4&fajrTwilight1=10&fajrTwilight2=10&ishaTwilight=10&ishaInterval=30&dayLight=0&page_background=&table_background=&table_lines=&text_color=&lang=arabic"));
				byte[] buffer = new byte[] { };

				string file = sr.ReadToEnd().ToLower();

				while (file.Contains("<font color=\"#000000\">"))
				{
					string key = GetFirstStringInside(ref file, "<font color=\"#000000\">", "</font>");
					if (key.Contains("<b>"))
					{
						key = GetFirstStringInside(ref key, "<b>", "</b>");
					}

					string value = GetFirstStringInside(ref file, "<font color=\"#000000\">", "</font>");
					if (value.Contains("<b>"))
					{
						value = GetFirstStringInside(ref value, "<b>", "</b>");
					}
					result.Add(new KeyValuePair<string, string>(key, value));

				}
				HttpContext.Current.Cache["PrayerTimes"] = result;


				return result;
			}

			/// <summary>
			/// Get the string value between the two specified strings in the source string
			/// </summary>
			/// <param name="source"></param>
			/// <param name="firstString"></param>
			/// <param name="secondString"></param>
			/// <returns></returns>
			private static string GetFirstStringInside(ref string source, string firstString, string secondString)
			{
				string result;
				result = source.Substring(source.IndexOf(firstString) + firstString.Length, source.IndexOf(secondString, source.IndexOf(firstString)) - source.IndexOf(firstString) - firstString.Length);
				source = source.Remove(source.IndexOf(firstString), source.IndexOf(secondString, source.IndexOf(firstString)) - source.IndexOf(firstString) + secondString.Length);
				return result;
			}

			public static string GetNextPrayTime()
			{
				string result = "";
				var prayerTimes = GetPrayerTimes();
				foreach (var item in prayerTimes)
				{
					if (!item.Value.Contains(":")) continue;
					int hour = ((GetDayNightMode(item) == "ص") ? Numeric.ParseInt(item.Value.Split(':')[0]) : Numeric.ParseInt(item.Value.Split(':')[0]) + 12).Value;
					if (hour == 24) hour = 12;
					DateTime prayerTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, Numeric.ParseInt(item.Value.Split(':')[1]).Value, 0);
					if (DateTime.Now < prayerTime)
					{
						result = item.Key;
						break;
					}

				}
				return result;
			}


			public static string GetDayNightMode(KeyValuePair<string, string> prayerTime)
			{
				switch (prayerTime.Key)
				{
					case "الفجر":
						return "ص";
					case "الشروق":
						return "ص";
					case "ظهر":
						if (Numeric.ParseInt(prayerTime.Value.Split(':')[0]) < 12)
						{
							return "ص";
						}
						else
						{
							return "م";
						}
				}
				return "م";
			}
		}
		# endregion

		# region CaptchaImage
		public class CaptchaImage
		{
			// Public properties (all read-only).
			public string Text
			{
				get { return this.text; }
			}
			public Bitmap Image
			{
				get { return this.image; }
			}
			public int Width
			{
				get { return this.width; }
			}
			public int Height
			{
				get { return this.height; }
			}

			// Internal properties.
			private string text;
			private int width;
			private int height;
			private string familyName;
			private Bitmap image;

			// For generating random numbers.
			private Random random = new Random();

			// ====================================================================
			// Initializes a new instance of the CaptchaImage class using the
			// specified text, width and height.
			// ====================================================================
			public CaptchaImage(string s, int width, int height)
			{
				this.text = s;
				this.SetDimensions(width, height);
				this.GenerateImage();
			}

			// ====================================================================
			// Initializes a new instance of the CaptchaImage class using the
			// specified text, width, height and font family.
			// ====================================================================
			public CaptchaImage(string s, int width, int height, string familyName)
			{
				this.text = s;
				this.SetDimensions(width, height);
				this.SetFamilyName(familyName);
				this.GenerateImage();
			}

			// ====================================================================
			// This member overrides Object.Finalize.
			// ====================================================================
			~CaptchaImage()
			{
				Dispose(false);
			}

			// ====================================================================
			// Releases all resources used by this object.
			// ====================================================================
			public void Dispose()
			{
				GC.SuppressFinalize(this);
				this.Dispose(true);
			}

			// ====================================================================
			// Custom Dispose method to clean up unmanaged resources.
			// ====================================================================
			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
					// Dispose of the bitmap.
					this.image.Dispose();
			}

			// ====================================================================
			// Sets the image width and height.
			// ====================================================================
			private void SetDimensions(int width, int height)
			{
				// Check the width and height.
				if (width <= 0)
					throw new ArgumentOutOfRangeException("width", width, "Argument out of range, must be greater than zero.");
				if (height <= 0)
					throw new ArgumentOutOfRangeException("height", height, "Argument out of range, must be greater than zero.");
				this.width = width;
				this.height = height;
			}

			// ====================================================================
			// Sets the font used for the image text.
			// ====================================================================
			private void SetFamilyName(string familyName)
			{
				// If the named font is not installed, default to a system font.
				try
				{
					Font font = new Font(this.familyName, 12F);
					this.familyName = familyName;
					font.Dispose();
				}
				catch (Exception ex)
				{
					this.familyName = System.Drawing.FontFamily.GenericSerif.Name;
				}
			}

			// ====================================================================
			// Creates the bitmap image.
			// ====================================================================
			private void GenerateImage()
			{
				// Create a new 32-bit bitmap image.
				Bitmap bitmap = new Bitmap(this.width, this.height, PixelFormat.Format32bppArgb);

				// Create a graphics object for drawing.
				Graphics g = Graphics.FromImage(bitmap);
				g.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle rect = new Rectangle(0, 0, this.width, this.height);

				// Fill in the background.
				HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.SteelBlue, Color.White);
				g.FillRectangle(hatchBrush, rect);

				// Set up the text font.
				SizeF size;
				float fontSize = rect.Height + 1;
				Font font;
				// Adjust the font size until the text fits within the image.
				do
				{
					fontSize--;
					font = new Font(this.familyName, fontSize, FontStyle.Bold);
					size = g.MeasureString(this.text, font);
				} while (size.Width > rect.Width);

				// Set up the text format.
				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;

				// Create a path using the text and warp it randomly.
				GraphicsPath path = new GraphicsPath();
				path.AddString(this.text, font.FontFamily, (int)font.Style, font.Size, rect, format);
				float v = 4F;
				PointF[] points =
			{
				new PointF(this.random.Next(rect.Width) / v, this.random.Next(rect.Height) / v),
				new PointF(rect.Width - this.random.Next(rect.Width) / v, this.random.Next(rect.Height) / v),
				new PointF(this.random.Next(rect.Width) / v, rect.Height - this.random.Next(rect.Height) / v),
				new PointF(rect.Width - this.random.Next(rect.Width) / v, rect.Height - this.random.Next(rect.Height) / v)
			};
				Matrix matrix = new Matrix();
				matrix.Translate(0F, 0F);
				path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

				// Draw the text.
				hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.SteelBlue, Color.SteelBlue);
				g.FillPath(hatchBrush, path);

				// Add some random noise.
				int m = Math.Max(rect.Width, rect.Height);
				for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
				{
					int x = this.random.Next(rect.Width);
					int y = this.random.Next(rect.Height);
					int w = this.random.Next(m / 50);
					int h = this.random.Next(m / 50);
					g.FillEllipse(hatchBrush, x, y, w, h);
				}

				// Clean up.
				font.Dispose();
				hatchBrush.Dispose();
				g.Dispose();

				// Set the image.
				this.image = bitmap;
			}
		}
		# endregion
	}
}

