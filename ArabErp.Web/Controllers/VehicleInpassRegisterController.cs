using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Data;
using System.Data.SqlClient;


namespace ArabErp.Web.Controllers
{
    public class VehicleInpassRegisterController : BaseController
    {
        // GET: VehicleInpassRegister
        public ActionResult Index(string InstallType = "all")
        {
            ViewBag.startdate = FYStartdate;
            return View();
        }
        public ActionResult VehicleInpassRegister(DateTime? from, DateTime? to, string InstallType = "all", string CustomerName = "", string RegNo = "", string status = "'Delivered','Completed','Work in Progress','Work not Started','Pending JC Creation'")
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_VehicleInpassRegister", new VehicleInPassRepository().GetVehicleInpassRegister(from, to,InstallType, CustomerName, RegNo, status));
        }
    }
}