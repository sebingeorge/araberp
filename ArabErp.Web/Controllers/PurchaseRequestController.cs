using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class PurchaseRequestController : Controller
    {
        // GET: PurchaseRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreatePurchaseRequest()
        {
            return View();
        }
        public ActionResult PendingPurchaseRequest()
        {
            return View();
        }
    }
}