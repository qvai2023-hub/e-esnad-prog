using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace QvApi.Helpers
{
    //This custom username validator is to validate the username and password
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
 

        public override void Validate(string userName, string password)
        {

            if (!(userName == ConfigurationManager.AppSettings["ServiceUsername"] && password == ConfigurationManager.AppSettings["ServicePassword"]))
            {
                throw new FaultException("Invalid username or password");
            }
        }
    }
}