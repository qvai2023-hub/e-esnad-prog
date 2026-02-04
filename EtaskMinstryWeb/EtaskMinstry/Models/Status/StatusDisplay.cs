using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;


namespace EtaskMinstry.Models.Status
{
    public class StatusDisplay
    {
        #region "Properties"

        public int ID { get; set; }
        public String Name { get; set; }
        public string DesignClass { get; set; }
        public int? Count { get; set; }

        private UnitOfWork _unitOfWork;

        public StatusDisplay()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        #endregion

        #region "Task Manage"

        /// <summary>
        /// Get All Priorities .
        /// </summary>
        /// <returns></returns>
        public List<StatusDisplay> Get()
        {
            List<StatusDisplay> StatusDisplay =_unitOfWork.StatusRepository.Get()
                              .Select(i => new StatusDisplay()
                              {
                                  ID = i.StatusID,
                                  Name = i.Name,
                                  DesignClass = i.DesignClass
                              })
                              .ToList();
            StatusDisplay.ForEach(s => s.Count = _unitOfWork.TaskRepository.Get(t => t.StatusID == s.ID).Count());
            return StatusDisplay;
        }

        public List<StatusDisplay> GetByEmployee(int empID)
        {
            List<StatusDisplay> StatusDisplay = _unitOfWork.StatusRepository.Get()
                              .Select(i => new StatusDisplay()
                              {
                                  ID = i.StatusID,
                                  Name = i.Name,
                                  DesignClass = i.DesignClass
                              })
                              .ToList();
            StatusDisplay.ForEach(s => s.Count =  _unitOfWork.TaskRepository.Get(t => t.StatusID == s.ID && t.EmpID == empID && !t.IsDeleted).Count());
            
         //   StatusDisplay.ForEach(s => s.Count = (s.ID == 10) ?( _unitOfWork.TaskRepository.Get(t => t.StatusID ==  (int)TaskStatus.Delay ? t.isDelayed : true).Count()) : _unitOfWork.TaskRepository.Get(t => t.StatusID == s.ID && t.EmpID == empID && !t.IsDeleted).Count());
            return StatusDisplay;
        }

        public StatusDisplay Get(int statusId)
        {
            var status = _unitOfWork.StatusRepository.GetByID(statusId);
            return new StatusDisplay 
            { 
             ID=status.StatusID,
             Name=status.Name,
             DesignClass = status.DesignClass
            };
        }

        public List<StatusDisplay> GetCustomStatus()
        {
            return Get().Where(s => s.ID != 9 && s.ID != 4).ToList();
        }
        #endregion
    }
}