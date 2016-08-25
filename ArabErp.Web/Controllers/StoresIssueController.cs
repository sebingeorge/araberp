using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StoresIssueController : BaseController
    {
        // GET: StoresIssue
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Issuance(int id = 0)
        {
            string referenceNo = DatabaseCommonRepository.GetNextDocNo(24, OrganizationId);
            FillDropdowns();
            if (id == 0) return RedirectToAction("Pending");
            return View(new StoreIssue { WorkShopRequestId = id, StoreIssueDate = DateTime.Today, StoreIssueRefNo = referenceNo });
        }
        [HttpPost]
        public ActionResult Issuance(StoreIssue model)
        {
            try
            {
                List<int> temp = (from StoreIssueItem i in model.Items
                                  where i.CurrentIssuedQuantity > 0
                                  select i.StoreIssueId).ToList();
                List<StoreIssueItem> items = model.Items.Where(m => m.CurrentIssuedQuantity > 0).ToList();
                if (temp.Count == 0)
                {
                    TempData["error"] = "Atleast one of the quantities must be greater than zero";
                    goto ReturnSameView;
                }

                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string result = new StoreIssueRepository().InsertStoreIssue(model);
                if (result.Length != 0) //if insert success
                {
                    TempData["success"] = "Saved succesfully. Reference No. is " + result;
                    TempData["error"] = "";
                    return RedirectToAction("Pending");
                }
                else throw new Exception();
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

        ReturnSameView:
            FillDropdowns();
            TempData["success"] = "";
            return View(model); //if insert fails
        }

        private void FillDropdowns()
        {
            EmployeeDropdown();
            StockpointDropdown();
        }
        public ActionResult Pending()
        {
            return View();
        }
        public PartialViewResult PendingWorkshopRequests()
        {
            return PartialView("_PendingWorkshopRequests", new WorkShopRequestRepository().PendingWorkshopRequests());
        }
        public PartialViewResult PendingWorkshopRequestDetails()
        {
            StoreIssue _model = new StoreIssue { Items = new StoreIssueRepository().PendingWorkshopRequestItems(Convert.ToInt32(Request.QueryString["id"])).ToList() };
            return PartialView("_IssuanceItems", _model);
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public void StockpointDropdown()
        {
            ViewBag.stockpointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }
        public JsonResult WorkshopRequestHeadDetails(int workshopRequestId)
        {
            string data = new StoreIssueRepository().WorkshopRequestHeadDetails(workshopRequestId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PreviousList()
        {
            return View(new StoreIssueRepository().PreviousList());
        }
    }
}