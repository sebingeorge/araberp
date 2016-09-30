using ArabErp.DAL;
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
    public class WorkShopRequestController : BaseController
    {
        // GET: WorkShopRequest
        public ActionResult Index(int isProjectBased)
        {
            FillWRNo();
            FillCustomerinWR();
            ViewBag.ProjectBased = isProjectBased;
            return View();
        }
        [HttpGet]
        public ActionResult Create(int? SaleOrderId)
        {
            ItemDropdown();
            WorkShopRequestRepository repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetSaleOrderForWorkshopRequest(SaleOrderId ?? 0);
            model.WorkDescription = repo.GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(SaleOrderId ?? 0).WorkDescription;
            var WSList = repo.GetWorkShopRequestData(SaleOrderId ?? 0);
            model.Items = new List<WorkShopRequestItem>();
            //model.Isused = true;
            foreach (var item in WSList)
            {
                model.Items.Add(new WorkShopRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, ActualQuantity = item.Quantity });

            }
            string internalId = "";
            try
            {
                if (model.isProjectBased == 0)
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(19, OrganizationId);
                }
                else
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(31, OrganizationId);
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
                TempData["error"] = "Some error occurred. Please try again.|" + ex.Message;
            }
            model.WorkShopRequestRefNo = internalId;
            model.WorkShopRequestDate = System.DateTime.Today;
            model.RequiredDate = System.DateTime.Today;
            return View(model);
        }
        public ActionResult Pending(int isProjectBased)
        {
            ViewBag.ProjectBased = isProjectBased;
            return View();
        }
        public ActionResult WorkShopRequestPending(int isProjectBased, string saleOrder = "")
        {

            var rep = new SaleOrderRepository();


            var slist = rep.GetSaleOrdersPendingWorkshopRequest(OrganizationId,isProjectBased,saleOrder.Trim());

           

            return PartialView("_PendingGrid", slist);
        }
        [HttpPost]
        public ActionResult Save(WorkShopRequest model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                string id = new WorkShopRequestRepository().InsertWorkShopRequest(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. WorkShop Request Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("Pending", new {model.isProjectBased});
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
            }
            catch (NullReferenceException nx)
            {
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            return View("Pending", model);
        }
        public ActionResult PreviousList( int isProjectBased,DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousList", new WorkShopRequestRepository().GetPrevious(isProjectBased, from, to, id, cusid, OrganizationId));
        }
        public ActionResult Edit(int? id)
        {
            ItemDropdown();
            var repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetWorkshopRequestHdData(id ?? 0);
            model.Items = repo.GetWorkShopRequestDtData(id ?? 0);
          
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(WorkShopRequest model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();



            var repo = new WorkShopRequestRepository();
            try
            {
                new WorkShopRequestRepository().UpdateWorkShopRequest(model);
                TempData["success"] = "Updated Successfully (" + model.WorkShopRequestRefNo + ")";
                return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
        }

        public ActionResult Delete(int id = 0, int isProjectBased = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new WorkShopRequestRepository().DeleteWorkShopRequest(id);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index", new { isProjectBased = isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }
  
        public void FillWRNo()
        {
            ViewBag.WRNoList = new SelectList(new DropdownRepository().WRNODropdown(OrganizationId), "Id", "Name");
        }
        public void FillCustomerinWR()
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().WRCustomerDropdown(OrganizationId), "Id", "Name");
        }
        
        private void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public JsonResult GetItemUnit(int itemId)
        {
            return Json(new StockReturnItemRepository().GetItemUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemPartNo(int itemId)
        {
            return Json(new WorkShopRequestRepository().GetItemPartNo(itemId), JsonRequestBehavior.AllowGet);
        }
       
    }
}