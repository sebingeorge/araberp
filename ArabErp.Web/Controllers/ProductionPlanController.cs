using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ProductionPlanController : Controller
    {
        // GET: ProductionPlan
        public ActionResult CreateProductionPlan()
        {
            return View();
        }
    }
}