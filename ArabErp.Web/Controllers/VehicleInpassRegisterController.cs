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
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult VehicleInpassRegister()
        {
            return PartialView("_VehicleInpassRegister",new VehicleInPassRepository().GetVehicleInpassRegister());
        }
    }
}