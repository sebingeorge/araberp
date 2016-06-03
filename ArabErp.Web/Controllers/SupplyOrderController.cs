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
        public ActionResult Create(IList<PendingPurchaseRequest> PendingPurchaseRequestItemsSelected)
        {
            SupplyOrder supplyorder = new SupplyOrder();
  


            SupplyOrderRepository rep = new SupplyOrderRepository();

            List<int> selectedpurchaserequests = (from PendingPurchaseRequest p in PendingPurchaseRequestItemsSelected where p.Select select p.PurchaseRequestId).ToList<int>();

            supplyorder.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests);

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