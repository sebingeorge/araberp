using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockReturnController : BaseController
    {
        // GET: StockReturn
        public ActionResult Index()
        {
            FillStockReturn();
            FillJobCardForPreviousList();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int jobcard = 0)
        {
            return PartialView("_PreviousListGrid", new StockReturnRepository().PreviousList(OrganizationId: OrganizationId, from: from, to: to, id: id, jobcard: jobcard));
        }
        public ActionResult Create()
        {
            FillJobCard();
            StockPointDropdown();
            string internalid = DatabaseCommonRepository.GetNextDocNo(21, OrganizationId);
            return View(new StockReturn { StockReturnDate = DateTime.Today, StockReturnRefNo = internalid });
        }

        [HttpPost]
        public ActionResult Create(StockReturn model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                if (new StockReturnRepository().InsertStockReturn(model) > 0)
                {
                    TempData["error"] = "";
                    TempData["success"] = "Saved successfully";
                    return RedirectToAction("Create");
                }
            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }

            FillJobCard();
            StockPointDropdown();
            return View("Create", model);
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
        public void StockPointDropdown()
        {
            ViewBag.stockPointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }

        public void FillStockReturn()
        {
            ViewBag.stockReturnList = new SelectList(new DropdownRepository().StockReturnDropdown(OrganizationId), "Id", "Name");
        }
        public void FillJobCardForPreviousList()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardForStockReturn(OrganizationId), "Id", "Name");
        }
    }
}