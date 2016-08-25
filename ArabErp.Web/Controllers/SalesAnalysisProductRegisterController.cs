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
    public class SalesAnalysisProductRegisterController : BaseController
    {
        // GET: SalesAnalysisProductRegister
        public ActionResult Index()
        {
            ViewBag.startdate = FYStartdate;
            return View();
        }
        public ActionResult SalesAnalysisProduct(DateTime? from, DateTime? to)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SalesAnalysisProduct", new SalesRegisterRepository().GetSalesAnalysisProductWise(from, to, OrganizationId));
        }
      
    }
}