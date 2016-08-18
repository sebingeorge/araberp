﻿using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class SalesRegisterController : BaseController
    {
        // GET: SalesRegister
        public ActionResult Index()
        {

            FillCustomer();
            return View();
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SOCustomerDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public ActionResult SaleRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_SaleRegister", new SalesRegisterRepository().GetSalesRegister(from, to, id, OrganizationId));
        }
      
      
    }
}