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
            var model = new StockCreation();
            return View();
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


        #region Dropdowns
        public void FillMaterial()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        #endregion
    }
}