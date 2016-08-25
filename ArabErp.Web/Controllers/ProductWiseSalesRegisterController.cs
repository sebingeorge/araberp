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
    public class ProductWiseSalesRegisterController : BaseController
    {
        // GET: ProductWiseSalesRegister
        public ActionResult Index()
        {
            FillCustomer();
            return View();
        }
        public ActionResult ProductWiseSalesRegister(int Id = 0)
        {

            return PartialView("_ProductWiseSalesRegister", new SalesRegisterRepository().GetProductWiseSalesRegister(OrganizationId, Id));
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
    }
}