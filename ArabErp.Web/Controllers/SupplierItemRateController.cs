using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SupplierItemRateController : BaseController
    {
        // GET: SupplierItemRate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillSupplier();

            SupplierItemRate SupplierItemRate = new SupplierItemRate();
            SupplierItemRate.SupplierItemRateItem = new List<SupplierItemRateItem>();
            SupplierItemRate.SupplierItemRateItem.Add(new SupplierItemRateItem());

            return View(SupplierItemRate);
        }

        public ActionResult SupplierItemRateList(int? SupplierId)
        {
            FillItem();

            SupplierItemRate SupplierItemRate = new SupplierItemRate();
            SupplierItemRate.SupplierItemRateItem = new List<SupplierItemRateItem>();
            if (SupplierId == null || SupplierId == 0)
            {
                var SupplierItemRateItem = new SupplierItemRateItem();
                SupplierItemRate.SupplierItemRateItem.Add(SupplierItemRateItem);
            }
            else
            {
                var repo = new SupplierItemRateRepository();
                SupplierItemRate.SupplierItemRateItem = repo.GetItem(SupplierId).ToList();
            }

            if (SupplierItemRate.SupplierItemRateItem.Count == 0)
                SupplierItemRate.SupplierItemRateItem.Add(new SupplierItemRateItem());
            return PartialView("SupplierItemRateList", SupplierItemRate);
        }


        public void FillSupplier()
        {
            var repo = new SupplierItemRateRepository();
            var list = repo.FillSupplier();
            ViewBag.Supplierlist = new SelectList(list, "Id", "Name");
        }
        public void FillItem()
        {
            var repo = new SupplierItemRateRepository();
            var list = repo.FillItem();
            ViewBag.Itemlist = new SelectList(list, "Id", "Name");
        }



        public ActionResult Save(SupplierItemRate model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new SupplierItemRateRepository().DeleteSupplierItemRate(model);
            new SupplierItemRateRepository().InsertSupplierItemRate(model);

            FillSupplier();
            FillItem();

            //TempData["Success"] = "Added Successfully!";
            SupplierItemRate SupplierItemRate = new SupplierItemRate();
            SupplierItemRate.SupplierItemRateItem = new List<SupplierItemRateItem>();
            SupplierItemRate.SupplierItemRateItem.Add(new SupplierItemRateItem());
            //return View("Create");
            return RedirectToAction("Create");
        }
    }
}