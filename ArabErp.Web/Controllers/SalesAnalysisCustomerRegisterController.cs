using ArabErp.DAL;
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
    public class SalesAnalysisCustomerRegisterController : BaseController
    {
        // GET: SalesAnalysisCustomerRegister
        public ActionResult Index()
        {
            FillCustomer();
            return View();
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
        public ActionResult SalesAnalysisCustomer(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_SalesAnalysisCustomer", new SalesRegisterRepository().GetSalesAnalysisCustomerWise(from, to, id, OrganizationId));
        }
      
    }
}