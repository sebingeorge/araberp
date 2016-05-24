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
        EmployeeRepository rep = new EmployeeRepository();
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillDesignationDropdown();
            FillCategoryDropdown();
            FillLocationDropdown();
            FillTaskDropdown();
           
            return View();
        }
        public void FillDesignationDropdown()
        {
            var emp = rep.FillDesignationDropdown();
            ViewBag.EmployeeDesignations = new SelectList(emp ,"Id", "Name");
        }
        public void FillCategoryDropdown()
        {
            var emp = rep.FillCategoryDropdown();
            ViewBag.EmployeeCategory = new SelectList(emp, "Id", "Name");
        }
        public void FillLocationDropdown()
        {
            var emp = rep.FillLocationDropdown();
            ViewBag.EmployeeLocation = new SelectList(emp, "Id", "Name");
        }
        public void FillTaskDropdown()
        {
            var emp = rep.FillTaskDropdown();
            ViewBag.EmployeeTask = new SelectList(emp, "Id", "Name");
        }
    }
}