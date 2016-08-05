using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers 
{
    public class StockJournalController : BaseController
    {
        // GET: StockJournal
        public ActionResult Index()
        {
            FillStockJournal();
            FillStockPointForPreviousList();
            return View();
        }
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int stockpoint = 0)
        {
            return PartialView("_PreviousListGrid", new StockJournalRepository().PreviousList(OrganizationId: OrganizationId, from: from, to: to, id: id, stockpoint: stockpoint));
        }
        public ActionResult Create()
        {
            FillStockPoint();
            FIllEmployee();
            FIllStockItems(0);
            //StockJournal StockJournal = new StockJournal();
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(StockJournal).Name);
            StockJournal StockJournalList = new StockJournal { StockJournelItems = new List<StockJournalItem>(), StockJournalRefno = "SJ/" + internalid };

            StockJournalList.StockJournelItems.Add(new StockJournalItem());
            //StockJournal.StockJournalRefno = "SJ/" + internalid;
            return View("Create", StockJournalList);
            //return View(new StockJournal { StockJournalRefno = "SJ/" + internalid });
         
        }
         [HttpPost]
        public ActionResult Create(StockJournal model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            if (new StockJournalRepository().InsertStockJournal(model) > 0)
            {
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("Create");
            }
            else
            {
                FillStockPoint();
                FIllEmployee();
                FIllStockItems(model.StockPointId);
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
                
            }

            
        }
        public PartialViewResult StockJournelList(int? StockPointId)
        {

            FIllStockItems(StockPointId);
          
            StockJournal StockJournalList = new StockJournal { StockJournelItems = new List<StockJournalItem>() };
            StockJournalList.StockJournelItems.Add(new StockJournalItem());
            //StockJournal List = new StockJournal();
            //List.StockJournelItems = new StockJournalItem();
            return PartialView("_StockJournalList", StockJournalList);
        }
        
        public JsonResult GetItemDetails(int itemId)
        {
            return Json(new StockJournalRepository().GetItemDetails(itemId), JsonRequestBehavior.AllowGet);
        }

        #region Dropdown
        public void FillStockPoint()
        {
            ViewBag.StockPointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }
        public void FIllStockItems(int? StockPointId)
        {
            ViewBag.StockJournalItems = new SelectList(new DropdownRepository().StockJournelItemsDropdown(StockPointId), "Id", "Name");
        }
        public void FIllEmployee()
        {
            ViewBag.EmployeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public void FillStockJournal()
        {
            ViewBag.stockJournalList = new SelectList(new DropdownRepository().StockJournalDropdown(OrganizationId), "Id", "Name");
        }
        public void FillStockPointForPreviousList()
        {
            ViewBag.stockpointList = new SelectList(new DropdownRepository().StockPointForStockJournal(OrganizationId), "Id", "Name");
        }
        #endregion
    }
}