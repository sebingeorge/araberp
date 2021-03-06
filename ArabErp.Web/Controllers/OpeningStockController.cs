﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class OpeningStockController : BaseController
    {
        // GET: OpeningStock
        public ActionResult Index()
        {
            FillStockpoint();
            return View();
        }
        public ActionResult OpeningStock()
        {
            OpeningStock OpeningStock = new OpeningStock();
            OpeningStock.OpeningStockItem = new OpeningStockRepository().GetItem();
            return PartialView("_OpeningStock", OpeningStock);
        }
        public ActionResult Create()
        {
            FillStockpoint();

            OpeningStock OpeningStock = new OpeningStock();
            OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
            OpeningStock.OpeningStockItem.Add(new OpeningStockItem());

            return View(OpeningStock);
        }

        public ActionResult OpeningStockList(int? stockpointId)
        {
            FillItem();
            FillPartNo();

            OpeningStock OpeningStock = new OpeningStock();
            OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
            if (stockpointId == null || stockpointId == 0)
            {
                var OpeningStockItem = new OpeningStockItem { Quantity = 0 };
                OpeningStock.OpeningStockItem.Add(OpeningStockItem);
            }
            else
            {
                var repo = new OpeningStockRepository();
                OpeningStock.OpeningStockItem = repo.GetItem(stockpointId).ToList();
            }

            if (OpeningStock.OpeningStockItem.Count == 0)
                OpeningStock.OpeningStockItem.Add(new OpeningStockItem());
            return PartialView("OpeningStockList", OpeningStock);
        }

        public void FillStockpoint()
        {
            var repo = new OpeningStockRepository();
            var list = repo.FillStockpoint();
            ViewBag.Stockpointlist = new SelectList(list, "Id", "Name");
        }

        public void FillItem()
        {
            var repo = new OpeningStockRepository();
            var list = repo.FillItem();
            ViewBag.Itemlist = new SelectList(list, "Id", "Name");
        }

        public void FillPartNo()
        {
            ViewBag.partNoList = new SelectList(new DropdownRepository().PartNoDropdown1(), "Id", "Name");
        }

        public ActionResult Save(OpeningStock model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            //new OpeningStockRepository().DeleteOpeningStock(model);
            //new OpeningStockRepository().InsertOpeningStock(model);
            //new OpeningStockRepository().DeleteStockUpdate(model);
            //new OpeningStockRepository().InsertStockUpdate(model);
            
            //new OpeningStockRepository().UpdateOpeningStock(model);
            new OpeningStockRepository().SaveOpeningStock(model);
            FillStockpoint();
            FillItem();
            FillPartNo();

            TempData["Success"] = "Added Successfully!";
            OpeningStock OpeningStock = new OpeningStock();
            OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
            OpeningStock.OpeningStockItem.Add(new OpeningStockItem());
            return RedirectToAction("Index");
        }

    }
}
