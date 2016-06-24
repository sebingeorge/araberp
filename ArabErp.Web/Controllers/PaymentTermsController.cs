using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class PaymentTermsController : Controller
    {
        // GET: PaymentTerms
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Save(PaymentTerms objPaymentTerms)
        {
            var repo = new PaymentTermsRepository();
            new PaymentTermsRepository().InsertPaymentTerms(objPaymentTerms);
            return View("Create");
        }
        public ActionResult FillPaymentTermsList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new PaymentTermsRepository();
            var List = rep.FillPaymentTermsList();
            return PartialView("PaymentTermsListView", List);
        }

    }
}