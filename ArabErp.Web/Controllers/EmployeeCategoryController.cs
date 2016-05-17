﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

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

      
        public ActionResult Save(EmployeeCategory objEmployeeCategory)
        {
            var repo = new EmployeeCategoryRepository();
            new EmployeeCategoryRepository().InsertEmployeeCategory(objEmployeeCategory);
            return View("Create");
        }

    }
}