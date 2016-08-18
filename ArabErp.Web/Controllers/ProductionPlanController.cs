using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ProductionPlanController : BaseController
    {
        // GET: ProductionPlan
        public ActionResult CreateProductionPlan()
        {
            return View();
        }

        public ActionResult CreateProductLine()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}