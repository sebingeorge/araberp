using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace ArabErp.Web.Controllers
{
    public class WorkShopGRNController : BaseController
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: WorkShopGRN
        public ActionResult CreateWorkShopGRN()
        {
            FillSupplier();
            FillCurrency();
            WorkShopGRN WorkShopGRN = new WorkShopGRN();
            WorkShopGRN.GRNDate = System.DateTime.Today;
            return View(WorkShopGRN);
        }
        public ActionResult DisplayWGRNList()
        {
            FillItem();
            FillCurrency();
            WorkShopGRN WorkShopGRN = new WorkShopGRN();

            WorkShopGRN.WorkShopGRNItems = new List<WorkShopGRNItem>();
            var item = new WorkShopGRNItem();

            WorkShopGRN.WorkShopGRNItems.Add(item);
            return PartialView("_DisplayWGRNList", WorkShopGRN);
        }

        public void FillSupplier()
        {
            WorkShopGRNRepository repo = new WorkShopGRNRepository();
            var result = repo.GetSupplierList();
            ViewBag.SupplierList = new SelectList(result, "SupplierId", "SupplierName");
        }

        public void FillCurrency()
        {
            var repo = new WorkShopGRNRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }

        public void FillItem()
        {
            var repo = new WorkShopGRNRepository();
            var list = repo.FillItem();
            ViewBag.itemlist = new SelectList(list, "Id", "Name");
        }

        public JsonResult GetPartNo(int ItemId)
        {
            string str = new WorkShopGRNRepository().GetPartNo(ItemId);

            return Json(str, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(WorkShopGRN model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            new WorkShopGRNRepository().InsertWorkShopGRN(model);
            return RedirectToAction("CreateWorkShopGRN");
        }
    }
}