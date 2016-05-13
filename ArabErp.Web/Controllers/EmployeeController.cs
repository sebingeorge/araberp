using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            var rep = new EmployeeRepository();
            var emp = rep.NewEmployee();
            ViewBag.designations= new SelectList(emp.Designations, "DesignationId", "DesignationName");
            return View(emp);
        }
    }
}