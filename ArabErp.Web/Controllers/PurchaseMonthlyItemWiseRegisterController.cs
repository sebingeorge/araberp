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

        public ActionResult ProductWiseSalesRegister(int Id = 0)
        {

            return PartialView("_ProductWiseSalesRegister", new SalesRegisterRepository().GetProductWiseSalesRegister(OrganizationId, Id));
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }

    }
}