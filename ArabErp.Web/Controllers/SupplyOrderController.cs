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
        public ActionResult Create(IList<PendingPurchaseRequest> model)
        {
            SupplyOrder supplyorder = new SupplyOrder();
            var item1 = new SupplyOrderItem();
            supplyorder.SupplyOrderItems.Add(item1);
            return View(supplyorder);
        }
        public ActionResult PendingSupplyOrder()
        {

            SupplyOrderRepository rep = new SupplyOrderRepository();

            IEnumerable<PendingPurchaseRequest> model = rep.GetPendingPurchaseRequest();

            return View(model);
        }
    }
}