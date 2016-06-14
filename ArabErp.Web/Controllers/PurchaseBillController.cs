using ArabErp.DAL;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;

namespace ArabErp.Web.Controllers
{
    public class PurchaseBillController : Controller
    {
        // GET: PurchaseBill
        public ActionResult Index()
        {
            SupplierDropdown();

            var repo = new PurchaseBillRepository();
            IEnumerable<PendingGRN> pendingGRN = repo.GetGRNPending();
            return View(pendingGRN);
        }
        public void SupplierDropdown()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().SupplierDropdown(), "Id", "Name");
        }
        //public ActionResult PendingGRN(int supplierId)
        //{
        //    var repo = new PurchaseBillRepository();
        //    IEnumerable<PendingGRN> pendingGRNwithid = repo.GetGRNPendingwithfilter(supplierId);
        //    return View(pendingGRNwithid);
        //}
        //public ActionResult Index(int? page)
        //{

        //    var rep = new SaleOrderRepository();


        //    var slist = rep.GetSaleOrdersPendingWorkshopRequest();

        //    var pager = new Pager(slist.Count(), page);

        //    var viewModel = new PagedSaleOrderViewModel
        //    {
        //        SaleOrders = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
        //        Pager = pager
        //    };

        //    return View(viewModel);
        //}
    }
}