<%@ WebHandler Language="C#" Class="ImageUpload" %>

using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

public class ImageUpload : IHttpHandler {

    private const string ServerSafeFileExtentions = "png,jpg,jpeg,gif,bmp,svg,pdf,doc,docx,xls,xlsx,ppt,pptx,mp3,mp4";

    public void ProcessRequest(HttpContext context)
    {
      
        string folder= context.Request.Form["folder"];
        string path =  folder;
        
        
        if (context.Request.Files.Count > 0)
        {
            HttpPostedFile file = context.Request.Files[0];
            string strExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

            string strFileName = Guid.NewGuid().ToString() + strExtension;
            string strSaveLocation = context.Server.MapPath(path)  + strFileName;
           // try
          //  {
                // check allowed extensions to prevent attacks using malware
                if (!ServerSafeFileExtentions.Split(',').Contains(strExtension.Replace(".", ""))) throw new Exception("File extension is rejected from server, " + strExtension);

                // save file
                context.Request.Files[0].SaveAs(strSaveLocation);
         //   }
         //  catch 
         //   {
          //      context.Response.Write("[{status:'fail'}]");
         //   }
            context.Response.ContentType = "text/html";
            context.Response.Write("[{status:'done', filename:'" + strFileName + "'}]");
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }
}
