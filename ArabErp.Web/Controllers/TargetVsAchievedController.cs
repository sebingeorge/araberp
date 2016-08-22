using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;

namespace ArabErp.Web.Controllers
{
    public class TargetVsAchievedController : BaseController
    {
        // GET: TargetVsAchieved
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TargetVsAchievedRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_TargetVsAchievedRegister");
        }
    }
}