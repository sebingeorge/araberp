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
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: GrnWareHouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingGrnWareHouse()
        {
            var repo = new GRNRepository();

            IEnumerable<PendingSupplyOrder> pendingSO = repo.GetGRNPendingList();
            return View(pendingSO);
        }


        public ActionResult PreviousList()
        {
            var repo = new GRNRepository();
            IEnumerable<GRN> GRNList = repo.GetGRNPreviousList();
            return View("PreviousList", GRNList);
        }


        public ActionResult Create(int? SupplyOrderId)
        {
            GRNRepository repo = new GRNRepository();
            FillWarehouse();
            FillCurrency();

            GRN model = repo.GetGRNDetails(SupplyOrderId ?? 0);
            var GRNList = repo.GetGRNItem(SupplyOrderId ?? 0);

            model.Items = new List<GRNItem>();
            foreach (var item in GRNList)
            {
                var grnitem = new GRNItem
                {
                    SupplyOrderItemId=item.SupplyOrderItemId,
                    ItemName = item.ItemName,
                    ItemId = item.ItemId,
                    PartNo = item.PartNo,
                    Remarks = item.Remarks,
                    PendingQuantity=item.PendingQuantity,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    Rate = item.Rate,
                    Discount = item.Discount,
                    Amount = item.Amount
                };
                model.Items.Add(grnitem);

            }
            return View(model);
        }

      
        public void FillWarehouse()
        {
            GRNRepository repo = new GRNRepository();
            var result = repo.GetWarehouseList();
            ViewBag.WarehouseList = new SelectList(result, "StockPointId", "StockPointName");
        }

        public void FillCurrency()
        {
            var repo = new GRNRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }

        public ActionResult Save(GRN model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new GRNRepository().InsertGRN(model);
            new GRNRepository().InsertStockUpdate(model);
            return RedirectToAction("PendingGrnWareHouse");
        }

        public ActionResult Modify(int? GRNId)
        {
            GRNRepository repo = new GRNRepository();
            FillWarehouse();
            FillCurrency();

            GRN model = repo.GetGRNDISPLAYDetails(GRNId ?? 0);
            model.Items = repo.GetGRNDISPLAYItem(GRNId ?? 0);

            return View("Create",model);
        }

        public ActionResult PendingDirectPurchaseRequests()
        {
            return View();
        }
    }
}