using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class CustomerVsWorkRateController : BaseController
    {
        // GET: CustomerVsWorkRate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillCustomer();

            CustomerVsWorkDescriptionRate CustomerVsWorkDescriptionRate = new CustomerVsWorkDescriptionRate();
            CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = new List<CustomerVsWorkRateItem>();
            CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem.Add(new CustomerVsWorkRateItem());

            return View(CustomerVsWorkDescriptionRate);
        }

        public ActionResult CustomerItemRateList(int? CustomerId)
        {


            CustomerVsWorkDescriptionRate CustomerVsWorkDescriptionRate = new CustomerVsWorkDescriptionRate();
            CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = new List<CustomerVsWorkRateItem>();
            var repo = new CustomerVsWorkRateRepository();

            if (CustomerId == null || CustomerId == 0)
            {
              
                CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = repo.GetWorkList().ToList();
            }
            else
            {
                CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = repo.GetItem(CustomerId).ToList();
            }

            if (CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem.Count == 0)
                CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = repo.GetWorkList().ToList();
            return PartialView("CustomerItemRateList", CustomerVsWorkDescriptionRate);
        }

        public void FillCustomer()
        {
            var repo = new CustomerVsWorkRateRepository();
            var list = repo.FillCustomer();
            ViewBag.Customerlist = new SelectList(list, "Id", "Name");
        }

        public ActionResult Save(CustomerVsWorkDescriptionRate model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            new CustomerVsWorkRateRepository().DeleteCustomerItemRate(model);
            new CustomerVsWorkRateRepository().InsertCustomerItemRate(model);

            FillCustomer();
            

            //TempData["Success"] = "Added Successfully!";
            CustomerVsWorkDescriptionRate CustomerVsWorkDescriptionRate = new CustomerVsWorkDescriptionRate();
            CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem = new List<CustomerVsWorkRateItem>();
            CustomerVsWorkDescriptionRate.CustomerVsWorkRateItem.Add(new CustomerVsWorkRateItem());
            //return View("Create");
            return RedirectToAction("Create");
        }

    }
}