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

        public ActionResult Create()
        {
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            FillPurchaseType();
            ViewBag.Title = "Create";
            Supplier Supplier = new Supplier();
            Supplier.SupplierRefNo = new SupplierRepository().GetRefNo(Supplier);
            Supplier.ContractDate = System.DateTime.Today;
            return View(Supplier);
        }

        [HttpPost]
        public ActionResult Create(Supplier model)
        {
       
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                var result = new SupplierRepository().InsertSupplier(model);
                
            if (result.SupplierId> 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["SupplierRefNo"] = result.SupplierRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SupplierRefNo"] = null;
                return View("Create", model);
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
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

                var result = new SupplierRepository().UpdateSupplier(model);

                if (result.SupplierId > 0)
                {
                    TempData["Success"] = "Updated Successfully!";
                    TempData["SupplierRefNo"] = result.SupplierRefNo;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SupplierRefNo"] = null;
                    return View("Edit", model);
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
             int result = new SupplierRepository().DeleteSupplier(model);
    
             if (result == 0)
           {
               TempData["Success"] = "Deleted Successfully!";
               TempData["SupplierRefNo"] = model.SupplierRefNo;
               return RedirectToAction("Index");
           }
           else
           {
               if (result == 1)
               {
                   TempData["error"] = "Sorry!! You Cannot Delete This Supplier. It Is Already In Use";
                   TempData["SupplierRefNo"] = null;
               }
               else
        {
                   TempData["error"] = "Oops!!..Something Went Wrong!!";
                   TempData["SupplierRefNo"] = null;
               }
               return RedirectToAction("Index");
           }

        }


    }
}