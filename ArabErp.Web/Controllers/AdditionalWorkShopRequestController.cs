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
    public class AdditionalWorkShopRequestController : BaseController
    {
        // GET: AdditionalWorkShopRequest
        public ActionResult Index(int  isProjectBased)
        {
            ViewBag.isProjectBased = isProjectBased;
            FillCustomer();
            FillWorkshopRequests(isProjectBased);
           FillJobCard();
          // ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PreviousList(int isProjectBased, DateTime? from, DateTime? to, int id = 0, int jobcard = 0, int customer = 0)
        {
            
            //from = from ?? FYStartdate;
            //to = to ?? DateTime.Today;
            return PartialView("_PreviousListGrid", new WorkShopRequestRepository().PreviousList(isProjectBased: isProjectBased, OrganizationId: OrganizationId, from: from, to: to, id: id, jobcard: jobcard, customer: customer));
        }

        public ActionResult Create(int isProjectBased)
        {
            string internalid = "";
            JobCardDropdown(isProjectBased);

            if(isProjectBased == 0)
            {
                 internalid = DatabaseCommonRepository.GetNextDocNo(20, OrganizationId);
            }
            else
            {
                 internalid = DatabaseCommonRepository.GetNextDocNo(43, OrganizationId);
            }
           

            //return View(new Employee { EmployeeRefNo = "EMP/" + internalid });

            return View(new WorkShopRequest { WorkShopRequestDate = DateTime.Today, RequiredDate = DateTime.Today, WorkShopRequestRefNo = internalid, isProjectBased = isProjectBased });
        }
        [HttpPost]
        public ActionResult Create(WorkShopRequest model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                if (new WorkShopRequestRepository().InsertAdditionalWorkshopRequest(model) > 0)
                {
                    TempData["success"] = "Saved succesfully";
                    TempData["error"] = "";
                    return RedirectToAction("Create", new {model.isProjectBased});
                }
            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required value was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            JobCardDropdown(model.isProjectBased);
            return View("Create", model);
        }
        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    ItemDropdown();
                    FillPartNo();
                    WorkShopRequest WorkShopRequest = new WorkShopRequest();
                    WorkShopRequest = new WorkShopRequestRepository().WorkShopRequestHD(id);
                    JobCardDropdown(WorkShopRequest.isProjectBased);
                    WorkShopRequest.Items = new WorkShopRequestItemRepository().WorkShopRequestDT(id);
                    return View("Edit", WorkShopRequest);
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
        
        public ActionResult Delete(int id)
        {

            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new WorkShopRequestRepository().DeleteWorkShopRequest(id);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }
        public void JobCardDropdown(int isProjectBased, int jobCardId = 0)
        {
            ViewBag.JobCardList = new SelectList(new DropdownRepository().JobCardDropdownforAddtional(jobCardId: jobCardId, organizationId: OrganizationId, isProjectBased: isProjectBased), "Id", "Name");
        }
      
        public JsonResult GetJobCardDetails(int jobCardId)
        {
            var data = new WorkShopRequestRepository().GetJobCardDetails(jobCardId);
            return Json(new
            {
                SaleOrderId = data.SaleOrderId,
                SaleOrderNo = data.SaleOrderRefNo,
                Customer = data.CustomerName,
                CustomerId = data.CustomerId,
                CustomerOrderRef = data.CustomerOrderRef
            }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult AdditionalItemsList()
        {
            ItemDropdown();
            FillPartNo();
            WorkShopRequest workShopRequest = new WorkShopRequest { Items = new List<WorkShopRequestItem>() };
            workShopRequest.Items.Add(new WorkShopRequestItem());

            return PartialView("_AdditionalItemsList", workShopRequest);
        }

        public JsonResult GetItemUnit(int itemId)
        {
            return Json(new StockReturnItemRepository().GetItemUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemPartNo(int itemId)
        {
            return Json(new WorkShopRequestRepository().GetItemPartNo(itemId), JsonRequestBehavior.AllowGet);
        }

        #region Dropdowns
        private void FillWorkshopRequests(int isProjectBased)
        {
            ViewBag.wrList = new SelectList(new DropdownRepository().WorkshopRequestDropdown(OrganizationId: OrganizationId, isProjectBased:isProjectBased), "Id", "Name");
        }
        private void FillCustomer()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerForAdditionalWorkshopRequest(OrganizationId: OrganizationId), "Id", "Name");
        }
        private void FillJobCard()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardForAdditionalWorkshopRequest(OrganizationId: OrganizationId), "Id", "Name");
        }
        public void FillPartNo()
        {
            ViewBag.partNoList = new SelectList(new DropdownRepository().PartNoDropdown2(), "Id", "Name");
        }
        private void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().FillItem(), "Id", "Name");
        }
        #endregion
    }
}