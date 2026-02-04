using System.Diagnostics;
using QvApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using QvApi.Helpers;
using QvApi.Data;
using System.ServiceModel.Channels;
namespace QvApi
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Etask" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Etask.svc or Etask.svc.cs at the Solution Explorer and start debugging.
    public class Etask : IEtask
    {
        private const string EventLogSource = "QvApi.Services.Etask";
        private const string EventLogLog = "Application";

        public Etask()
        {
            try
            {
                //Check eventlog setup
                //if (!EventLog.SourceExists(EventLogSource))
                   // EventLog.CreateEventSource(EventLogSource, EventLogLog);
            }
            catch (Exception ex)
            {
               // EventLog.WriteEntry(EventLogSource, ex.ToString(), EventLogEntryType.Error);
            }
        }

        public EvaluationResponseMessage GetTeleworkersEvaluation(EvaluationRequestMessage requestMessage)
        {
            try
            {

               // var authHeader=  OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("AuthHeader", "http://tempuri.org/");
                
                
                //The following line was hashed because they don't want to use public key for now
                //string public_key = requestMessage.AuthHeader;
               // if (public_key == System.Configuration.ConfigurationManager.AppSettings["ClientReference"])

                string public_key = "";
                if(public_key=="") 
                {
                     DateTime PeriodStart = DateTime.ParseExact(requestMessage.PeriodStart, "dd/MM/yyyy", new System.Globalization.CultureInfo("fr-FR"));
                     DateTime PeriodEnd = DateTime.ParseExact(requestMessage.PeriodEnd, "dd/MM/yyyy", new System.Globalization.CultureInfo("fr-FR"));

                     if (Validation.ValidateStartEndDates(PeriodStart, PeriodEnd))
                     {

                         EvaluationResponseMessage result = EvaluationData.GetEvaluation(PeriodStart, PeriodEnd);
                         return result;
                     }
                     return null;
                 }
                 else
                 {
                     throw new FaultException("Unauthorized Access");
                 }
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                //EventLog.WriteEntry(EventLogSource,ex.ToString(),EventLogEntryType.Error);
                throw new FaultException("Internal Error, Please contact Q-vision support."+ex.Message); 
            }
        }
        
    }
}
