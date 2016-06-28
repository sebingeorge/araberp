using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CustomerController : BaseController
    {
        CustomerRepository rep = new CustomerRepository();
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CustomerList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CustomerRepository();
            var List = repo.GetCustomers();
            return PartialView("_CustomerListView",List);
        }

     
        public void FillCategoryDropdown()
        {
            var cus = rep.FillCategoryDropdown();
            ViewBag.CustomerCategory = new SelectList(cus, "Id", "Name");
        }

        public void FillCountryDropdown()
        {
            var cus = rep.FillCountryDropdown();
            ViewBag.CustomerCountry = new SelectList(cus, "Id", "Name");
        }

        public void FillCurrencyDropdown()
        {
            var cus = rep.FillCurrencyDropdown();
            ViewBag.CustomerCurrency=new SelectList (cus,"Id","Name");
        }

        public ActionResult Create()
        {
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            ViewBag.Title = "Create";
            return View();
        }

        [HttpPost]
        public ActionResult Create(Customer model)
        {
            //if (ModelState.IsValid)
            //{
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

                var repo = new CustomerRepository();
                new CustomerRepository().InsertCustomer(model);
                return RedirectToAction("Create");
            //}
            //else
            //{
            //    FillCategoryDropdown();
            //    FillCountryDropdown();
            //    FillCurrencyDropdown();
            //    return View(model);
            //}
        }


         public ActionResult Edit(int Id)
        {
            //int Id = 0;
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            ViewBag.Title = "Edit";
            Customer objCustomer = new CustomerRepository().GetCustomer(Id);
            return View("Create", objCustomer);


        }

        [HttpPost]
        public ActionResult Edit(Customer model)
        {

            if (ModelState.IsValid)
            {
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

                var repo = new CustomerRepository();
                new CustomerRepository().UpdateCustomer(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                return View(model);
            }

        }

        public ActionResult Delete(int Id)
        {
            //int Id = 0;
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            ViewBag.Title = "Delete";
            Customer objCustomer = new CustomerRepository().GetCustomer(Id);
            return View("Create", objCustomer);


        }

        [HttpPost]
        public ActionResult Delete(Customer model)
        {

            if (ModelState.IsValid)
            {
                var repo = new CustomerRepository();
                new CustomerRepository().DeleteCustomer(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                return View(model);
            }

        }


    }
}