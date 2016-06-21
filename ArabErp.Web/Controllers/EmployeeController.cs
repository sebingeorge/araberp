using ArabErp.DAL;
using ArabErp.Domain;
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

        public ActionResult Save(Employee model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if(new EmployeeRepository().Insert(model)>0)
                return RedirectToAction("Create");

            FillDesignationDropdown();
            FillCategoryDropdown();
            FillLocationDropdown();

            return View("Create", model);
        }
        //public ActionResult FillEmployeeList()
        //{
        //    //var repo = new EmployeeRepository();
        //    var List = rep.FillEmployeeList();
        //    return PartialView("EmployeeListView", List);
        //}

        public ActionResult FillEmployeeList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new EmployeeRepository();
            var List = rep.FillEmployeeList();
            return PartialView("_EmployeeListView", List);
        }
    }
}