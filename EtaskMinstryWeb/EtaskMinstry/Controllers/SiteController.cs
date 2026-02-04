using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace EtaskMinstry.Controllers
{
    public class SiteController : Controller
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
           // Response.Redirect("http://t.etask.web.qvtest.com/");
            return View();
        }

        public ActionResult AboutETask()
        {
           // Response.Redirect("http://t.etask.web.qvtest.com/");
            return View();
        }

        public ActionResult AboutQVision()
        {
           // Response.Redirect("http://t.etask.web.qvtest.com/");
            return View();
        }

        public ActionResult Features()
        {
            //Response.Redirect("http://t.etask.web.qvtest.com/");
            return View();
        }

        public ActionResult ContactUs()
        {
            //Response.Redirect("http://t.etask.web.qvtest.com/");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(ContactUs obj)
        {
            if (ModelState.IsValid)
            {
                ContactUs contact = new ContactUs();
                contact.Save(obj);
                ModelState.Clear();
                ViewBag.SuccessMessage = "Success";
                return View();
            }
            else
            {
                return View();
            }
          
        }

        public ActionResult VerifyShowData()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyShowData(ContactUs obj)
        {
            if (obj.code == "12qvsd")
            {
                TempData["Verified"] = obj.code;
                return RedirectToAction("ShowContactUsData");
            }
            else
            {
                return View();
            }
        }

        public ActionResult ShowContactUsData()
        {
            if (TempData["Verified"] != null)
            {
                ContactUs contact = new ContactUs();
                var lst = contact.GetAll();
                return View(lst);
            }
            else
            {
                return RedirectToAction("VerifyShowData");
            }
        
        }
    }
}
