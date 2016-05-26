using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CustomerController : Controller
    {
        CustomerRepository rep = new CustomerRepository();
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            return View();
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

        public ActionResult Save(Customer objCustomer)
        {
            var repo = new CustomerRepository();
            new CustomerRepository().InsertCustomer(objCustomer);
            FillCategoryDropdown();
            FillCountryDropdown();
            FillCurrencyDropdown();
            return View("Create");
        }
    }
}