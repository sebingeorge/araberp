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
    public class ServiceRegisterController : BaseController
    {
        // GET: ServiceRegister
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ServiceRegister(string Customer = "")
        {

            return PartialView("_ServiceRegister", new ServiceRegisterRepository().GetServiceRegister(Customer));
        }
    }
}