﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ItemGroupController : BaseController
    {
        // GET: ItemGroup
        public ActionResult Index()
        {
            return View();
        }
         public ActionResult Create()
        {
            //var repo = new ItemGroupRepository();
            //var sym = repo.FillCategory();
            //ViewBag.ItemCategory = new SelectList(sym.ItemCategory, "itmCatId", "CategoryName");
            dropdown();
            return View();
        }
         public ActionResult Save(ItemGroup objItemGroup)
         {
             dropdown();
             new ItemGroupRepository().InsertItemGroup(objItemGroup);
             return View("Create");
         }
         public void dropdown()
         {
             var repo = new ItemGroupRepository();
             var List = repo.FillCategory();
             ViewBag.ItemCategory = new SelectList(List, "Id", "Name");
         }
        public ActionResult FillItemGroupList(int?page)
         {
             int itemsPerPage = 10;
             int pageNumber = page ?? 1;
             var repo = new ItemGroupRepository();
             var List = repo.FillItemGroupList();
             return PartialView("ItemGroupListView", List);
         }
    }
}