using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Models.TimeUnit
{
    public class TimeUnitDisplay
    {
        #region "Properties"

        public int ID { get; set; }
        public String Name { get; set; }

        private UnitOfWork _unitOfWork;

        public TimeUnitDisplay()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        #endregion

        #region "Task Manage"

        /// <summary>
        /// Get All Time Units .
        /// </summary>
        /// <returns></returns>
        public List<TimeUnitDisplay> Get()
        {
            return _unitOfWork.TimUnitRepository.Get()
                              .Select(i => new TimeUnitDisplay()
                                  {
                                      ID = i.TimeUnitID,
                                      Name = i.Name
                                  }).ToList();
        }

        #endregion

    }
}