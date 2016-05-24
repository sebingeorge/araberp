using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ItemController : Controller
    {
        // GET: Item
        public ActionResult Create()
        {
            FillItemCategory();
            FillUnit();
            InitDropdown();
            Item oItem=new Item();
            oItem.PartNo = null;
            oItem.ItemName = null;
            oItem.ItemPrintName = null;
            oItem.ItemShortName = null;
            oItem.CommodityId = null;
            oItem.ItemCategoryId = null;
            oItem.ItemGroupId = null;
            oItem.ItemSubGroupId = null;
            oItem.ItemUnitId = null;
            oItem.ExpiryDate = null;
            oItem.MinLevel = null;
            oItem.ReorderLevel = null;
            oItem.MaxLevel = null;
            oItem.StockRequired=true;
            oItem.BatchRequired=false;
           
            return View("Create", oItem);
        }

        public ActionResult Save(Item oitem)
        {
            FillItemCategory();
            FillUnit();
            InitDropdown();

            new ItemRepository().InsertItem(oitem);
            return View("Create");
        }

        public ActionResult View(int Id)
        {

            Item objItem =new JobCardRepository().GetItem(Id);
            return View("Create", objItem);
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
        public void FillItemGroup(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemGroup(Id);
            ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
        }
        public void FillUnit()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillUnit();
            ViewBag.Unit = new SelectList(List, "Id", "Name");
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
        
    }
}