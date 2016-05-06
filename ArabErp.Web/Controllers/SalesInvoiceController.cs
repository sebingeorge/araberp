using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SalesInvoiceController : Controller
    {
        // GET: SalesInvoice
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateSalesInvoice()
        {
            return View();
        }
        public ActionResult PendingSalesInvoice()
        {
            return View();
        }
    }
}