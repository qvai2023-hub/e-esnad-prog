using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using EtaskMinstry;
using TaskManagementModel;

namespace EtaskMinstry.Models.Project
{
    [Serializable]
    public class ProjectDisplay
    {
        #region "Properties"

        public int ID { get; set; }

        public String Name { get; set; }

        public String FullDescription { get; set; }

        public String ShortDescription { get; set; }

        public int CompanyID { get; set; }

        public Boolean bCanDelete { get; set; }

        #endregion

        #region "Task Manage"

        private UnitOfWork _unitOfWork;

        public ProjectDisplay()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// Get All Projects .
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public List<ProjectDisplay> Get(String strName = "")
        {
            strName = strName.Trim().ToLower();
            List<ProjectDisplay> lst = new List<ProjectDisplay>();
            if (MvcApplication.userData.userId !=0)
            {
                if (MvcApplication.userData.CompanyId == 0)
                { lst = GetProjectsByCompany(MvcApplication.userData.userId).Where
                        (i => i.Name.ToLower().Contains(strName)).ToList();}
                else
                {
                    lst = GetProjectsByCompany(MvcApplication.userData.CompanyId.Value).Where
                        (i => i.Name.ToLower().Contains(strName)).ToList();
                }

                
                return lst;
            }
            else
                return null;
        }

        public ProjectDisplay GetById(int projectid)
        {
            var project = _unitOfWork.ProjectRepository.GetByID(projectid);
            return new ProjectDisplay
            {
                ID = project.ProjectID,
                Name = project.Name
            };
        }

        /// <summary>
        /// get all projects which the passed employee has tasks in
        /// </summary>
        /// <param name="empid"></param>
        /// <returns></returns>
        public List<ProjectDisplay> GetEmployeeProject(int empid)
        {
            return
                _unitOfWork.ProjectRepository.Get()
                           .Where(p => p.Tasks.Any(t => t.EmpID == empid))
                           .Select(p => new ProjectDisplay
                               {
                                   ID = p.ProjectID,
                                   Name = p.Name
                               }).ToList();
        }

        /// <summary>
        /// get projects of specific Company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="strName"></param>
        /// <returns></returns>
        public List<ProjectDisplay> GetProjectsByCompany(int companyId)
        {
            var objProject = _unitOfWork.ProjectRepository.Get(i =>
                                                               i.CompanyID== companyId
                                                               && 
                                                               !i.IsDeleted
                ).Select(i => new ProjectDisplay()
                    {
                        ID = i.ProjectID,
                        Name = i.Name,
                        FullDescription = i.Description,
                        ShortDescription = i.Description,
                        bCanDelete = (!i.Tasks.Any() || i.Tasks.All(t => t.IsDeleted))
                    }).OrderByDescending(i => i.ID).ToList();

            objProject.ForEach(i => i.ShortDescription = Extentions.SubString(i.ShortDescription, 30));
            return objProject;
        }

        public ProjectDisplay Get(int iID)
        {
            return Get().FirstOrDefault(i => i.ID == iID);
        }

        /// <summary>
        /// Delete Project If It Has Not Tasks Or If It Has All Tasks IsDeleted Set Project To IsDeleted .
        /// </summary>
        /// <param name="iID">Project ID</param>
        /// <returns></returns>
        public Boolean Delete(int iID)
        {
            var objProject = _unitOfWork.ProjectRepository.GetByID(iID);

            if (objProject != null)
            {
                if (!objProject.Tasks.Any())
                    _unitOfWork.ProjectRepository.Delete(iID);

                else if (objProject.Tasks.All(i => i.IsDeleted))
                    objProject.IsDeleted = true;

                _unitOfWork.Save();
                //log.
                AppCode.Generallog.Delete(objProject.Name, MvcApplication.userData.userId);
   
                return true;
            }
          
            return false;
        }

        #endregion

    }
}