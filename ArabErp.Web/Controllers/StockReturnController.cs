using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockReturnController : Controller
    {
        // GET: StockReturn
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillJobCard();
            return View();
        }
        [HttpPost]
        public ActionResult Create(StockReturn model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new StockReturnRepository().InsertStockReturn(model);
            return RedirectToAction("Create");
        }
        public PartialViewResult StockReturnList(int jobCardId)
        {
            FillProduct(jobCardId);

            StockReturn stockReturn = new StockReturn { Items = new List<StockReturnItem>() };
            stockReturn.Items.Add(new StockReturnItem());

            return PartialView("_StockReturnList", stockReturn);
        }
        public void FillJobCard()
        {
            ViewBag.jobcardList = new SelectList(new StockReturnRepository().FillJobCard(), "Id", "Name");
        }
        public void FillProduct(int jobCardId)
        {
            ViewBag.productList = new SelectList(new StockReturnRepository().FillProduct(jobCardId), "Id", "Name");
        }
        public JsonResult GetJobCardDetails(int id)
        {
            string str = new StockReturnRepository().GetJobCardDetails(id);
            return Json(str, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemUnit(int itemId)
        {
            string str = new StockReturnItemRepository().GetItemUnit(itemId);
            return Json(str, JsonRequestBehavior.AllowGet);
        }
    }
}