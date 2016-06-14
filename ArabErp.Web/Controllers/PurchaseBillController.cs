using ArabErp.DAL;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class PurchaseBillController : Controller
    {
        // GET: PurchaseBill
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PurchaseBillPendingList(int? page)
        {

            var rep = new PurchaseBillRepository();


            var slist = rep.PurchaseBillPendingList();

            var pager = new Pager(slist.Count(), page);

            var viewModel = new PagedPendingGRNViewModel
            {
                GRNs = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };

            return View(viewModel);
        }
    }
}