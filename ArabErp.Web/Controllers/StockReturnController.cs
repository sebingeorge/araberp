﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockReturnController : Controller
    {
        // GET: StockReturn
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateStockReturn()
        {
            return View();
        }
    }
}