using ArabErp.DAL;
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
            FillStockpoint();
            var model = new StockCreation { StockCreationDate = DateTime.Today };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StockCreation model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserName;

                string ref_no = new StockCreationRepository().CreateStock(model);
                TempData["success"] = "Saved successfully. The Reference No. is " + ref_no;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured while saving. Please try again|" + ex.Message;
                FillDropdowns();
                return View("Create", model);
            }
            return RedirectToAction("Create");
        }

        public ActionResult FinishedGoodsGrid()
        {
            FillMaterial();
            var model = new StockCreation();
            model.FinishedGoods = new List<StockCreationFinishedGood>();
            var a = new StockCreationFinishedGood();
            model.FinishedGoods.Add(a);
            return PartialView("_FinishedGood", model);
        }
        public ActionResult ConsumedItemsGrid()
        {
            FillMaterial();
            var model = new StockCreation();
            model.ConsumedItems = new List<StockCreationConsumedItem>();
            var b = new StockCreationConsumedItem();
            model.ConsumedItems.Add(b);
            return PartialView("_ConsumedItem", model);
        }

        public ActionResult PreviousList()
        {
            return View(new StockCreationRepository().GetStockCreations());
        }

        //public ActionResult Details(int id)
        //{
        //    string str = ViewData["mode"].ToString();
        //    return View(new StockCreationRepository().GetStockCreation(id));
        //}

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