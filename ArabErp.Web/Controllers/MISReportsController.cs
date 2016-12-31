using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class MISReportsController : BaseController
    {
        // GET: MISReports
        public ActionResult Index()
        {
            return View();
        }

        #region Delivery Tracking Report (DC Report)
        public ActionResult DCReport()
        {
            FillNeworService();
            ViewBag.Year = FYStartdate.Year;
            return View();
        }
        public ActionResult DCReportGrid(int? month, int? year, string ChassisNo = "", string UnitSlNo = "",
            string Customer = "", string JobcardNo = "", string InstallType = "all")
        {
            return PartialView("_DCReportGrid", new MISReportsRepository().GetDCReport(OrganizationId, month, year, ChassisNo, UnitSlNo, Customer, JobcardNo, InstallType));
        } 
        #endregion

        #region Material Activity Report
        public ActionResult MaterialActivityReport()
        {
            FillMaterial();
            return View();
        }

        public ActionResult GetPendingLPO(int item)
        {
            try
            {
                var list = new MISReportsRepository().GetPendingLPO(item, OrganizationId);
                return PartialView("_PendingLPO", list);
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred while loading the report. Please try again.";
                throw;
            }
        } 
        #endregion

        #region Dropdowns
        public void FillNeworService()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 1, Name = "New Installation" });
            types.Add(new Dropdown { Id = 2, Name = "Service" });
            ViewBag.Type = new SelectList(types, "Id", "Name");
        }
        public void FillMaterial()
        {
            var list = new DropdownRepository().ItemDropdown();
            ViewBag.materialList = new SelectList(list, "Id", "Name");
        }
        #endregion
    }
}