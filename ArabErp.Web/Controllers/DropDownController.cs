using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class DropDownController : Controller
    {
        EmployeeRepository _EmpRepo;
        public DropDownController(EmployeeRepository EmpRepo)
        {
            _EmpRepo = EmpRepo;
        }
        // GET: DropDown
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult EmployeeCategory()
        {
            var list = _EmpRepo.CategoryDropdownList();
            List<EmployeeCategoryDropDown> Category = new List<EmployeeCategoryDropDown>();
            Category.Add(new EmployeeCategoryDropDown
            {
                Code = 0,
                Name = "All"
            });
            foreach (var item in list)
            {
                Category.Add(item);
            }
            return PartialView("_EmployeeCategory", Category);
        }

      
    }
}