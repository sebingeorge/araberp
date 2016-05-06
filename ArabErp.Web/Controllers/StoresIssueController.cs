using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StoresIssueController : Controller
    {
        // GET: StoresIssue
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateStoresIssue()
        {
            return View();
        }
        public ActionResult PendingStoresIssue()
        {
            return View();
        }
    }
}