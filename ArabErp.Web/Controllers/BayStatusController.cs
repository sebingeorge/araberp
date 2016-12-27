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
        public ActionResult Index(string type="all")//type=service means service bay only
        {
            return View();
        }

        public ActionResult BayStatusGrid(string type = "all")
        {
            var list = new BayStatusReportRepository().GetBayStatusReport(type, OrganizationId);
            //foreach(var item in list)
            //{
            //    if (item.JobCardId == null) continue;
            //    item.Tasks = new BayStatusReportRepository().GetJobCardDetails(item.JobCardId);
            //}
            return PartialView("_BayStatusGrid", list);
        }
    }
}