using ArabErp.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobCardController : Controller
    {
        JobCardRepository repo;
        public JobCardController()
        {
            repo = new JobCardRepository();
        }
        // GET: JobCard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult PendingJobCard()
        {
            IEnumerable<PendingSO> pendingSo = repo.GetPendingSO();
            return View(pendingSo);            
        }

        public ActionResult Save(JobCard jc)
        {



            var data = new JobCardRepository().SaveJobCard(jc);
            return Json(data);
           // return View();
        }
    }
}