using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class LocalPurchaseController : Controller
    {
        // GET: LocalPurchase
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Request()
        {
            List<LocalPurchaseRequestItem> list = new List<LocalPurchaseRequestItem>();
            list.Add(new LocalPurchaseRequestItem());
            return View(new LocalPurchaseRequest { items = list });
        }
    }
}