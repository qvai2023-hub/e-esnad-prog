using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.Models.Attachment;
using EtaskMinstry.Models.Company;
using EtaskMinstry.Models.Holiday;
using EtaskMinstry.Models.Priority;
using EtaskMinstry.Models.Status;
using EtaskMinstry.Models.Task;
using EtaskMinstry.Models.TimeUnit;
using EtaskMinstry.Models.WeekEnd;
using EtaskMinstry.Models.Project;
using EtaskMinstry.Models;
using TaskManagementModel;
using EtaskMinstry.AppCode;
using EtaskMinstry.Controllers;
using EtaskMinstry;


namespace EtaskMinstry.Areas.Company.Controllers
{
    public class TaskController : BaseCompanyController
    {
        private void FillDropDownLists()
        {
            ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name");
            ViewBag.TimeUnit = new SelectList(new TimeUnitDisplay().Get(), "ID", "Name");
            ViewBag.Priority = new SelectList(new PriorityDisplay().Get(), "ID", "Name");
            ViewBag.Status = new SelectList(new StatusDisplay().Get(), "ID", "Name");
            ViewBag.Employee = new SelectList(ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "id", "name");
            ViewBag.Tasks = new SelectList(new CompanyTaskVM().Select("", "", "", "", "", 0, 0, null, null, 0, 0), "TaskID","Task");
        }


        //public ActionResult SaveData(TaskAddEdit objTask)
        //{
        //    FillDropDownLists();

        //    if (String.IsNullOrEmpty(objTask.Title))
        //    {
        //        return View();
        //    }
        //    else
        //    {
        //        int iTaskID = objTask.Save();
        //        // Saved .
        //        if (iTaskID > 0)
        //        {
        //            // To Delete File .
        //            if (objTask.RemovedFiles != null)
        //            {
        //                List<String> RemovedFiles = objTask.RemovedFiles.Split(',').ToList();

        //                for (int i = 0; i < RemovedFiles.Count; i++)
        //                    if (!String.IsNullOrEmpty(RemovedFiles[i]))
        //                    {
        //                        if (System.IO.File.Exists(Server.MapPath("/Upload/Task/" + RemovedFiles[i])))
        //                            System.IO.File.Delete(Server.MapPath("/Upload/Task/" + RemovedFiles[i]));

        //                        new AttachmentDisplay().Delete(new AttachmentDisplay().GetAttachment(RemovedFiles[i]));
        //                    }
        //            }

        //            // Uploaded File Description .
        //            List<String> objDescription = (objTask.FilesDescription == null)
        //                                              ? new List<string>()
        //                                              : (objTask.FilesDescription.Split(',').ToList());

        //            if (objDescription.Count > 1)
        //            {
        //                objDescription.RemoveAt(0);
        //            }
        //            // Insert Attachments .

        //            if (Extentions.ValidateReCaptcha())
        //            {
        //                for (int i = 0; i < Request.Files.Count - 1; i++)
        //                    if (Request.Files[i].ContentLength > 0)
        //                    {
        //                        objTask.RecaptchCheck = false;

        //                        if (Extentions.CheckMimeType(Request.Files[i].ContentType))
        //                        {
        //                            Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
        //                            FileHeader = Extentions.ValidHeaders(FileHeader);
        //                            string fileName = "";
        //                            byte[] header;
        //                            string fileExt;
        //                            fileExt = Request.Files[i].FileName.Substring(Request.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
        //                            byte[] tmp = FileHeader[fileExt];
        //                            header = new byte[tmp.Length];
        //                            Request.Files[i].InputStream.Read(header, 0, header.Length);
        //                            if (Extentions.CompareArray(tmp, header))
        //                            {
        //                                //Valid
        //                                // if(Request.Files[i].FileName.Any(Request.Files[i].FileName))
        //                                string strFileName = Guid.NewGuid().ToString() +
        //                                                  Path.GetExtension(Request.Files[i].FileName);
        //                                Request.Files[i].SaveAs(Server.MapPath("/Upload/Task/" + strFileName));
        //                                fileName = strFileName;

        //                                var objAttachment = new AttachmentDisplay()
        //                                    {
        //                                        TaskID = iTaskID,
        //                                        FileName = fileName,
        //                                        Description = objDescription[i]
        //                                    };

        //                                new AttachmentDisplay().Insert(objAttachment);

        //                                // Log Attachment .
        //                                LogTask.LogAddAttachment(iTaskID, new Attachment()
        //                                    {
        //                                        AttachmentID = objAttachment.ID,
        //                                        Description = objAttachment.Description,
        //                                        FileName = objAttachment.FileName,
        //                                        TaskID = iTaskID
        //                                    });
        //                            }
        //                        }
        //                        objTask.RecaptchCheck = true;


        //                    }//if
        //            }//check recaptha
        //        }
        //    }
        //    return View("SaveData", objTask);
        //}


        public ActionResult SaveData()
        {
                FillDropDownLists();
                return View();
        }
        [HttpPost]
        public ActionResult SaveTask(TaskAddEdit objTask)
        {
            AssignTask(objTask);
            //if (objTask.EmployessLst == null)
            //{
            //    AssignTask(objTask);
            //}
            //else {
            //    foreach (var emp in objTask.EmployessLst)
            //    {
            //        objTask.EmpID = emp;
            //        AssignTask(objTask);
            //    }
            //}
            //int iTaskID = objTask.Save();
            //// Saved .
            //if (iTaskID > 0)
            //{
            //    // To Delete File .
            //    if (objTask.RemovedFiles != null)
            //    {
            //        List<String> RemovedFiles = objTask.RemovedFiles.Split(',').ToList();

            //        for (int i = 0; i < RemovedFiles.Count; i++)
            //            if (!String.IsNullOrEmpty(RemovedFiles[i]))
            //            {
            //                if (System.IO.File.Exists(Server.MapPath("/Upload/Task/" + RemovedFiles[i])))
            //                    System.IO.File.Delete(Server.MapPath("/Upload/Task/" + RemovedFiles[i]));

            //                //new AttachmentDisplay().Delete(new AttachmentDisplay().GetAttachment(RemovedFiles[i]));
            //                new AttachmentDisplay().Delete(new AttachmentDisplay().GetByID(int.Parse(RemovedFiles[i])));
            //            }
            //    }

            //    // Uploaded File Description .
            //    List<String> objDescription = (objTask.FilesDescription == null)
            //                                      ? new List<string>()
            //                                      : (objTask.FilesDescription.Split(',').ToList());

            //    if (objDescription.Count > 1)
            //    {
            //        objDescription.RemoveAt(0);
            //    }
            //    // Insert Attachments .

            //    //if (Extentions.ValidateReCaptcha())
            //    //{
            //        for (int i = 0; i < Request.Files.Count - 1; i++)
            //            if (Request.Files[i].ContentLength > 0)
            //            {
            //                objTask.RecaptchCheck = false;

            //                if (Extentions.CheckMimeType(Request.Files[i].ContentType))
            //                {
            //                    Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
            //                    FileHeader = Extentions.ValidHeaders(FileHeader);
            //                    string fileName = "";
            //                    byte[] header;
            //                    string fileExt;
            //                    fileExt = Request.Files[i].FileName.Substring(Request.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
            //                    byte[] tmp = FileHeader[fileExt];
            //                    header = new byte[tmp.Length];
            //                    Request.Files[i].InputStream.Read(header, 0, header.Length);
            //                    if (Extentions.CompareArray(tmp, header))
            //                    {
            //                        //Valid
            //                        // if(Request.Files[i].FileName.Any(Request.Files[i].FileName))
            //                        string strFileName = Guid.NewGuid().ToString() +
            //                                          Path.GetExtension(Request.Files[i].FileName);
            //                        Request.Files[i].SaveAs(Server.MapPath("/Upload/Task/" + strFileName));
            //                        fileName = strFileName;

            //                        var objAttachment = new AttachmentDisplay()
            //                        {
            //                            TaskID = iTaskID,
            //                            FileName = fileName,
            //                            Description = objDescription[i]
            //                        };

            //                        new AttachmentDisplay().Insert(objAttachment);

            //                        // Log Attachment .
            //                        LogTask.LogAddAttachment(iTaskID, new Attachment()
            //                        {
            //                            AttachmentID = objAttachment.ID,
            //                            Description = objAttachment.Description,
            //                            FileName = objAttachment.FileName,
            //                            TaskID = iTaskID
            //                        });
            //                    }
            //                }
            //                objTask.RecaptchCheck = true;


            //            }//if

            //    //}//check recaptha
            //}
            objTask.RecaptchCheck = true;
            
                if (objTask.TaskID != 0 && objTask.TaskID != null)
                {
                    return Redirect("~/Company/Company/Index");
                }
                else
                {
                    if (objTask.RecaptchCheck == null || objTask.RecaptchCheck == true)

                    {
                        TempData["RecaptchCheck"] = objTask.RecaptchCheck;
                        TempData["Saved"] = objTask.Saved;
                        return RedirectToAction("SaveData");
                    }
                    else
                    {
                        FillDropDownLists();
                        return View("SaveData", objTask);
                    }
                }
            
                //return Json(new
                //{
                //    Saved = objTask.Saved,
                //    RecaptchCheck = objTask.RecaptchCheck
                //}, JsonRequestBehavior.AllowGet);
                //return Json(objTask);
        }


        /// <summary>
        /// Contoller to check country Code valdiation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        // [AllowAnonymous]
        public ActionResult checkDublicated(string Title, int TaskID = 0)
        {
            bool isDublicated = new TaskAddEdit().CheckDublication(TaskID, Title);
            return Json(!isDublicated, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Check Days Count Without Count Holidays and WeekEnds .
        /// </summary>
        /// <param name="dtStartDate">Task Start Date</param>
        /// <param name="dtEndDate">Task End Date</param>
        /// <param name="iTimeCount">Time Count</param>
        /// <param name="iTimeUnit">Time Unit</param>
        /// <returns></returns>
        [HttpPost]
        public Boolean DateDifference(String dtStartDate, String dtEndDate, Double iTimeCount, int iTimeUnit)
        {
            DateTime StartDate = dtStartDate.ToGregExactformate();

            DateTime EndDate = dtEndDate.ToGregExactformate();

            Double iTotalDays = (EndDate - StartDate).TotalDays + 1;

            // Get Holidays .
            int Holidays = 0;

            // Get All Company WeekEnds .
            List<WeekEndDisplayVM> lstWeekEnds =
                new WeekEndDisplayVM().GetByCompany(MvcApplication.userData.userId);

            // Get All Company HoliDays .
            List<HolidayDisplayVM> lstHoliDays =
                new HolidayDisplayVM().GetByCompany(MvcApplication.userData.userId);

            for (int i = 0; i < iTotalDays; i++)
            {
                DateTime dtToCheckDate = StartDate.AddDays(i);

                // HoliDays .
                if (lstHoliDays.Any(d => dtToCheckDate >= d.StartDate && dtToCheckDate <= d.EndDate))
                    Holidays++;
                else // Week Ends .
                    Holidays += lstWeekEnds.Count(w =>
                                                  dtToCheckDate.DayOfWeek ==
                                                  (DayOfWeek)Enum.Parse(typeof(DayOfWeek), w.WeekEndDay.ToString()));
            }

            iTotalDays = iTotalDays - Holidays;

            switch (iTimeUnit)
            {
                // Convert iTotalDays To Months .
                case (int)TimeUnit.Month:
                    // Check If iTotalDays After Converting To Month < iTimeCount .
                    if (iTimeCount > iTotalDays / ((int)TimeUnitValue.Day * (int)TimeUnitValue.Week + 2))
                        return false;
                    break;

                // Convert iTotalDays To Weeks .
                case (int)TimeUnit.Week:
                    // Check If iTotalDays After Converting To Weeks < iTimeCount .
                    if (iTimeCount > iTotalDays / (int)TimeUnitValue.Day)
                        return false;
                    break;

                case (int)TimeUnit.Day:
                    if (iTimeCount > iTotalDays)
                        return false;
                    break;

                // Convert iTotalDays To Hours .
                case (int)TimeUnit.Hour:
                    // Check If iTotalDays After Converting To Hour < iTimeCount .
                    if (iTimeCount > iTotalDays * (int)TimeUnitValue.Hour)
                        return false;
                    break;

                // Convert iTotalDays To Minutes .
                case (int)TimeUnit.Minute:
                    // Check If iTotalDays After Converting To Minute < iTimeCount .
                    if (iTimeCount > iTotalDays * (int)TimeUnitValue.Hour * (int)TimeUnitValue.Minute)
                        return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// To Check Dates .
        /// </summary>
        /// <param name="dtStartDate"></param>
        /// <param name="dtEndDate"></param>
        /// <returns></returns>
        [HttpPost]
        public String CheckDate(String dtStartDate, String dtEndDate)
        {
            DateTime? startDate = null;

            DateTime? endDate = null;
            //if (!String.IsNullOrEmpty(dtStartDate))
            //    startDate =  dtStartDate.ToGregExactformate();

            //if (!String.IsNullOrEmpty(dtEndDate))
            //    endDate =  dtEndDate.ToGregExactformate() ;

            if (!String.IsNullOrEmpty(dtStartDate))
                startDate = (MvcApplication.IsGregDate ? dtStartDate.ToGregExactformate() : dtStartDate.ToGregExact());

            if (!String.IsNullOrEmpty(dtEndDate))
                endDate = (MvcApplication.IsGregDate ? dtEndDate.ToGregExactformate() : dtEndDate.ToGregExact());

            // Check Start Date and End Date .
            if (startDate.HasValue && endDate.HasValue && (DateTime.Compare(startDate.Value, endDate.Value) > 0))
                return "لا يجب ان يكون تاريخ النهاية قبل تاريخ البداية ";

            // Check Start Date and Today Date .
            if (dtStartDate != "" && (DateTime.Compare(startDate.Value, DateTime.Now.Date) < 0))
                return "لا يجب ان يكون تاريخ البداية قبل تاريخ اليوم";

            // Check End Date and Today Date .
            if (dtEndDate != "" && (DateTime.Compare(endDate.Value, DateTime.Now.Date) < 0))
                return "لا يجب ان يكون تاريخ النهاية قبل تاريخ اليوم";

            return "valid";
        }

        [HttpPost]
        public ActionResult GetTask(int taskId)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            var task =new TaskAddEdit().gettask(taskId);
            Data.Add("Title",task.Title);
            Data.Add("Description",task.Description);           
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EDitTask(int taskId)
        {
            var task = new TaskAddEdit().Select(taskId);

            FillDropDownLists();
            return View(task);
        }


        //public TaskAddEdit AssignTask(TaskAddEdit objTask)
        //{

        //    int iTaskID = objTask.Save();
        //    // Saved .
        //    if (iTaskID > 0)
        //    {
        //        // To Delete File .
        //        if (objTask.RemovedFiles != null)
        //        {
        //            List<String> RemovedFiles = objTask.RemovedFiles.Split(',').ToList();

        //            for (int i = 0; i < RemovedFiles.Count; i++)
        //                if (!String.IsNullOrEmpty(RemovedFiles[i]))
        //                {
        //                    if (System.IO.File.Exists(Server.MapPath("/Upload/Task/" + RemovedFiles[i])))
        //                        System.IO.File.Delete(Server.MapPath("/Upload/Task/" + RemovedFiles[i]));

        //                    //new AttachmentDisplay().Delete(new AttachmentDisplay().GetAttachment(RemovedFiles[i]));
        //                    new AttachmentDisplay().Delete(new AttachmentDisplay().GetByID(int.Parse(RemovedFiles[i])));
        //                }
        //        }

        //        // Uploaded File Description .
        //        List<String> objDescription = (objTask.FilesDescription == null)
        //                                          ? new List<string>()
        //                                          : (objTask.FilesDescription.Split(',').ToList());

        //        if (objDescription.Count > 1)
        //        {
        //            objDescription.RemoveAt(0);
        //        }
        //        // Insert Attachments .

        //        //if (Extentions.ValidateReCaptcha())
        //        //{
        //        for (int i = 0; i < Request.Files.Count - 1; i++)
        //            if (Request.Files[i].ContentLength > 0)
        //            {
        //                objTask.RecaptchCheck = false;

        //                if (Extentions.CheckMimeType(Request.Files[i].ContentType))
        //                {
        //                    Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
        //                    FileHeader = Extentions.ValidHeaders(FileHeader);
        //                    string fileName = "";
        //                    byte[] header;
        //                    string fileExt;
        //                    fileExt = Request.Files[i].FileName.Substring(Request.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
        //                    byte[] tmp = FileHeader[fileExt];
        //                    header = new byte[tmp.Length];
        //                    Request.Files[i].InputStream.Read(header, 0, header.Length);
        //                    if (Extentions.CompareArray(tmp, header))
        //                    {
        //                        //Valid
        //                        // if(Request.Files[i].FileName.Any(Request.Files[i].FileName))
        //                        string strFileName = Guid.NewGuid().ToString() +
        //                                          Path.GetExtension(Request.Files[i].FileName);
        //                        Request.Files[i].SaveAs(Server.MapPath("/Upload/Task/" + strFileName));
        //                        fileName = strFileName;

        //                        var objAttachment = new AttachmentDisplay()
        //                        {
        //                            TaskID = iTaskID,
        //                            FileName = fileName,
        //                            Description = objDescription[i]
        //                        };

        //                        new AttachmentDisplay().Insert(objAttachment);

        //                        // Log Attachment .
        //                        LogTask.LogAddAttachment(iTaskID, new Attachment()
        //                        {
        //                            AttachmentID = objAttachment.ID,
        //                            Description = objAttachment.Description,
        //                            FileName = objAttachment.FileName,
        //                            TaskID = iTaskID
        //                        });
        //                    }
        //                }
         


        //            }//if

     
        //    }
        //    return objTask;
        //}

        public TaskAddEdit AssignTask(TaskAddEdit objTask)
        {
            int iTaskID = 0;
            if (objTask.EmployessLst == null)
            {
                 iTaskID = objTask.Save();
                // Saved .
                if (iTaskID > 0)
                {
                    // To Delete File .
                    if (objTask.RemovedFiles != null)
                    {
                        List<String> RemovedFiles = objTask.RemovedFiles.Split(',').ToList();

                        for (int i = 0; i < RemovedFiles.Count; i++)
                            if (!String.IsNullOrEmpty(RemovedFiles[i]))
                            {
                                if (System.IO.File.Exists(Server.MapPath("/Upload/Task/" + RemovedFiles[i])))
                                    System.IO.File.Delete(Server.MapPath("/Upload/Task/" + RemovedFiles[i]));

                                //new AttachmentDisplay().Delete(new AttachmentDisplay().GetAttachment(RemovedFiles[i]));
                                new AttachmentDisplay().Delete(new AttachmentDisplay().GetByID(int.Parse(RemovedFiles[i])));
                            }
                    }

                    // Uploaded File Description .
                    List<String> objDescription = (objTask.FilesDescription == null)
                                                      ? new List<string>()
                                                      : (objTask.FilesDescription.Split(',').ToList());

                    if (objDescription.Count > 1)
                    {
                        objDescription.RemoveAt(0);
                    }
                    // Insert Attachments .

                    //if (Extentions.ValidateReCaptcha())
                    //{
                    for (int i = 0; i < Request.Files.Count - 1; i++)
                        if (Request.Files[i].ContentLength > 0)
                        {
                            objTask.RecaptchCheck = false;

                            if (Extentions.CheckMimeType(Request.Files[i].ContentType))
                            {
                                Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
                                FileHeader = Extentions.ValidHeaders(FileHeader);
                                string fileName = "";
                                byte[] header;
                                string fileExt;
                                fileExt = Request.Files[i].FileName.Substring(Request.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
                                byte[] tmp = FileHeader[fileExt];
                                header = new byte[tmp.Length];
                                Request.Files[i].InputStream.Read(header, 0, header.Length);
                                if (Extentions.CompareArray(tmp, header))
                                {
                                    //Valid
                                    // if(Request.Files[i].FileName.Any(Request.Files[i].FileName))
                                    string strFileName = Guid.NewGuid().ToString() +
                                                      Path.GetExtension(Request.Files[i].FileName);
                                    Request.Files[i].SaveAs(Server.MapPath("/Upload/Task/" + strFileName));
                                    fileName = strFileName;

                                    var objAttachment = new AttachmentDisplay()
                                    {
                                        TaskID = iTaskID,
                                        FileName = fileName,
                                        Description = objDescription[i]
                                    };

                                    new AttachmentDisplay().Insert(objAttachment);

                                    // Log Attachment .
                                    LogTask.LogAddAttachment(iTaskID, new Attachment()
                                    {
                                        AttachmentID = objAttachment.ID,
                                        Description = objAttachment.Description,
                                        FileName = objAttachment.FileName,
                                        TaskID = iTaskID
                                    });
                                }
                            }



                        }//if


                }
            }
            else
            {
                List<string> uploadedFiles = new List<string>();
                for (int i = 0; i < Request.Files.Count - 1; i++)
                {
                    if (Request.Files[i].ContentLength > 0)
                    {
                        if (Extentions.CheckMimeType(Request.Files[i].ContentType))
                        {
                            Dictionary<string, byte[]> FileHeader = new Dictionary<string, byte[]>();
                            FileHeader = Extentions.ValidHeaders(FileHeader);
                            byte[] header;
                            string fileExt;
                            fileExt = Request.Files[i].FileName.Substring(Request.Files[i].FileName.LastIndexOf('.') + 1).ToUpper();
                            byte[] tmp = FileHeader[fileExt];
                            header = new byte[tmp.Length];
                            Request.Files[i].InputStream.Read(header, 0, header.Length);
                            if (Extentions.CompareArray(tmp, header))
                            {
                                string strFileName = Guid.NewGuid().ToString() +
                                                  Path.GetExtension(Request.Files[i].FileName);
                                Request.Files[i].SaveAs(Server.MapPath("/Upload/Task/" + strFileName));
                                uploadedFiles.Add(strFileName);
                            }
                        }
                    }
                }
                foreach (var emp in objTask.EmployessLst)
                {
                    objTask.EmpID = emp;
                     iTaskID = objTask.Save();
                    if (iTaskID > 0)
                    {
                        // To Delete File .
                        if (objTask.RemovedFiles != null)
                        {
                            List<String> RemovedFiles = objTask.RemovedFiles.Split(',').ToList();

                            for (int i = 0; i < RemovedFiles.Count; i++)
                                if (!String.IsNullOrEmpty(RemovedFiles[i]))
                                {
                                    if (System.IO.File.Exists(Server.MapPath("/Upload/Task/" + RemovedFiles[i])))
                                        System.IO.File.Delete(Server.MapPath("/Upload/Task/" + RemovedFiles[i]));

                                    //new AttachmentDisplay().Delete(new AttachmentDisplay().GetAttachment(RemovedFiles[i]));
                                    new AttachmentDisplay().Delete(new AttachmentDisplay().GetByID(int.Parse(RemovedFiles[i])));
                                }
                        }

                        // Uploaded File Description .
                        List<String> objDescription = (objTask.FilesDescription == null)
                                                          ? new List<string>()
                                                          : (objTask.FilesDescription.Split(',').ToList());

                        if (objDescription.Count > 1)
                        {
                            objDescription.RemoveAt(0);
                        }
                        // Insert Attachments .
                        for (int j = 0; j < uploadedFiles.Count; j++)
                        {
                            var objAttachment = new AttachmentDisplay()
                            {
                                TaskID = iTaskID,
                                FileName = uploadedFiles[j],
                                Description = objDescription[j]
                            };

                            new AttachmentDisplay().Insert(objAttachment);

                            // Log Attachment .
                            LogTask.LogAddAttachment(iTaskID, new Attachment()
                            {
                                AttachmentID = objAttachment.ID,
                                Description = objAttachment.Description,
                                FileName = objAttachment.FileName,
                                TaskID = iTaskID
                            });
                        }

                    }

                }
            }

            return objTask;
        }



    }
}