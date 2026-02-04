<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="EtaskMinstry.Areas.Company.Report.TasksReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />

    <style>
        #VisibleReportContentrepv_ctl09 table:first-child {
            margin: 0 auto;
        } 
         #VisibleReportContentrepv_ctl10 table:first-child {
            margin: 0 auto;
        } 

       div[id*="VisibleReportContentrepv"] table:nth-child(2) tr td:first-child {
           display: none;
           }

        table:first-child {
            margin: 0 auto;
        }

        div#repv_ctl05 div:first-child {
            background-color: #FAF9F9;
            color: #024A63;
        }

        img#repv_ctl05_ctl04_ctl00_ButtonImg {
            transform: rotateY(180deg);
        }

        .reportviewer {
            background-color: white;
        }

        table td, table th {
            padding: 0px 0px;
            text-align: center;
            word-break:break-all;
        }

        div[id$="ReportDiv"] table:first-child {
            margin: auto;
        }

        [id$="ReportCell"] div:first-child {
            margin: auto;
        }

        [id$="ReportCell"] > table {
            margin: auto;
        }

        div > table {
            margin: auto;
        }
        table {
            margin: auto;
        }
        div#repv_ctl10 {
            overflow: visible !important;
        }
        #Pf5ac46846308459c802dc83576d3532a_1_oReportDiv{
   overflow-x :scroll;
}  

    </style>
</head>
<body dir="rtl" style="width: 100%; height: 100%; ">
    <form id="form1" runat="server">
        <%--    <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>

        <div style="text-align:center; width: 990px; height:100%; overflow: hidden; margin: 0 auto;">
            <rsweb:reportviewer height="100%" width="95%" id="repv" runat="server" style="margin: 0 auto;">
            </rsweb:reportviewer>
            &nbsp;
        </div>--%>


        <div id="ReportDiv" class="ReportClass" style="direction: rtl; text-align: center;">
            <asp:ScriptManager ID="ScriptManager2" runat="server"></asp:ScriptManager>

            <rsweb:ReportViewer CssClass="reportviewer" ID="repv" Width="100%" Height="100%"
                Font-Names="Tahoma" Font-Size="8pt" interactivedeviceinfos="(Collection)"
                WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Font-Bold="True"
                Font-Italic="False" Style="direction: rtl; margin: 0 auto;" PageCountMode="Actual" runat="server" ProcessingMode="Local">
                <ServerReport />
            </rsweb:ReportViewer>
        </div>

    </form>
</body>
</html>
<%--<script src="../Content/ControlPanel/js/jquery-1.11.0.min.js"></script>--%>
<script src="../Scripts/jquery.js"></script>
<%--<script src="../Content/js/jquery-1.11.0.min.js"></script>--%>
<script type="text/javascript">

    $(function () {

        $("div[id$='AsyncWait_Wait']").html('<div><img src="../Scripts/QVUploader/images/AjaxLoader.gif" /><br />يتم تحميل الملف ...</div>');
        var sel = $("[id$=Menu]");
        sel.find("a[title*='PDF']").text("استخراج إلى PDF");
        //sel.find("a[title*='Word']").text("استخراج إلى Word");
        sel.find("a[title*='Excel']").text("استخراج إلى Excel");
        sel.find("a[title*='Word']").remove();
        //sel.find("a[title*='Excel']").remove();
        sel.find("a[title*='XML']").remove();
        sel.find("a[title*='CSV']").remove();
        sel.find("a[title*='MHTML']").remove();
        sel.find("a[title*='TIFF']").remove();
        sel.css({ 'z-index': '1000', 'width': '150px' });
        $("[id$=Menu]").css('left', '140px !important');
        $("[id$=Menu]").css('border', '2px solid red !important');
        $('span').filter(function () { return ($(this).text() === 'of') }).text('من');
        $('a[title="Find"]').text('بحث');
        $('a[title="Find Next"]').text('التالي');
        $('a[title="Refresh"]').remove();
    });

    // resize report viewer
    // ResizeReport();
    function ResizeReport() {
        var viewer = document.getElementById('<%= repv.ClientID %>');
        var htmlheight = document.documentElement.clientHeight;
        viewer.style.height = (htmlheight - 30) + "px";
    }
    window.onresize = function resize() { ResizeReport(); }

    $('img').attr("src", "/Reserved.ReportViewerWebControl.axd?Culture=1033&amp;CultureOverrides=True&amp;UICulture=1033&amp;UICultureOverrides=True&amp;ReportStack=1&amp;ControlID=554eb148b2bf45ffbff932ff463e25c5&amp;Mode=true&amp;OpType=ReportImage&amp;ResourceStreamID=Blank.gif").last().hide()
    $('table [title="Refresh"]').remove();

    $.each($('table'), function (i, e) {
        console.log(i)
        console.log(e.innerHTML)
    })
</script>


<script>

    $(function () {
        debugger;

        //for local
        $('[id$="VisibleReportContentrepv_ctl09"]').bind("DOMNodeInserted", function () {

            $('[id$="VisibleReportContentrepv_ctl09"]').find("[id*='ReportDiv'] > table").css({ "margin": "auto" });
         //to be report responsive
            $('[id$="VisibleReportContentrepv_ctl09"]').find("[id*='ReportDiv']").css({ "overflow-x": "scroll" });
         
        });


        //for test
        $('[id$="VisibleReportContentrepv_ctl10"]').bind("DOMNodeInserted", function () {

            $('[id$="VisibleReportContentrepv_ctl10"]').find("[id*='ReportDiv'] > table").css({ "margin": "auto" });
            //to be report responsive
            $('[id$="VisibleReportContentrepv_ctl10"]').find("[id*='ReportDiv']").css({ "overflow-x": "scroll" });

        });


        //for test 
        $("#repv_ctl06_ctl04_ctl00_Menu").find("a")[0].innerHTML = "استخراج إلى Excel";
        $("#repv_ctl06_ctl04_ctl00_Menu").find("a")[1].innerHTML = "استخراج إلى PDF";
        $("#repv_ctl06_ctl04_ctl00_Menu").find("a")[2].remove()
        $("#repv_ctl06_ctl04_ctl00_Menu").css('left', '140px !important');
        $("#repv_ctl06_ctl04_ctl00_Menu").css('border', '2px solid red !important');



        $("table").find("[id*='ReportDiv'] > table").css({ "margin": "auto" });
        $('div[id$="ReportDiv"]').find("table:first").css("margin", "auto");
        $('[id$="ReportCell"]').find("div:first").css("margin", "auto");

       




    });

</script>
