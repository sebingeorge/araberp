using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;


namespace ArabErp.Web.Controllers
{
    public class PendingPARregisterController : BaseController
    {
        // GET: PendingPARregister
        public ActionResult Index()
        {
            return View(new PurchaseRequestRepository().GetPendingPARregisterData(OrganizationId));
            //return View();
        }
    }
}