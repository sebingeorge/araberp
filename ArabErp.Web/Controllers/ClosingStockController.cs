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
    public class ClosingStockController : BaseController
    {
        // GET: ClosingStock
        public ActionResult Index()
        {
            InitDropdown();
            FillWarehouse();
            FillItemCategory();
            ClosingStock cs = new ClosingStock();
            cs.itmCatId = 0;
            return View("Index", cs);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemList = new SelectList(List, "Id", "Name");

        }
        public ActionResult ClosingStockRegister(DateTime? Ason, int stkid = 0, int itmcatid = 0, int itmid = 0)
        {
            Ason = Ason ?? DateTime.Today;
            return PartialView("_ClosingStockRegister", new ClosingStockRepository().GetClosingStockData(Ason, stkid, itmcatid, itmid, OrganizationId));
        }
        public void FillWarehouse()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.StockpointDropdown();
            ViewBag.WarehouseList = new SelectList(result, "Id", "Name");
        }
        public void FillItem(int Id)
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCatDropdown(Id);
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        
 public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }
 public ActionResult Item(int Code)
 {
     FillItem(Code);
     return PartialView("_ItemDropDown");
 }
    }
}