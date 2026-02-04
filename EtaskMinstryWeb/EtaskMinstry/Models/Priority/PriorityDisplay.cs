using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Models.Priority
{
    public class PriorityDisplay
    {
        #region "Properties"

        public int ID { get; set; }
        public String Name { get; set; }


        private UnitOfWork _unitOfWork;

        public PriorityDisplay()
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
        public List<PriorityDisplay> Get()
        {
            return _unitOfWork.PriorityRepository.Get()
                              .Select(i => new PriorityDisplay()
                                  {
                                      ID = i.PriorityID,
                                      Name = i.Name
                                  })
                              .ToList();
        }

        /// <summary>
        /// Get All Priorities .
        /// </summary>
        /// <returns></returns>
        public PriorityDisplay Get(int proprityId)
        {
            var priority = _unitOfWork.PriorityRepository.GetByID(proprityId);
            return new PriorityDisplay
            {
                ID = priority.PriorityID,
                Name = priority.Name
            };
        }
        #endregion
    }
}