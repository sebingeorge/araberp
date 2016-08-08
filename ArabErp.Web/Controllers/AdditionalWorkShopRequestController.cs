using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class AdditionalWorkShopRequestController : BaseController
    {
        // GET: AdditionalWorkShopRequest
        public ActionResult Index()
        {
            FillCustomer();
            FillWorkshopRequests();
            FillJobCard();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int customer = 0, int jobcard = 0)
        {
            return PartialView("_PreviousListGrid", new WorkShopRequestRepository().PreviousList(OrganizationId: OrganizationId, from: from, to: to, id: id, jobcard: jobcard, customer: customer));
        }

        public ActionResult Create()
        {
            JobCardDropdown();
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(WorkShopRequest).Name);

            //return View(new Employee { EmployeeRefNo = "EMP/" + internalid });

            return View(new WorkShopRequest { WorkShopRequestDate = DateTime.Today, RequiredDate = DateTime.Today, WorkShopRequestRefNo = "WR/" + internalid });
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
                    return RedirectToAction("Create");
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
            JobCardDropdown();
            return View("Create", model);
        }
        public void JobCardDropdown()
        {
            ViewBag.JobCardList = new SelectList(new DropdownRepository().JobCardDropdown(), "Id", "Name");
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

            WorkShopRequest workShopRequest = new WorkShopRequest { Items = new List<WorkShopRequestItem>() };
            workShopRequest.Items.Add(new WorkShopRequestItem());

            return PartialView("_AdditionalItemsList", workShopRequest);
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

        #region Dropdowns
        private void FillWorkshopRequests()
        {
            ViewBag.wrList = new SelectList(new DropdownRepository().WorkshopRequestDropdown(OrganizationId: OrganizationId), "Id", "Name");
        }
        private void FillCustomer()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerForAdditionalWorkshopRequest(OrganizationId: OrganizationId), "Id", "Name");
        }
        private void FillJobCard()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardForAdditionalWorkshopRequest(OrganizationId: OrganizationId), "Id", "Name");
        }
        #endregion
    }
}