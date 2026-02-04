using ETaskService;
using ETaskServiceClient.AppCode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }


    
    protected void Button1_Click(object sender, EventArgs e)
    {

        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; //to be removed in production
        var client = new EtaskClient();
        Session["response"] = null;
        Session["request"] = null;
        lblErrorMessage.InnerText = "";
        //This is for displaying XML values
        client.Endpoint.Behaviors.Add(new CustomOutputBehavior());
        try
        {
            string clientReference = ConfigurationManager.AppSettings["ClientReference"];
            Evaluation[] result = client.GetTeleworkersEvaluation(new GetTeleworkersEvaluations() { PeriodStart = fromDate.Value, PeriodEnd = toDate.Value });

            resultsListView.DataSource = result;
            resultsListView.DataBind();

            if (Session["request"] != null)
            {
                requestXML.Visible = true;
            }

            if (Session["response"] != null)
            {
                responseXML.Visible = true;
            }

        }
        catch (Exception ex)
        {
            lblErrorMessage.InnerText = ex.Message;
            resultsListView.DataSource = null;
            resultsListView.DataBind();
            requestXML.Visible = false;
            responseXML.Visible = false;
        }


        
    }
}