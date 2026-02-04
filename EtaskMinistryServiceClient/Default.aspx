<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Scripts/jquery-1.7.1.min.js"></script>
        <script src="/Scripts/jquery-ui.min.js"></script>
    <link href="Styles/jquery-ui.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        

<style>
    #results table {
    
           width:100%;
          

    }

    #results table th {
        font-weight:bold;
    }

    #results table th, td {
            border: 1px solid black;
    }



</style>

<h2>Test Service</h2>

From: <input type="text" id="fromDate" name="fromDate" runat="server"/> <br />
To: <input type="text" id="toDate" name="toDate" runat="server"/> <br />

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Get Results" />
        <br />
        <label runat="server" id="lblErrorMessage" style="color:red;"></label>
        <div runat="server"  id="requestXML" visible="false">
        <a href="GetRequestXML.aspx" target="_blank">Get request xml</a>
</div>


<div runat="server"  id="responseXML" visible="false">
        <a href="GetResponseXML.aspx" target="_blank">Get REsponse xml</a>
</div>


        <asp:ListView ID="resultsListView" runat="server">
             <LayoutTemplate>
          <table cellpadding="2" width="640px" border="1" runat="server" id="tblProducts">
                       <thead>
                <tr>
                    <th>ID Number</th>
                    <th>Sequence Number</th>
                    <th>Labor Office ID</th>
                    <th>Completed Tasks</th>
                    <th>Assigned Tasks</th>
                    <th>Total Work Time</th>
                    <th>Activity Level</th>
                    <th>Login Count</th>
                    <th>Logout Count</th>
                </tr>
            </thead>
              <tr runat="server" id="itemPlaceholder" />
          </table>
       
        </LayoutTemplate>
            <ItemTemplate>
                <tr>
             <td> <%#Eval("IdNumber") %> </td>
            <td> <%#Eval("EstSequenceNumber") %> </td>
            <td><%#Eval("EstLaborOfficeId") %> </td>
            <td> <%#Eval("CompletedTasks") %> </td>
            <td> <%#Eval("AssignedTasks") %> </td>
            <td> <%#Eval("TotalWorkTime") %> </td>
            <td> <%#Eval("ActivityLevel") %> </td>
            <td><%#Eval("LoginCount") %> </td>
            <td> <%#Eval("LogoutCount") %> </td>
            
         </tr>
               
            </ItemTemplate>

        </asp:ListView>


 




<script>
    $(document).ready(function () {
        $.datepicker.setDefaults({ dateFormat: '<%=ConfigurationManager.AppSettings["dateformat"]%>' });

        $("#fromDate").datepicker();
        $("#toDate").datepicker();

     
    });

</script>

        
    </form>
</body>
</html>
