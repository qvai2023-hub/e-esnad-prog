using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;

namespace ETaskServiceClient.AppCode
{
    public class ConsoleOutputBehaviorExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new CustomOutputBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(CustomOutputBehavior);
            }
        }
    }
}