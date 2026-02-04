using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EtaskMinstry.Models
{
    public enum FileTypes
    {
        WordAndPDF,
        Images
    }

    public class UploaderVM
    {
        public String strColumnName { get; set; }
        public String strFileName { get; set; }
        public FileTypes fileType { get; set; }
        public int iMaxFileSizeInMB { get; set; }
        public Boolean bRequired { get; set; }
        public Boolean bPreview { get; set; }
        public String iViewID { get; set; }

        public UploaderVM()
        {
            // Create Unique ID For Uploader
            iViewID = Guid.NewGuid().ToString().Substring(0, 5);
        }
    }
}