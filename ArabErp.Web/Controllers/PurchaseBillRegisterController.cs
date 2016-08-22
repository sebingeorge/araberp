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
    public class PurchaseBillRegisterController : BaseController
    {
        // GET: PurchaseBillRegister
        public ActionResult Index()
        {
            FillItem();
            FillSupplier();
            return View();
        }
        public ActionResult PurchaseBillRegister(DateTime? from, DateTime? to, int id = 0,int itmid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_PurchaseBillRegister", new PurchaseBillRegisterRepository().GetPurchaseBillRegisterData(from,to,id,itmid,OrganizationId));
        }


        public ActionResult PurchaseBillDetailed()
        {
            FillItem();
            FillItemCategory();
            return View();
        }

        public ActionResult PurchaseBillDetailedList(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_PurchaseBillDetailedList", new PurchaseBillRegisterRepository().PurchaseBillDetailedData(from, to, id, itmid, OrganizationId));
        }



        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PBItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.GrnSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }
    }
}