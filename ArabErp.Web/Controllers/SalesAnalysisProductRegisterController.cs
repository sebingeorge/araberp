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
            return View();
        }
        public ActionResult SalesAnalysisProduct(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_SalesAnalysisProduct", new SalesRegisterRepository().GetSalesAnalysisProductWise(from, to, id, OrganizationId));
        }
      
    }
}