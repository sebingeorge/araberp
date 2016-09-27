using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

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
            string internalid = DatabaseCommonRepository.GetNextDocNo(22, OrganizationId);
            StockJournal StockJournalList = new StockJournal { StockJournelItems = new List<StockJournalItem>(), StockJournalRefno = internalid };

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
            model.CreatedBy = UserID.ToString();

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

         public ActionResult Edit(int id = 0)
         {
             try
             {
                 if (id != 0)
                 {
                     FillStockPoint();
                     FIllEmployee();
                     StockJournal StockJournal = new StockJournal();
                     StockJournal = new StockJournalRepository().GetStockJournalHD(id);
                     FIllStockItems(StockJournal.StockPointId);
                     StockJournal.StockJournelItems = new StockJournalItemsRepository().GetStockJournalDT(id);

                     return View(StockJournal);
                 }
                 else
                 {
                     TempData["error"] = "That was an invalid/unknown request. Please try again.";
                     TempData["success"] = "";
                 }
             }
             catch (InvalidOperationException iox)
             {
                 TempData["error"] = "Sorry, we could not find the requested item. Please try again.|" + iox.Message;
             }
             catch (SqlException sx)
             {
                 TempData["error"] = "Some error occured while connecting to database. Please try again after sometime.|" + sx.Message;
             }
             catch (NullReferenceException nx)
             {
                 TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
             }
             catch (Exception ex)
             {
                 TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
             }

             TempData["success"] = "";
             return RedirectToAction("Index");
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