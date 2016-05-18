using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace ArabErp.Web.Controllers
{
    public class StockPointController : Controller
    {
        // GET: StockPoint
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