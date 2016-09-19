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
            return View();
        }

        public ActionResult EmployeeGrid(int? month)
        {
            var list = new ResourceAvailabilityRepository().GetEmployeeAvailability(month ?? DateTime.Today.Month, OrganizationId);
            return PartialView("_EmployeeGrid", list);
        }

        public void FillMonths()
        {
            
        }
    }
}