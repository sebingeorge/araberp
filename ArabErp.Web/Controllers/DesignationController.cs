﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class DesignationController : Controller
    {
        // GET: Designation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        {
            return View();
        }
        public ActionResult Save(Designation objDesignation)
        {
            var repo = new DesignationRepository();
            new DesignationRepository().InsertDesignation(objDesignation);
            return View("Create");
        }
    }
}