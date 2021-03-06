﻿using System;
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
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.CustomersDropdown();
            ViewBag.CustomerList1 = new SelectList(result, "Id", "Name");

        }
        public ActionResult CustomerList(int? page, string customer = "")
         {
            int pageNumber = page ?? 1;
            return PartialView("_CustomerListView", new CustomerRepository().GetCustomers(customer));
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
            ViewBag.CustomerCurrency = new SelectList(cus, "Id", "Name");
        }

        public ActionResult Create()
        {
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            ViewBag.Title = "Create";
            Customer Customer = new Customer();
            Customer.CustomerRefNo = new CustomerRepository().GetRefNo(Customer);
            return View(Customer);
        }

        [HttpPost]
        public ActionResult Create(Customer model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CustomerRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Customer", "CustomerName", model.CustomerName, null, null);
            if (!isexists)
            {
                var result = new CustomerRepository().InsertCustomer(model);
                if (result.CustomerId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["CustomerRefNo"] = result.CustomerRefNo;
                    return RedirectToAction("Index");
                }

                else
                {
                    FillCategoryDropdown();
                    FillCountryDropdown();
                    FillCurrencyDropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CustomerRefNo"] = null;
                return View("Create", model);
            }

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
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CustomerRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Customer", "CustomerName", model.CustomerName, "CustomerId", model.CustomerId);
            if (!isexists)
            {
                var result = new CustomerRepository().UpdateCustomer(model);
                if (result.CustomerId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CustomerRefNo"] = result.CustomerRefNo;
                    return RedirectToAction("Index");
                }

                else
                {
                    FillCategoryDropdown();
                    FillCountryDropdown();
                    FillCurrencyDropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillCategoryDropdown();
                FillCountryDropdown();
                FillCurrencyDropdown();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CustomerRefNo"] = null;
                return View("Create", model);
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
            int result = new CustomerRepository().DeleteCustomer(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CustomerRefNo"] = model.CustomerRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Customer. It Is Already In Use";
                    TempData["CustomerRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerRefNo"] = null;
                }
                return RedirectToAction("Index");
            }

        }

        public ActionResult ApprovalList(int Id)
        {
            //int Id = 0;
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            ViewBag.Title = "Approve";
            Customer objCustomer = new CustomerRepository().GetCustomer(Id);
            return View("Approve", objCustomer);

        }

       
         public ActionResult Approve(Customer model)
        {
            var repo = new CustomerRepository();
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
           

            try
            {
                new CustomerRepository().ApproveCustomer(model);
                TempData["Success"] = "Approved Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("Approve",model );
        }
    }
}