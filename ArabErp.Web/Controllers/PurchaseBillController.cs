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
            GrnSupplierDropdown();
            return View();
            
        }
        public ActionResult pendingGRN(int supplierId)
        {

            if (supplierId == 0)
            {
                List<PendingGRN> list = new List<PendingGRN>();
                return PartialView("_pendingGRN", list);
            }
            return PartialView("_pendingGRN", new PurchaseBillRepository().GetGRNPending(supplierId));

            
        }
        public void GrnSupplierDropdown()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().GrnSupplierDropdown(), "Id", "Name");
        }

        public ActionResult Create()
        {
            PurchaseBill purchasebill = new PurchaseBill();
            purchasebill.PurchaseBillDate = System.DateTime.Today;
            return View(purchasebill);

        }
    }
}