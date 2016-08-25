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
    public class PendingSaleOrderRegisterController : BaseController
    {
        // GET: PendingSaleOrderRegister
        //PendingSaleOrderRegister
        public ActionResult Index(string rptType)
        {
            ViewBag.type = rptType;
            ViewBag.startdate = Startdate;
            FillCustomer();
            return View();
        }
        //SaleOrderVarianceRegister
      
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SOCustomerDropDown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
        public ActionResult PendingSaleOrderRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? Startdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSaleOrderRegister", new SalesRegisterRepository().GetPendingSO(from, to, id, OrganizationId));
        }
        public ActionResult SaleOrderVarianceRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? Startdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSaleOrderRegister", new SalesRegisterRepository().GetPendingSO(from, to, id, OrganizationId));
        }
      
      
    }
}