using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace EtaskMinstry.AppCode
{
    public class UploadFile
    {
        public string Uploadfile(HttpRequestBase currentRequest,string strPath)
        {
            Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
            FileHeader = Extentions.ValidHeaders(FileHeader);
            string fileName = "";
            byte[] header;
            for (int i = 0; i < currentRequest.Files.Count; i++)
            {
                if (currentRequest.Files[i].ContentLength > 0 && currentRequest.Files[i].ContentLength <= int.Parse(ConfigurationManager.AppSettings["maxfileSize"].ToString()))
                {
                    if (Extentions.CheckMimeType(currentRequest.Files[i].ContentType))
                    {
                        string fileExt;
                        fileExt = currentRequest.Files[i].FileName.Substring(currentRequest.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
                        byte[] tmp = FileHeader[fileExt];
                        header = new byte[tmp.Length];

                        // GET HEADER INFORMATION OF UPLOADED FILE
                        currentRequest.Files[i].InputStream.Read(header, 0, header.Length);

                        if (Extentions.CompareArray(tmp, header))
                        {
                            //Valid
                            string strFileName = Guid.NewGuid().ToString() +
                                         Path.GetExtension(currentRequest.Files[i].FileName);
                            currentRequest.Files[i].SaveAs(HttpContext.Current.Server.MapPath(strPath + strFileName));
                            fileName = strFileName;
                        }
                        else
                        {
                            //InValid
                            return "FAILED";
                        }
                    }
                }
            }
            return fileName;
        }

      

    }
}