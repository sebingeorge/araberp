﻿using ArabErp.DAL;
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

            ClosingStock cs = new ClosingStock();
            cs.itmCatId = 0;
            return View("Index", cs);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemList = new SelectList(List, "Id", "Name");

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
        public ActionResult StockMovementRegister(DateTime? from, DateTime? to, int itmcatid = 0, int itmid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_StockMovementRegister", new StockMovementRegisterRepository().GetStockMovementData(from, to, itmcatid, itmid, OrganizationId));
        }
        public ActionResult Item(int Code)
        {
            FillItem(Code);
            return PartialView("_ItemDropDown");
        }

    }
}