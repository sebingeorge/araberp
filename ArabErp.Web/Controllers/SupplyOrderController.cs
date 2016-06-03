using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;

namespace ArabErp.Web.Controllers
{
    public class SupplyOrderController : Controller
    {
        // GET: SupplyOrder
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult PendingSupplyOrder()
        {

            SupplyOrderRepository rep = new SupplyOrderRepository();

            IEnumerable<PendingPurchaseRequest> model = rep.GetPendingPurchaseRequest();

            return View(model);
        }
    }
}