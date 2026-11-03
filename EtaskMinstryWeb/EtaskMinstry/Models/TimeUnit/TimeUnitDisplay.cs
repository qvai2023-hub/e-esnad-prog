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
        /// Get the Time Units a task estimate may use: Day / Hour / Minute (the agreed values 3 / 4 / 5).
        /// </summary>
        /// <returns></returns>
        public List<TimeUnitDisplay> Get()
        {
            int[] allowedUnits = EtaskMinstry.AppCode.TaskTimeEstimate.AllowedUnits;
            return _unitOfWork.TimUnitRepository.Get(filter: i => allowedUnits.Contains(i.TimeUnitID))
                              .Select(i => new TimeUnitDisplay()
                                  {
                                      ID = i.TimeUnitID,
                                      Name = i.Name
                                  }).ToList();
        }

        #endregion

    }
}