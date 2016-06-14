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
                return RedirectToAction("CreateRequest");
            }
            else
            {
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
    }
}