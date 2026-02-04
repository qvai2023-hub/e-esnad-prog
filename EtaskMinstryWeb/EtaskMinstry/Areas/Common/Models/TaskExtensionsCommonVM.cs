using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EtaskMinstry.AppCode;

using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Models.Common
{
    public class TaskExtensionsCommonVM
    {

        #region F E I L D  S  &  P R O P E R T I E S

        public int TaskID { get; set; }
        public int StatusID { get; set; }
        public bool IsArchived { get; set; }
        public int EmployeeID { get; set; }
        public int? Createdby { get; set; }

        public string CreatedbyName { get; set; }
        public string EmployeeName { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired", ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string CommentText { get; set; }

        public List<TaskComment> Comments { get; set; }

        public List<TaskManagementModel.Attachment> TaskAttachment { get; set; }
        #endregion

        private UnitOfWork _unitOfWork;

        public TaskExtensionsCommonVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public TaskExtensionsCommonVM Select(object iID)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);
            if (objTask == null)
                return null;
            TaskExtensionsCommonVM objDetails = new TaskExtensionsCommonVM();
            objDetails.TaskID = objTask.TaskID;
            objDetails.Createdby = objTask.Createdby;
            objDetails.CreatedbyName = objTask.Createdby == null ? " " : ServiceManger.GetEmplyeeName(objTask.Createdby.Value);
            objDetails.StatusID = objTask.StatusID;
            objDetails.IsArchived = objTask.IsArchived;
            objDetails.EmployeeID = objTask.EmpID.HasValue ? objTask.EmpID.Value : 0;
            objDetails.EmployeeName = ServiceManger.GetEmplyeeName(EmployeeID);
            objDetails.Comments = objTask.TaskComments.OrderBy(c => c.CreatedDate).ToList();
            objDetails.TaskAttachment = objTask.Attachments.OrderBy(a => a.AttachmentID).ToList();
            return objDetails;
        }
    }



}