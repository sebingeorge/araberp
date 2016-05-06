using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class WorkDescriptionController : Controller
    {
        // GET: WorkDescription
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateWorkDescription()
        {
            return View();
        }
    }
}