using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class BayStatusController : BaseController
    {
        // GET: BayStatus
        public ActionResult Index()
        {
            return View((new BayStatusReportRepository()).GetBayStatusReport());
        }
    }
}