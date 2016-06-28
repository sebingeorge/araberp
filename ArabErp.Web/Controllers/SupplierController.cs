using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SupplierController : BaseController
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
            ViewBag.Title = "Create";
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

        
        public ActionResult SupplierList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new SupplierRepository();
            var List = repo.GetSuppliers();
            return PartialView("_SupplierListView", List);
        }
        [HttpPost]
        public ActionResult Create(Supplier model)
        {
            if (ModelState.IsValid)
            {
                var repo = new SupplierRepository();
                new SupplierRepository().InsertSupplier(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                FillPurchaseType();
                return View(model);
            }
        }


        public ActionResult Edit(int Id)
        {
            //int Id = 0;
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            FillPurchaseType();
            ViewBag.Title = "Edit";
            Supplier objSupplier = new SupplierRepository().GetSupplier(Id);
            return View("Create", objSupplier);


        }

        [HttpPost]
        public ActionResult Edit(Supplier model)
        {

            if (ModelState.IsValid)
            {
                var repo = new SupplierRepository();
                new SupplierRepository().UpdateSupplier(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                FillPurchaseType();
                return View(model);
            }

      }

        public ActionResult Delete(int Id)
        {
            //int Id = 0;
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            FillPurchaseType();
            ViewBag.Title = "Delete";
            Supplier objSupplier = new SupplierRepository().GetSupplier(Id);
            return View("Create", objSupplier);


        }

        [HttpPost]
        public ActionResult Delete(Supplier model)
        {

            if (ModelState.IsValid)
            {
                var repo = new SupplierRepository();
                new SupplierRepository().DeleteSupplier(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                FillPurchaseType();
                return View(model);
            }

        }


    }
}