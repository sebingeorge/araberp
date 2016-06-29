﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ItemController : BaseController
    {
        public ActionResult Index()
        {
            return View();
         }

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
            oItem.ItemCategoryId = 0;
            oItem.ItemGroupId = 0;
            oItem.ItemSubGroupId = 0;
            oItem.ItemUnitId = null;
            oItem.ExpiryDate = DateTime.Now;
            oItem.MinLevel = null;
            oItem.ReorderLevel = null;
            oItem.MaxLevel = null;
            oItem.StockRequired = false;
            oItem.BatchRequired=false;
            oItem.ItemRefNo = "ITM/"+DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Item).Name);
           
            return View("Create", oItem);
        }
        [HttpPost]
        public ActionResult Create(Item oitem)
        {
            FillItemCategory();
            FillUnit();
            InitDropdown();
            oitem.OrganizationId = 1;
            oitem.CreatedDate = System.DateTime.Now;
            oitem.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var result = new ItemRepository().InsertItem(oitem);


            if (result.ItemId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["ItemRefNo"] = result.ItemRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["ItemRefNo"] = null;
                return View("Create", oitem);
            }
          
        }

        public ActionResult View(int Id)
        {

            Item objItem =new JobCardRepository().GetItem(Id);
            return View("Create", objItem);
        }

        public ActionResult Edit(int Id)
        {
            
            Item objItem = new ItemRepository().GetItem(Id);
          
            FillUnit();
          
            
             return View(objItem);
                
            
        }
       [HttpPost]
        public ActionResult Edit(Item model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var result = new ItemRepository().UpdateItem(model);


            if (result.ItemId > 0)
            {
                TempData["Success"] = "Updated Successfully!";
                TempData["ItemRefNo"] = result.ItemRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["ItemRefNo"] = null;
                SaleOrder saleOrder = new SaleOrder();
                saleOrder.SaleOrderDate = System.DateTime.Today;
                saleOrder.Items = new List<SaleOrderItem>();
                saleOrder.Items.Add(new SaleOrderItem());
                return View("Edit", model);
            }
           
        }


       public ActionResult Delete(int Id)
       {

            Item objItem = new ItemRepository().GetItem(Id);

           FillUnit();


           return View(objItem);
                
            
        }
       [HttpPost]
       public ActionResult Delete(Item model)
       {
           
           int result = new ItemRepository().DeleteItem(model);


           if (result == 0)
           {
               TempData["Success"] = "Deleted Successfully!";
               TempData["ItemRefNo"] = model.ItemRefNo;
               return RedirectToAction("Index");
           }
           else
           {
               if (result == 1)
               {
                   TempData["error"] = "Sorry!! You Cannot Delete This Item. It Is Already In Use";
                   TempData["ItemRefNo"] = null;
               }
               else
        {
                   TempData["error"] = "Oops!!..Something Went Wrong!!";
                   TempData["ItemRefNo"] = null;
               }
               return RedirectToAction("Index");
           }

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
        public ActionResult ItemList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new ItemRepository();
            var List = repo.GetItems();
            return PartialView("_ItemListView",List);
        }



        
    }
}