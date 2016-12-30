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
    public class GRNRegisterController : BaseController
    {
        // GET: GRNRegister
        public ActionResult Index()
        {
            FillItemGroup();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public void FillItemGroup()
        {
            ViewBag.ItmGrpList = new SelectList(new DropdownRepository().ItemGroupDropdown(), "Id", "Name");
        }

        public ActionResult GRNRegister(DateTime? from, DateTime? to, int id = 0, string material = "", string supplier = "")
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_GRNRegister", new GRNRegisterRepository().GetGRNRegister(from, to, id, material, supplier));
        }

    }
}