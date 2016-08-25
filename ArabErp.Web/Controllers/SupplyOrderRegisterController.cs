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
            ViewBag.startdate = FYStartdate;
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
            var result = repo.SOItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public ActionResult SupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderRegister", new SupplyOrderRegisterRepository().GetSupplyOrderRegisterData(from, to, id, itmid, OrganizationId));
        }

        public ActionResult PengingSO()
        {
           
            FillSupplier();
            FillItem();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PendingSupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSupplyOrderRegister", new SupplyOrderRegisterRepository().GetPendingSupplyOrderRegister(from, to, id, itmid, OrganizationId));
        }


        public ActionResult SOVariance()
        {
           
            FillSupplier();
            FillItem();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult SupplyOrderVarianceReport(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderVarianceReport", new SupplyOrderRegisterRepository().GetSOVarianceData(from, to, id, itmid, OrganizationId));
        }

    }
}