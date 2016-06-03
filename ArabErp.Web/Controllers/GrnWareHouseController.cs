using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class GrnWareHouseController : Controller
    {
        // GET: GrnWareHouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingGrnWareHouse(int? page)
        {

            var rep = new SupplyOrderRepository();


            var slist = rep.GetSupplyOrdersPendingWorkshopRequest();

            var pager = new Pager(slist.Count(), page);

            var viewModel = new PagedSupplyOrderViewModel
            {
                SupplyOrders = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(SupplyOrder model)
        {
            GRN objGRN = new GRN();

            objGRN.Supplier = model.SupplierName;
            objGRN.SupplierId = model.SupplierId;
            objGRN.SONODATE = model.SoNoWithDate;
            objGRN.SupplyId = model.SupplyOrderId;
            objGRN.GRNDate = System.DateTime.Today;
            FillWarehouse();
            objGRN.Items = new List<GRNItem>();
            objGRN.Items.Add(new GRNItem());
            return View(objGRN);
        }


        public void FillWarehouse()
        {
            GRNRepository repo = new GRNRepository();
            var result = repo.GetWarehouseList();
            ViewBag.WarehouseList = new SelectList(result, "StockPointId", "StockPointName");
        }


        public ActionResult GRNData(SupplyOrder model)
        {

            var repo = new GRNRepository();
            var GRNList = repo.GetGRNData(model.SupplyOrderId);
            model.Items = new List<SupplyOrderItem>();
            foreach (var item in GRNList)
            {
                //model.Items.Add(new SupplyOrderItem { PurchaseRequestItemId = item.ItemId,ItemName = item.ItemName,PartNo = item.PartNo,PendingQuantity = item.Quantity,
                //ReceivedQuantity=item.Quantity,Unit = item.UnitName,Rate=item.Rate,Discount =item.Discount,Amount=item.Amount});

            }
            return PartialView("_DisplayGRNData", model);



        }
    }
}