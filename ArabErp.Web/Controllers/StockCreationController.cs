﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockCreationController : BaseController
    {
        // GET: StockCreation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillDropdowns();
            var model = new StockCreation
            {
                StockCreationDate = DateTime.Today,
                StockCreationRefNo = DatabaseCommonRepository.GetNextDocNo(25, OrganizationId),
                ConsumedItems = ConsumedItemsGrid(),
                FinishedGoods = FinishedGoodsGrid()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StockCreation model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                string ref_no = new StockCreationRepository().CreateStock(model);
                TempData["success"] = "Saved successfully. Reference No. is " + ref_no;
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while saving. Please try again.";
                FillDropdowns();
                return View(model);
            }
            return RedirectToAction("Create");  
        }

        public List<StockCreationFinishedGood> FinishedGoodsGrid()
        {
            var FinishedGoods = new List<StockCreationFinishedGood>();
            var a = new StockCreationFinishedGood();
            FinishedGoods.Add(a);
            return FinishedGoods;
        }

        public List<StockCreationConsumedItem> ConsumedItemsGrid()
        {
            var ConsumedItems = new List<StockCreationConsumedItem>();
            var b = new StockCreationConsumedItem();
            ConsumedItems.Add(b);
            return ConsumedItems;
        }

        public ActionResult PreviousList()
        {
            return View(new StockCreationRepository().GetStockCreations(organizationId: OrganizationId));
        }

        //public ActionResult Details(int id)
        //{
        //    string str = ViewData["mode"].ToString();
        //    return View(new StockCreationRepository().GetStockCreation(id));
        //}

        public ActionResult GetStockQuantity(string date, int id = 0, int stockpoint = 0)
        {
            try
            {
                if (id == 0 || stockpoint == 0 || date == null) throw new InvalidOperationException();
                ClosingStock stock = new ClosingStockRepository().GetClosingStockData(Convert.ToDateTime(date), stockpoint, 0, id, OrganizationId).First();
                return Json(new { Quantity = stock.Quantity, UnitName=stock.UnitName }, JsonRequestBehavior.AllowGet);
            }
            catch (InvalidOperationException)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Edit(int id = 0)
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request";
                return RedirectToAction("Index", "Home");
            }
            FillDropdowns();
            return View(new StockCreationRepository().GetStockCreation(id, OrganizationId));
        }

        #region Dropdowns
        public void FillDropdowns()
        {
            FillMaterial();
            FillStockpoint();
        }

        public void FillMaterial()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }

        public void FillStockpoint()
        {
            ViewBag.stockpointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }
        #endregion
    }
}