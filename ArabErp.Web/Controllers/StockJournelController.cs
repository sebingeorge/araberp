using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockJournelController : Controller
    {
        // GET: StockJournel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateStockJournel()
        {
            return View();
        }
    }
}