using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;

namespace ArabErp.Web.Controllers
{
    public class PurchaseRequestController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET: PurchaseRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult PendingWorkShopRequest()
        {
            var repo = new WorkShopRequestRepository();
            var list = repo.GetWorkShopRequestPending();

            //logger.Error("Happy Logging");

            return View(list);
        }
    }
}