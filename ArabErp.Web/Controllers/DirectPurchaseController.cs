using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class DirectPurchaseController : Controller
    {
        // GET: LocalPurchase
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateRequest()
        {
            GetMaterials();
            List<DirectPurchaseRequestItem> list = new List<DirectPurchaseRequestItem>();
            list.Add(new DirectPurchaseRequestItem());
            return View("Create", new DirectPurchaseRequest { items = list });
        }
        [HttpPost]
        public ActionResult CreateRequest(DirectPurchaseRequest model)
        {
            model.OrganizationId = 1;
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
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return RedirectToAction("CreateRequest");
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
                int organizationId = 1;
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
            string str="";
            if (val1 != 1)
                str += "Purchase request number already exists";
            if (val2 != 1)
                str += "|Total amount should not exceed the purchase limit";
            if(str.Length == 0)
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            return Json(new { status = false, message = str }, JsonRequestBehavior.AllowGet);
        }
    }
}