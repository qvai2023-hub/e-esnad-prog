using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EtaskMinstry.AppCode;

using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Models.Company
{
    public class TaskExtensionsVM
    {

        #region F E I L D  S  &  P R O P E R T I E S

        public int TaskID { get; set; }
        public int StatusID { get; set; }
        public bool IsArchived { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired", ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string CommentText { get; set; }

        public List<TaskComment> Comments { get; set; }

        public List<TaskManagementModel.Attachment> TaskAttachment { get; set; }
        #endregion

        private UnitOfWork _unitOfWork;

        public TaskExtensionsVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public TaskExtensionsVM Select(object iID)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);
            if (objTask == null)
                return null;
            TaskExtensionsVM objDetails = new TaskExtensionsVM();
            objDetails.TaskID = objTask.TaskID;
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