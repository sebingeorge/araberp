﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class OpeningStockController : Controller
    {
        // GET: OpeningStock
        public ActionResult Index()
        {
            return View();
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

            OpeningStock OpeningStock = new OpeningStock();
            OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
            if (stockpointId == null || stockpointId == 0)
            {
                var OpeningStockItem = new OpeningStockItem();
                OpeningStock.OpeningStockItem.Add(OpeningStockItem);
            }
            else
            {
                var repo = new OpeningStockRepository();
                OpeningStock.OpeningStockItem = repo.GetItem(stockpointId).ToList();
            }

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

        

              public ActionResult Save(OpeningStock model)
              {

                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                new OpeningStockRepository().DeleteOpeningStock(model);
                new OpeningStockRepository().InsertOpeningStock(model);
                new OpeningStockRepository().DeleteStockUpdate(model);
                new OpeningStockRepository().InsertStockUpdate(model);
                  
            
                FillStockpoint();
                FillItem();

                //TempData["Success"] = "Added Successfully!";
                OpeningStock OpeningStock = new OpeningStock();
                OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
                OpeningStock.OpeningStockItem.Add(new OpeningStockItem());
                return View("Create");
             }

        }
    }
