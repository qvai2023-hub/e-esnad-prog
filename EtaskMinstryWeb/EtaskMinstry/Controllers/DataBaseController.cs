using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Controllers
{
    public class DataBaseController : Controller
    {
        //
        // GET: /DataBase/

        public ActionResult Index()
        
        {
            Dictionary<int, string> allDatabaseList = GetDatabaseList();
            Dictionary<int, string> allTablesList = GetTablesList();
            ViewBag.allDatabases = new SelectList(allDatabaseList.OrderBy(x => x.Value).ToList(), "Key", "Value");

            ViewBag.AllTables = new SelectList(allTablesList.OrderBy(x => x.Value).ToList(), "Key", "Value");
            
            return View();
        }
         public string getConnectionString() 
         {
             
             string conString = "data source=172.16.16.30;initial catalog=EtaskMCloud_26_3_2018;user id=sa;password=111@admin;Connection Timeout=60;multipleactiveresultsets=True;application name=EntityFramework&quot;";
       
        return conString;
        
        }
        public  Dictionary<int, string> GetTablesList()
        {
            List<string> list = new List<string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            // Open connection to the database
             string conString= getConnectionString() ;
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.tables", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (dr.Read())
                        {

                            dict.Add(index ++, dr[0].ToString());
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return dict;

        }

        public Dictionary<int, string> GetDatabaseList()
        {
            List<string> list = new List<string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            // Open connection to the database
           string conString= getConnectionString() ;
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (dr.Read())
                        {

                            dict.Add(index++, dr[0].ToString());
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return dict;

        }

        [HttpPost]
        public ActionResult getTableByID(int? tableID = 0, string tableName = null)
        {
            string conString = getConnectionString();
           DataTable dt = new DataTable();

            DataSet data;
            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM " + tableName, con))
                {
                   
                    try
                    {
                        SqlDataAdapter sqlAd = new SqlDataAdapter();
                        sqlAd.SelectCommand = cmd;
                        data = new DataSet();
                        sqlAd.Fill(data, "ActionType");

                        dt = data.Tables[0];
                      
                    }
                    catch (Exception) { }
                }
            }
        
            return View("~/Views/DataBase/getTableByID.cshtml", dt);
        }


    }
}
