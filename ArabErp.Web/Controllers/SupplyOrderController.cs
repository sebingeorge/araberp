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
            if (PendingPurchaseRequestItemsSelected != null)
            {
                if (PendingPurchaseRequestItemsSelected.Count > 0)
                {
                    List<int> selectedpurchaserequests = (from PendingPurchaseRequest p in PendingPurchaseRequestItemsSelected
                                                          where p.Select
                                                          select p.PurchaseRequestId).ToList<int>();
                    supplyorder.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests);
                }


            }


            supplyorder.SupplyOrderDate = System.DateTime.Today;
            supplyorder.RequiredDate = System.DateTime.Today;



            FillSupplier();

            return View(supplyorder);
        }
        public ActionResult PendingSupplyOrder()
        {

            SupplyOrderRepository rep = new SupplyOrderRepository();

            IEnumerable<PendingPurchaseRequest> model = rep.GetPendingPurchaseRequest();

            return View(model);
        }

        public void FillSupplier()
        {
            var repo = new SupplierRepository();
            List<Dropdown> list = repo.FillSupplier();
            ViewBag.SupplierList = new SelectList(list, "Id", "Name");
        }

        [HttpPost]
        public ActionResult Save(SupplyOrder model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new SupplyOrderRepository().InsertSupplyOrder(model);
            return RedirectToAction("PendingSupplyOrder");
        }

        public ActionResult LocalSupplyOrder()
        {
            FillSupplier();
            GetMaterialDropdown();
            List<SupplyOrderItem> list = new List<SupplyOrderItem>();
            list.Add(new SupplyOrderItem());
            return View("CreateLocalSupplyOrder", new SupplyOrder { SupplyOrderItems = list, SupplyOrderDate = DateTime.Now, RequiredDate = DateTime.Now });
        }
        [HttpPost]
        public ActionResult LocalSupplyOrder(SupplyOrder model)
        {
            return View("LocalSupplyOrder");
        }

        private void GetMaterialDropdown()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
    }
}