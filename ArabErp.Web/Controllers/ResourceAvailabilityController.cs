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
            ViewBag.Year = FYStartdate.Year;
            return View();
        }

        public ActionResult EmployeeGrid(int? month, int? year)
        {
            var list = new ResourceAvailabilityRepository().GetEmployeeAvailability(month ?? DateTime.Today.Month, year ?? DateTime.Today.Year, OrganizationId);
            list[0].Month = month ?? DateTime.Today.Month;
            list[0].Year = year ?? FYStartdate.Year;
            return PartialView("_EmployeeGrid", list);
        }
    }
}