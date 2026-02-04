using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EtaskMinstry.App_Code;
using TaskManagementModel;

namespace EtaskMinstry.Models.WeekEnd
{
    public class WeekEndDisplayVM
    {
        #region"Properties"

        public int WeekEndID { get; set; }

        public int CompanyID { get; set; }

        public int WeekEndDay { get; set; }

        #endregion

        #region"Manage"

        private UnitOfWork _unitOfWork;

        public WeekEndDisplayVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// Get WeekEnds By Company .
        /// </summary>
        /// <param name="iCompanyID"> Company ID </param>
        /// <returns> List Of WeekEndDisplayVM </returns>
        public List<WeekEndDisplayVM> GetByCompany(int iCompanyID)
        {
            return _unitOfWork.WeekEndRepository.Get()
                              .Where(i => i.CompanyID== iCompanyID)
                              .Select(i => new WeekEndDisplayVM()
                                  {
                                      WeekEndID = i.WeekEndID,
                                      CompanyID = i.CompanyID,
                                      WeekEndDay = i.WeekEndDay
                                  })
                              .ToList();
        }
        #endregion
    }
}