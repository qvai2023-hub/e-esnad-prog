using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace QvApi.Helpers
{
    public class Validation
    {
        public static bool ValidateStartEndDates(DateTime PeriodStart, DateTime PeriodEnd)
        {
            if (PeriodStart.Date >= DateTime.Now.Date)
            {
                throw new FaultException("Period start must be before today's date.");
            }

            if (PeriodEnd.Date > DateTime.Now.Date)
            {
                throw new FaultException("Period end must be before or equals to today's date.");             
            }


            if( PeriodEnd.Date <= PeriodStart.Date)
            {
                throw new FaultException("Period end must be after period start");  
            }
          


            /*
            var Reference = ConfigurationManager.AppSettings["ClientReference"].ToString();
            if (ClientReference != Reference)
            {
                throw new FaultException("Invalid Client Reference");   
            }
             */
            return true;
        }
    }


    


}