using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class PendingTasksForCompletionController : BaseController
    {
        // GET: PendingTasksForCompletion
        public ActionResult Index()
        {
            IEnumerable list = new PendingTasksForCompletionRepository().GetPendingTasks(OrganizationId: OrganizationId);
            return View(list);
        }

        public ActionResult PendingTasks(string saleorder = "", string jobcard = "", string jobcarddate = "",
                                         string engineer = "", string task = "", string technician = "")
        {
            var list = new PendingTasksForCompletionRepository().GetPendingTasks(
                OrganizationId: OrganizationId,
                saleorder: saleorder,
                jobcard: jobcard,
                jobcarddate: jobcarddate,
                technician: technician,
                task: task,
                engineer: engineer);
            return PartialView("_PendingTasksGrid", list);
        }
    }
}