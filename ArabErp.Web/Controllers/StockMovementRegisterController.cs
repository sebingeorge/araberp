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
    public class StockMovementRegisterController : BaseController
    {
        // GET: StockMovementRegister
        public ActionResult Index()
        {
            InitDropdown();
            FillWarehouse();
            FillItemCategory();
            FillPartNo();
            //FillGroup();
            //FillSubGroup();
            ClosingStock cs = new ClosingStock();
            cs.itmCatId = 0;
            cs.itmGrpId = 0;
            cs.itmSubGrpId = 0;
            cs.ItemId = 0;
            ViewBag.startdate = FYStartdate;
            return View("Index", cs);
        }
        //public void InitDropdown()
        //{
        //    var List = "";
        //    ViewBag.ItemList = new SelectList(List, "Id", "Name");

        //}
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

        //public void FillItemCategory()
        //{
        //    DropdownRepository repo = new DropdownRepository();
        //    var result = repo.ItemCategoryDropdown();
        //    ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        //}
        public void FillGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemGroupDropdown();
            ViewBag.ItemGroup = new SelectList(result, "Id", "Name");
        }
        public void FillSubGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemSubgroupDropdown();
            ViewBag.ItemSubgroup = new SelectList(result, "Id", "Name");
        }
        public ActionResult StockMovementRegister(DateTime? from, DateTime? to, int stkid = 0, int itmcatid = 0, int itmid = 0, int ItemGroupId = 0, int ItemSubGroupId = 0, string PartNo="")
      {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_StockMovementRegister", new StockMovementRegisterRepository().GetStockMovementData(from: from, to: to, stkid:stkid, itmcatid: itmcatid, itmid: itmid, OrganizationId: OrganizationId, ItemGroupId: ItemGroupId, ItemSubGroupId: ItemSubGroupId,PartNo:PartNo));
        }
        //public ActionResult Item(int Code)
        //{
        //    FillItem(Code);
        //    return PartialView("_ItemDropDown");
        //}
        public void FillItemGroup(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemGroup(Id);
            ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
        }
        public void FillItemSubGroup(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemSubGroup(Id);
            ViewBag.ItemSubGroupList = new SelectList(List, "Id", "Name");
        }
        public void FillItemCategory()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemCategory();
            ViewBag.ItemCategoryList = new SelectList(List, "Id", "Name");
        }

        public void FillMaterial(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillMaterial(Id);
            ViewBag.ItemList = new SelectList(List, "Id", "Name");
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
            ViewBag.ItemSubGroupList = new SelectList(List, "Id", "Name");
        }
        public ActionResult ItemGroup(int Code)
        {
            FillItemGroup(Code);
            return PartialView("_ItemGroupDropdown");
        }
        public ActionResult ItemSubGroup(int Code)
        {
            FillItemSubGroup(Code);
            return PartialView("_ItemSubGroupDropdown");
        }
        public ActionResult ItemCategory()
        {
            FillItemCategory();
            return PartialView("_ItemCategoryDropdown");
        }
        public ActionResult Material(int Code)
        {
            FillMaterial(Code);
            return PartialView("_ItemDropdown");
        }
        public JsonResult GetPartNo(int itemId)
        {
            return Json(new ItemRepository().GetPartNo(itemId), JsonRequestBehavior.AllowGet);
        }
        public void FillPartNo()
        {
            ViewBag.partNoList = new SelectList(new DropdownRepository().PartNoDropdown1(), "Id", "Name");
        }
    }
}