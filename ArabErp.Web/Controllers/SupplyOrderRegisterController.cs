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
    public class SupplyOrderRegisterController : BaseController
    {
        // GET: SupplyOrderRegister
        public ActionResult Index()
        {
            FillSupplier();
            FillItem();
            return View();
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");

        }
        public void FillItem()
        {
            DropdownRepository repo=new DropdownRepository();
            var result = repo.ItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public ActionResult SupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderRegister", new SupplyOrderRegisterRepository().GetSupplyOrderRegisterData(from, to, id, itmid, OrganizationId));
        }

    }
}