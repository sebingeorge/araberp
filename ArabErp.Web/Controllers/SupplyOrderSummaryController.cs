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
    public class SupplyOrderSummaryController : BaseController
    {
        // GET: SupplyOrderSummary
        public ActionResult Index()
        {
            FillSupplier();
            ViewBag.startdate = FYStartdate;
            return View();
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");

        }

        public ActionResult SupplyOrderSummary(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderSummary", new SupplyOrderRegisterRepository().GetSupplyOrderSummaryData(from, to, id, OrganizationId));
        }

    }
}