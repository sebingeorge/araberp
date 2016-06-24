using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SupplierController : Controller
    {
        SupplierRepository rep = new SupplierRepository();
        // GET: Supplier
        public ActionResult Index()
        {
            return View();

        }
        public ActionResult Create()
        {
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            FillPurchaseType();
            Supplier Supplier = new Supplier();
            Supplier.ContractDate = System.DateTime.Today;
            return View(Supplier);
        }

        public void FillCategoryDropdown()
        {
            var sup = rep.FillCategoryDropdown();
            ViewBag.SupplierCategory = new SelectList(sup, "Id", "Name");
        }

        public void FillCountryDropdown()
        {
            var sup = rep.FillCountryDropdown();
            ViewBag.SupplierCountry = new SelectList(sup, "Id", "Name");
        }

        public void FillCurrencyDropdown()
        {
            var sup = rep.FillCurrencyDropdown();
            ViewBag.SupplierCurrency = new SelectList(sup, "Id", "Name");
        }

        public void FillPurchaseType()
        {
            var sup = rep.FillPurchaseType();
            ViewBag.SupplierPurchaseType = new SelectList(sup, "Id", "Name");
        }

        public ActionResult Save(Supplier objSupplier)
        {
            var repo = new SupplierRepository();
            new SupplierRepository().InsertSupplier(objSupplier);
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            FillPurchaseType();
            Supplier Supplier = new Supplier();
            Supplier.ContractDate = System.DateTime.Today;
            return View("Create", objSupplier);
        }

    }
}