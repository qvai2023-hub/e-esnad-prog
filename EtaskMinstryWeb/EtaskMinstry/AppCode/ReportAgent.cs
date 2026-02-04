using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace EtaskMinstry.AppCode
{
   
    public class ReportAgent
    {
        public static List<ReportParameter> ReportParameters
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["ReportParameters"] == null)
                    System.Web.HttpContext.Current.Session["ReportParameters"] = new List<ReportParameter>();
                return System.Web.HttpContext.Current.Session["ReportParameters"] as List<ReportParameter>;
            }
        }

        // get lst
        //desirialize
        //add item
        // serialize lst
        //  set in session
        public static bool AddReportDataSources(ReportDataSource objDS)
        {
           string session = System.Web.HttpContext.Current.Session["ReportDataSources"].ToString();
           
           List<ReportDataSource> items = new List<ReportDataSource>();
           items.Add(objDS);
           var json = JsonConvert.SerializeObject(items);
             System.Web.HttpContext.Current.Session["ReportDataSources"] = json;
          
           return true;

     
        }

        public static List<ReportDataSource> ReportDataSources
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["ReportDataSources"] == null)
                {
                    System.Web.HttpContext.Current.Session["ReportDataSources"] = JsonConvert.SerializeObject(new List<ReportDataSource>()); // SerializeObject(new List<ReportDataSource>());
                }

                List<ReportDataSource> deserializeSession =  JsonConvert.DeserializeObject(System.Web.HttpContext.Current.Session["ReportDataSources"].ToString(), typeof(List<ReportDataSource>)) as List<ReportDataSource>;
                if (deserializeSession.Count() > 0)
                {
                    string valueDeserializeSession = deserializeSession[0].Value.ToString();
                    if (deserializeSession[0].Name == "DS_ProjectTasks")
                    {
                        List<TaskManagementModel.sp_ProjectTasks_Result> deserializeProjectTasksResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_ProjectTasks_Result>)) as List<TaskManagementModel.sp_ProjectTasks_Result>;
                        deserializeSession[0].Value = deserializeProjectTasksResult;
                    }
                    else if (deserializeSession[0].Name == "DS_CompanyTasks")
                    {
                        List<TaskManagementModel.sp_CompanyTasks_Result> deserializeCompanyTasksResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_CompanyTasks_Result>)) as List<TaskManagementModel.sp_CompanyTasks_Result>;
                        deserializeSession[0].Value = deserializeCompanyTasksResult;
                    }

                    else if (deserializeSession[0].Name == "DS_TotalEmployeeTasks")
                    {
                        List<TaskManagementModel.sp_TotalEmployeeTasks_Result> deserializeTotalEmployeeTasksResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_TotalEmployeeTasks_Result>)) as List<TaskManagementModel.sp_TotalEmployeeTasks_Result>;
                        deserializeSession[0].Value = deserializeTotalEmployeeTasksResult;
                    }
                    else if (deserializeSession[0].Name == "DS_attendance")
                    {
                        List<TaskManagementModel.sp_Attendance_Result> deserializeResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_Attendance_Result>)) as List<TaskManagementModel.sp_Attendance_Result>;
                        deserializeSession[0].Value = deserializeResult;
                    }
                    else if (deserializeSession[0].Name == "DS_EmployeePerformance")
                    {
                        List<TaskManagementModel.sp_EmployeePerformanceReport_Result> deserializeResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_EmployeePerformanceReport_Result>)) as List<TaskManagementModel.sp_EmployeePerformanceReport_Result>;
                        deserializeSession[0].Value = deserializeResult;
                    }
                    else if (deserializeSession[0].Name == "DS_EmpReport")
                    {
                        List<TaskManagementModel.sp_EmployeesReport1_Result> deserializeResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_EmployeesReport1_Result>)) as List<TaskManagementModel.sp_EmployeesReport1_Result>;
                        deserializeSession[0].Value = deserializeResult;
                    }
                    else if (deserializeSession[0].Name == "DataSet1")
                    {
                        List<TaskManagementModel.sp_BriefTasksReport2_Result> deserializeResult = JsonConvert.DeserializeObject(valueDeserializeSession, typeof(List<TaskManagementModel.sp_BriefTasksReport2_Result>)) as List<TaskManagementModel.sp_BriefTasksReport2_Result>;
                        deserializeSession[0].Value = deserializeResult;
                    }
                }
                return deserializeSession;
            }
        }


        public static SubreportProcessingEventHandler SubreportProcEventHandler
        {
            get { return System.Web.HttpContext.Current.Session["SubreportProcEventHandler"] as SubreportProcessingEventHandler; }
            set { System.Web.HttpContext.Current.Session["SubreportProcEventHandler"] = value; }
        }


      

    }
}