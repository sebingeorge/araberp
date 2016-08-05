using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class StoresLedgerController : BaseController
    {
        // GET: StoresLedger
        public ActionResult Index()
        {

            FillItem();
            FillWarehouse();
            FillItemCategory();
            return View();
        }
        public void FillWarehouse()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.StockpointDropdown();
            ViewBag.WarehouseList = new SelectList(result, "Id", "Name");
        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }
        public ActionResult StoresLedger(DateTime? from, DateTime? to, int stkid = 0, int itmcatid = 0, int itmid = 0)
        {
            to = to ?? DateTime.Today;
            return PartialView("_StoresLedger", new StoresLedgerRepository().GetStoresLedgerData(from, to, stkid, itmcatid, itmid, OrganizationId));
        }
    }
}