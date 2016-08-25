using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;

namespace ArabErp.Web.Controllers
{
    public class PurchaseMonthlyItemWiseRegisterController : BaseController
    {
        // GET: PurchaseMonthlyItemWiseRegister
        public ActionResult Index()
        {
            FillSupplier();
            return View();
        }

        public ActionResult PurchaseMonthlyItemWiseRegister(int Id = 0)
        {

            return PartialView("_PurchaseMonthlyItemWiseRegister", new PurchaseBillRegisterRepository().GetPurchaseMonthlyItemWiseData(OrganizationId, Id));
        }

        public ActionResult PurchaseMonthlySupplierWise()
        {
            FillItem();
            return View();
        }

        public ActionResult PurchaseMonthlySupplierWiseRegister(int Id = 0)
        {

            return PartialView("_PurchaseMonthlySupplierWiseRegister", new PurchaseBillRegisterRepository().GetPurchaseMonthlySupplieriseData(OrganizationId, Id));
        }

        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PurchaseBillSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PBItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }


    }
}