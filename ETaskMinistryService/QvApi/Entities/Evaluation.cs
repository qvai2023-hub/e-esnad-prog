using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

using System.ServiceModel;
namespace QvApi.Entities
{
    
   

    [MessageContract(IsWrapped = false, WrapperNamespace = "http://tempuri.org/")]

    public class EvaluationResponseMessage
    {

        [MessageBodyMember(Name = "Evaluations", Namespace = "http://tempuri.org/")]
       
       public List<Evaluation> Evaluations { set; get; }
       
    }

    [MessageContract(IsWrapped = true, WrapperName = "GetTeleworkersEvaluations", WrapperNamespace = "http://tempuri.org/")]
   
    public class EvaluationRequestMessage
    {
        [MessageBodyMember(Name = "PeriodStart", Order = 1, Namespace = "http://tempuri.org/"), DataMember(Order = 1)]
        
        public string PeriodStart { get; set; }

        [MessageBodyMember(Name = "PeriodEnd", Order = 2, Namespace = "http://tempuri.org/"), DataMember(Order = 2)]
        public string PeriodEnd { get; set; }

        /*
        [MessageHeader(Namespace = "http://tempuri.org/",Name="AuthHeader",MustUnderstand=false)]
            public string AuthHeader { get; set; }
       */

    }
    

     [MessageContract(IsWrapped = false, WrapperNamespace = ""), DataContract(Namespace = "http://tempuri.org/")]
    public class Evaluation
    {

        [MessageBodyMember(Order = 1, Namespace = "http://tempuri.org/"), DataMember(Order = 1)]
        public int EstLaborOfficeId { set; get; }
         [MessageBodyMember(Order = 2, Namespace = "http://tempuri.org/"), DataMember(Order = 2)]
        public int EstSequenceNumber { set; get; }

        [MessageBodyMember(Order = 3, Namespace = "http://tempuri.org/"), DataMember(Order = 3)]
        public long IdNumber { set; get; }

          [MessageBodyMember(Order = 4, Namespace = "http://tempuri.org/"), DataMember(Order = 4)]
        public Decimal ActivityLevel { set; get; }

          [MessageBodyMember(Order = 5, Namespace = "http://tempuri.org/"), DataMember(Order = 5)]
        public int LoginCount { set; get; }

          [MessageBodyMember(Order = 6, Namespace = "http://tempuri.org/"), DataMember(Order = 6)]
        public int LogoutCount { set; get; }

          [MessageBodyMember(Order = 7, Namespace = "http://tempuri.org/"), DataMember(Order = 7)]
        public int AssignedTasks { set; get; }

        [MessageBodyMember(Order = 8, Namespace = "http://tempuri.org/"), DataMember(Order = 8)]
        public int CompletedTasks { set; get; }

          [MessageBodyMember(Order = 9, Namespace = "http://tempuri.org/"), DataMember(Order = 9)]
        public Decimal TotalWorkTime { set; get; }
        
    }
}