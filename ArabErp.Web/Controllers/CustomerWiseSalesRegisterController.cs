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
    public class CustomerWiseSalesRegisterController : BaseController
    {
        // GET: CustomerWiseSalesRegister
        public ActionResult Index()
        {
            FillWorkDesc();
            return View();
        }
        public void FillWorkDesc()
        {
            ViewBag.ItmList = new SelectList(new DropdownRepository().WorkDescDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult CustomerWiseSalesRegister(int id = 0)
        {
            return PartialView("_CustomerWiseSalesRegister", new SalesRegisterRepository().GetCustomerWiseSalesRegister(OrganizationId,id));
        }
    }
}