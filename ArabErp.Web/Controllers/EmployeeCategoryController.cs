using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class EmployeeCategoryController : Controller
    {
        // GET: EmployeeCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        {
            return View();
        }
    }
}