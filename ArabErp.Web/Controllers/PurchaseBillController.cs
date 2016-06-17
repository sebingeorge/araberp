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

        public ActionResult Create(IList<PendingGRN> PendingGRNSelected)

        {

            PurchaseBill purchasebill = new PurchaseBill();
            PurchaseBillRepository rep = new PurchaseBillRepository();
            if (PendingGRNSelected != null)
            {
                if (PendingGRNSelected.Count > 0)
                {
                    List<int> selectedgrn = (from PendingGRN p in PendingGRNSelected
                                                          where p.Select
                                                          select p.GRNId).ToList<int>();
                    purchasebill.Items = rep.GetGRNItems(selectedgrn);
                }


            }
            purchasebill.Supplier = PendingGRNSelected[0].SupplierName;
            purchasebill.SupplierId = PendingGRNSelected[0].SupplierId;
            purchasebill.PurchaseBillDate = System.DateTime.Today;
            return View(purchasebill);

        }
        public ActionResult Save(PurchaseBill model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new PurchaseBillRepository().InsertPurchaseBill(model);
            return RedirectToAction("Index");
           

        }

    }
}