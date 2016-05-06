using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobOrderCompletionController : Controller
    {
        // GET: JobOrderCompletion
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateJobOrderCompletion()
        {
            return View();
        }
        public ActionResult PendingJobOrderCompletion()
        {
            return View();
        }
    }
}