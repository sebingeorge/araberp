using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ResourceAvailabilityController : BaseController
    {
        // GET: ResourceAvailability
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Employee()
        {
            ViewBag.StartYear = FYStartdate.Year;// == DateTime.Today.Year ? FYStartdate.Year : FYEnddate.Year;
            ViewBag.EndYear = FYEnddate.Year;// == DateTime.Today.Year ? FYStartdate.Year : FYEnddate.Year;
            return View();
        }

        public ActionResult EmployeeGrid(int? month, int? year, int isProject = 0)
        {
            var list = new ResourceAvailabilityRepository().GetEmployeeAvailability(month ?? DateTime.Today.Month, year ?? DateTime.Today.Year, isProject, OrganizationId);
            try
            {
                ViewBag.Month = month ?? DateTime.Today.Month;
                ViewBag.Year = year ?? DateTime.Today.Year;
            }
            catch (Exception) { }
            return PartialView("_EmployeeGrid", list);
        }
    }
}