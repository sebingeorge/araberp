using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class WorkShopRequestController : Controller
    {
        // GET: WorkShopRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateWorkShopRequest()
        {
            return View();
        }
        public ActionResult WorkShopRequestPending()
        {
            return View();
        }
    }
}