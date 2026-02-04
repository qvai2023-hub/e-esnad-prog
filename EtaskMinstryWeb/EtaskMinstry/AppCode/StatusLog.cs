using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
    public class StatusLog
    {
        /// <summary>
        /// Log status.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="empId"></param>
        /// <param name="statuseId"></param>
        public static void LogStatuse(int taskId, int statusId=0, int empId=0, int timeCount=0)
        {
            UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            TaskTLog tasklog = _unitOfWork.TaskStatuseLog.Get(l=>l.TaskID==taskId).OrderByDescending(l => l.TaskTLogID).FirstOrDefault();
            TaskTLog statuselog = new TaskTLog
            {
                TaskID = taskId,
                EmpID = empId,
                StatusID = statusId == 0 ? tasklog.StatusID : statusId,
                CreatedDate = DateTime.Now,
                TimeCount = timeCount
            };
            _unitOfWork.TaskStatuseLog.Insert(statuselog);
            _unitOfWork.Save();
            _unitOfWork.Dispose();
        }
    }
}