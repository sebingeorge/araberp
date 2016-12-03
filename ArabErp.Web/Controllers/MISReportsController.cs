﻿using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class MISReportsController : BaseController
    {
        // GET: MISReports
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DCReport()
        {
            ViewBag.Year = FYStartdate.Year;
            return View();
        }

        public ActionResult DCReportGrid(int? month, int? year)
        {
            return PartialView("_DCReportGrid", new MISReportsRepository().GetDCReport(month, year, OrganizationId));
        }
    }
}