using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;

namespace EtaskMinstry.Services
{
    public class SharedService
    {
        private UnitOfWork _unitOfWork;

        public SharedService()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }


        public SelectList GetProviders()
        {
            return new SelectList(new List<SelectListItem>{
        new SelectListItem { Text = "اسناد", Value = Provider.Esnad.ToString() },
        new SelectListItem { Text = "تلى ساك", Value = Provider.Telesak.ToString()}}, "Value", "Text");
        }
            public List<SelectListItem> GetCompaniesByproviderId(string provider)
        {
            bool isTelesak = false;
            if(provider== Provider.Telesak.ToString())
            {
                isTelesak = true;
            }

            //      var selectList = _unitOfWork.UserAccount
            //.Get(x => x.IsTelesak == isTelesak && x.CompanyID != null)
            //.GroupBy(n => new { n.CompanyID, CompanyName = n.Company.Name })
            //.AsEnumerable() 
            //.Select(g => new SelectListItem
            //{
            //    Value = g.Key.CompanyID?.ToString(),
            //    Text = g.Key.CompanyName
            //})
            //.ToList();
            var selectList = _unitOfWork.UserAccount
.Get(x => x.IsTelesak == isTelesak && x.CompanyID != null)
.GroupBy(n => new { n.CompanyID, CompanyName = n.Company.Name })
.AsEnumerable()
.Select(g => new SelectListItem
{
    Value = g.Key.CompanyID?.ToString(),
    Text = g.Key.CompanyName
})
.ToList();
            return selectList;
        }
    }
}