using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EtaskMinstry.App_Code;
using TaskManagementModel;

namespace EtaskMinstry.Models.Holiday
{
    public class HolidayDisplayVM
    {
        #region"Properties"

        public int HolidayID { get; set; }

        public String HolidayName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int CompanyID { get; set; }

        public String Description { get; set; }

        #endregion

        #region"Manage"

        private UnitOfWork _unitOfWork;

        public HolidayDisplayVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// Get Holidays By Company .
        /// </summary>
        /// <param name="iCompanyID"> Company ID </param>
        /// <returns> List Of HolidayDisplayVM </returns>
        public List<HolidayDisplayVM> GetByCompany(int iCompanyID)
        {
            return _unitOfWork.HolidayRepository.Get()
                              .Where(i => i.CompanyID == iCompanyID)
                              .Select(i => new HolidayDisplayVM()
                                  {
                                      HolidayID = i.HoliDayID,
                                      HolidayName = i.HoliDayName,
                                      CompanyID = i.CompanyID,
                                      StartDate = i.StartDate,
                                      EndDate = i.EndDate,
                                      Description = i.Description
                                  })
                              .ToList();
        }

        #endregion
    }
}