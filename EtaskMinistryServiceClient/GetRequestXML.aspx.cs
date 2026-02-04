using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class GetRequestXML : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        HttpContext.Current.Response.ContentType = "text/xml";
        HttpContext.Current.Response.Write(Session["request"] != null ? Session["request"].ToString() : "");
    }
}