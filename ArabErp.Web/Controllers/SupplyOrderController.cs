using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SupplyOrderController : Controller
    {
        // GET: SupplyOrder
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateSupplyOrder()
        {
            return View();
        }
        public ActionResult PendingSupplyOrder()
        {
            return View();
        }
    }
}