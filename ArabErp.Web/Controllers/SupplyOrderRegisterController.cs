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
    public class SupplyOrderRegisterController : BaseController
    {
        // GET: SupplyOrderRegister
        public ActionResult Index()
        {
            FillSupplier();
            FillItem();
            return View();
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");

        }
        public void FillItem()
        {
            DropdownRepository repo=new DropdownRepository();
            var result = repo.SOItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public ActionResult SupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-7);
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderRegister", new SupplyOrderRegisterRepository().GetSupplyOrderRegisterData(from, to, id, itmid, OrganizationId));
        }


        public ActionResult SOCount(IList<SupplyOrderRegister> PendingPurchaseRequestItemsSelected)
        {
            SupplyOrderRegister SupplyOrderRegister = new SupplyOrderRegister();

         
            SupplyOrderRepository rep = new SupplyOrderRepository();
            if (PendingPurchaseRequestItemsSelected != null)
            {
                if (PendingPurchaseRequestItemsSelected.Count > 0)
                {
                    List<int> selectedpurchaserequests = (from PendingPurchaseRequest p in PendingPurchaseRequestItemsSelected
                                                          where p.Select
                                                          select p.PurchaseRequestId).ToList<int>();
                    //SupplyOrderRegister.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests);
                }
            }

            return View(SupplyOrderRegister);
        }

    }
}