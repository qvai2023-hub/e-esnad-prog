using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;
using EtaskMinstry;

namespace EtaskMinstry.Models
{
    public class PushNotificationVM
    {
        private List<NotificationPattren> _Data;
         private List<NotificationPattren> _AllData;
         public PushNotificationVM()
         {
            
         }

        public PushNotificationVM(int Skip, int Take) 
        {
            this._Data = NotificationHub.Get(Skip, Take);
        }

    
        public List<NotificationPattren> Data
        {
            get
            {

                return this._Data;
            }
        }

      
        public int NewCount
        {
            get
            {
                return this._Data.Where(i => !i.IsSeen).Count();
            }
        }


        public List<NotificationPattren> GetAllNotSeen()
        {
            return _AllData = NotificationHub.GetAll().Where(i => !i.IsSeen).ToList();
        }
    }
}