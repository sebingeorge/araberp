using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;
namespace ArabErp.Web.Controllers
{
    public class SalesRegisterController : BaseController
    {
        // GET: SalesRegister
        public ActionResult Index()
        {
            ViewBag.startdate = Startdate;
            FillCustomer();
            return View();
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public ActionResult SaleRegister(DateTime? from, DateTime? to, int id = 0)
        {
          
            from = from ?? Startdate;
            to = to ?? DateTime.Today;
            return PartialView("_SaleRegister", new SalesRegisterRepository().GetSalesRegister(from, to, id, OrganizationId));
        }
      
      
      
    }
}