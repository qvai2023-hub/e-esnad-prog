using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Resources
{
    public static class ValidationResource
    {
        // Regex Resources
        #region "Regex"

        public const string RevString = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]$";
        public const string RevString15 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,2}$";
        //public const string RevString50 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,50}$";
          public const string RevString50 = @"^[a-zA-Z0-9\u0600-\u06ff\s_-]{1,50}$";
        public const string RevString100 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,100}$";
       // public const string RevString150 = @"^[a-zA-Z0-9\u0600-\u06ff\s_-]{1,150}$";
        public const string RevString150 = @"^[a-zA-Z0-9 \u0600-\u06ff,.\s_-]{1,150}$";
        public const string RevString200 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,200}$";
        public const string RevString250 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,250}$";
        public const string RevString300 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,300}$";
        public const string RevString350 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,350}$";
        public const string RevString400 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,400}$";
        public const string RevString500 = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,500}$";

        public const string RevLenght150 = @"^[\s\S]{0,150}$";
       // public const string RevStringmax = @"^[a-zA-Z0-9\u0600-\u06ff,./\s_-]{1,}$";
        public const string RevStringmax = @"[^<>]{1,}$";
        public const string ValidPrice = "^[0-9]{1,3}([.,][0-9]{1,3})?$";
        public const string phone = @"^\(?[\d]{3}\)?[\s-]?[\d]{3}[\s-]?[\d]{4}$";
        public const string revMail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public const string revSearchMail = @"^[a-zA-Z0-9\u0600-\u06ff.@\s_-]{1,50}$";
        public const string revPassword = @"^(?=.*\d).{4,8}$";
        public const string revNumber50 = @"^[0-9]{1,50}$";
        public const string revNumberOnly = @"^[1-9]+[0-9]*$";
        public const string revDecemailNumber = @"\b[0-9]{1,18}\b";
        public const string revUrl = @"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&amp;=]*)?";
        public const string ImageExtensions = @"gif,png,jpeg,jpg";
        public const string FileExtensions = @"doc,docx,pdf,xls,gif,png,jpeg,jpg";
        public const string MaxImageSize = @"1024000";
        public const string MinHeightForEmployee = "240";
        public const string MinWidthForEmployee = "320";
        public const string ArVision = "31";
        public const string ArMession = "2";
        public const string Numbers = @"^\d{1,15}$";
        public const string MonthNumbers = @"^\d{3,60}$";
        public const string MobNumbersG = @"^\d{10,15}$";
        public const string MobNumbers = @"^\d{6,10}$";
        public const string revPhone = @"^(\+[1-9][0-9]*(\([0-9]*\)|-[0-9]*-))?[0]?[1-9][0-9\- ]*$";
        public const string revMobile = @"[\+]{0,1}(\d{10,13}|[\(][\+]{0,1}\d{2,}[\13)]*\d{5,13}|\d{2,6}[\-]{1}\d{2,13}[\-]*\d{3,13})";
        public const string ValidRegularPhone = @"[\+]{0,1}(\d{10,13}|[\(][\+]{0,1}\d{2,}[\13)]*\d{5,13}|\d{2,6}[\-]{1}\d{2,13}[\-]*\d{3,13})";
        public const string revNumber15 = @"^[0-9]{1,15}$";
        public const string numberonly = @"^\d{1,9}$";
        public const string ExtpectedTime = @"^[0-9]{1,3}([.][0-9]{1,2})?$";
   //. $ ^ { [ ( | ) *< > / + ? \
        //   [^<>;/]
        // [^<>`~!/@\#}$%:;)(^{&*=|'+]
        //public const string revloginMail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        //public const string revloginPassword = @"^(?=.*\d/</>/).{4,8}$";
            //@"[\%\/\\\&\?\,\'\;\:\!\<\-\>]+";
       // \!\
//@"^(?=.*\d/<>`~!/@\#}$%:;,/).{4,8}$";
//@"^[^<>`~!/@\#}$%:;,)(_^{&*=|'+]+$";
        // @"^[^<>`~!/@\#}$%:;)(^{&*=|'+](?=.*\d).{4,8}$";
        //          @"^[a-zA-Z0-9\u0600-\u06ff\s_-]{1,150}$"
        #endregion
    }
}