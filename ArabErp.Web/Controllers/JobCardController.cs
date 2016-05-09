using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobCardController : Controller
    {
        // GET: JobCard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateJobCard()
        {
            return View();
        }
        public ActionResult PendingJobCard()
        {
            return View();
        }

        public ActionResult Save(JobCard jc)
        {



            //var data = new JobCardRepository().SaveJobCard(jc);
            return View();
        }
    }
}