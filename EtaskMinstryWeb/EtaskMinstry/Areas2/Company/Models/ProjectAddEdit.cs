using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using Resources;
using TaskManagementModel;
using System.Globalization;

namespace EtaskMinstry.Models.Project
{
    public class ProjectAddEdit
    {
        #region "Properties"

        public int ID { get; set; }

        [Display(Name = "الاسم")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Remote("checkDublicated", "Project", AdditionalFields = "ID",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages),
            ErrorMessageResourceName = "DublicatedItem")]
        public String Name { get; set; }

        [Display(Name = "الوصف")]
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        public String Description { get; set; }

        public int CompanyID { get; set; }

        #endregion

        #region "Task Manage"

        private UnitOfWork _unitOfWork;

        public ProjectAddEdit()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// Get Project by ID .
        /// </summary>
        /// <param name="iProjectID">Project ID</param>
        /// <returns></returns>
        public ProjectAddEdit Get(int iProjectID)
        {
            TaskManagementModel.Project objProject = _unitOfWork.ProjectRepository.GetByID(iProjectID);

            if (objProject == null)
                return new ProjectAddEdit();
            else
                return new ProjectAddEdit()
                    {
                        Name = objProject.Name,
                        CompanyID = MvcApplication.userData.userId,
                        Description = objProject.Description,
                        ID = objProject.ProjectID
                    };
        }

        public Boolean Save()
        {
           
            Boolean bReuslt = false;
            if (ID == 0)
            {
                TaskManagementModel.Project obj = new TaskManagementModel.Project()
                    {
                        CompanyID = MvcApplication.userData.userId,
                        Description = Description,
                        ProjectID = ID,
                        Name = Name
                    };
                _unitOfWork.ProjectRepository.Insert(obj);

                _unitOfWork.Save();

                bReuslt = obj.ProjectID > 0 ? true : false;
                //log.
                //AppCode.Generallog.Log(ID,obj,null,true,Name,string.Empty,CompanyID
                //log.               
                AppCode.Generallog.Log(ID, obj, null, true, Name,0, MvcApplication.userData.userId);     
            }
            else
            {
                var obj = _unitOfWork.ProjectRepository.GetByID(ID);
                var objbefore = obj.Clone();
                if (obj != null)
                {
                    obj.Description = Description;
                    obj.Name = Name;
                    _unitOfWork.ProjectRepository.Update(obj);

                    _unitOfWork.Save();
                }

                bReuslt = true;
                //log.
                AppCode.Generallog.Log(ID, obj, objbefore, true, Name, 0, MvcApplication.userData.userId);
            }
            return bReuslt;
        }

        /// <summary>
        /// Check Project Title .
        /// </summary>
        /// <param name="Project ID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool CheckDublication(int? ProjectID, string Name)
        {
           
            return
                _unitOfWork.ProjectRepository.Get(
                    i =>
                    i.Name.ToLower() == Name.ToLower().Trim() && i.ProjectID != ProjectID &&
                    i.CompanyID == MvcApplication.userData.userId).Any();
        }

        #endregion
    }
}