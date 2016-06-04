using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StoresIssueController : Controller
    {
        // GET: StoresIssue
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Issuance(int id=0)
        {
            EmployeeDropdown();
            StockpointDropdown();
            if (id == 0) return RedirectToAction("Pending");
            return View(new StoreIssue { WorkShopRequestId = id });
        }
        [HttpPost]
        public ActionResult Issuance(StoreIssue model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if(new StoreIssueRepository().InsertStoreIssue(model) >0) return RedirectToAction("Pending"); //if insert success
            return View(new { id = model.WorkShopRequestId }); //if insert fails
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
            StoreIssue _model = new StoreIssue{ Items=new StoreIssueRepository().PendingWorkshopRequestItems(Convert.ToInt32(Request.QueryString["id"])).ToList()};
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
    }
}