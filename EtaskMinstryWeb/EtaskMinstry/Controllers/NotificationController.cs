using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.Models;
using EtaskMinstry;
using EtaskMinstry.Controllers;

namespace EtaskMinstry.Controllers
{
    public class NotificationController : BaseController
    {
        //
        // GET: /Notification/

        public ActionResult Index()
        {
            SecurityController security = new SecurityController();
            var allNotification = new PushNotificationVM().GetAllNotSeen();
            foreach (var item in allNotification)
            {
                security.UpdateNotification(item.ID);
            }
          
            return View("Notification", new PushNotificationVM(0, 10).Data);
        }

        public JsonResult GetNotification(int iSkip)
        {
            return Json(new PushNotificationVM(iSkip, 10).Data.Select(i => new
                {
                    ID = i.ID,
                    Value = i.Value,
                    IsSeen = i.IsSeen,
                    SendDate = MvcApplication.IsGregDate ? i.SendDate.ToGregArabicDateTime() : i.SendDate.ToHijriArabicDateTime(),
                    Link = string.IsNullOrEmpty(i.Link) ? "" : i.Link.Substring(0, i.Link.LastIndexOf('/')) + "/" + Extentions.Encrypt(i.Link.Split('/').Last()),
                    Type = i.Type
                }), JsonRequestBehavior.AllowGet);
        }
    }
}