using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ProfitabilityReportController : BaseController
    {
        // GET: ProfitabilityReport
        public ActionResult Index()
        {
            ProfitabilityReportRepository repo = new ProfitabilityReportRepository();
            return View(repo.GetProfitabilityReport());
        }
    }
}