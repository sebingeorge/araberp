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
    public class DirectPurchaseController : BaseController
    {
        // GET: LocalPurchase
        public ActionResult Index()
        {
            return View(new DirectPurchaseRepository().GetPreviousList());
        }
        public ActionResult CreateRequest()
        {
            FillSO();
            FillJC();
            GetMaterials();
            List<DirectPurchaseRequestItem> list = new List<DirectPurchaseRequestItem>();
            list.Add(new DirectPurchaseRequestItem());
            return View("Create", new DirectPurchaseRequest { items = list, PurchaseRequestDate = DateTime.Today, RequiredDate = DateTime.Today });
        }
        [HttpPost]
        public ActionResult CreateRequest(DirectPurchaseRequest model)
        {
            FillSO();
            FillJC();
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if (new DirectPurchaseRepository().InsertDirectPurchaseRequest(model) > 0)
            {
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("CreateRequest");
            }
            else
            {
                GetMaterials();
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }
        }
        public void GetMaterials()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public JsonResult GetPartNoUnit(int itemId)
        {
            return Json(new ItemRepository().GetPartNoUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseLimit()
        {
            string str;
            try
            {
                int organizationId = OrganizationId;
                str = new DirectPurchaseRepository().GetPurchaseLimit(organizationId);
            }
            catch (Exception)
            {
                str = "error";
            }
            return Json(str, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ValidateForm(string requestNo, int total = 0)
        {
            int val1 = new DirectPurchaseRepository().isNotExist(requestNo);
            int val2 = new DirectPurchaseRepository().validateTotal(total);
            string str = "";
            if (val1 != 1)
                str += "Purchase request number already exists";
            if (val2 != 1)
                str += "|Total amount should not exceed the purchase limit";
            if (str.Length == 0)
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            return Json(new { status = false, message = str }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Approval()
        {
            return View(new DirectPurchaseRepository().GetUnApprovedRequests());
        }
        public JsonResult Approve(int id)
        {
            try
            {
                TempData["success"] = "Approved successfully";
                TempData["error"] = "";
                new DirectPurchaseRepository().ApproveRequest(id);
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (SqlException)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.";
                return Json("error|Some error occured while connecting to database. Please check your network connection and try again.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return Json("error|" + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public void FillSO()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillSO();
            ViewBag.SO = new SelectList(list, "Id", "Name");
        }
        public void FillJC()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillJC();
            ViewBag.JC = new SelectList(list, "Id", "Name");
        }

    }
}