using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockReportController : Controller
    {
        // GET: StockReport
        public ActionResult Index()
        {
            StockReportRepository repo = new StockReportRepository();
            return View(repo.GetStockReport());
        }
        public ActionResult DrillDown(int itemId)
        {
            StockReportRepository repo = new StockReportRepository();
            return View(repo.GetStockReportItemWise(itemId));
        }
    }
}