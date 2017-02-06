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
    public class ProjectStatusReportController : BaseController
    {
        // GET: ProjectStatusReport
        public ActionResult Index()
        {
            FillJobCard();
            return View();
        }

        public void FillJobCard()
        {
            ViewBag.JCList = new SelectList(new DropdownRepository().JobCardDropdown(OrganizationId), "Id", "Name");
        }
    }
}