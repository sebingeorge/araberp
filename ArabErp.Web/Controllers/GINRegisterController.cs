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
    public class GINRegisterController : BaseController
    {
        // GET: GRNRegister
        public ActionResult Index()
        {
            FillItemsinWR();
            return View();
        }
        public void FillItemsinWR()
        {
            ViewBag.ItmList = new SelectList(new DropdownRepository().WRItemDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult GINRegisterReport( int itmid=0)
        {
           
            return PartialView("_GINRegister", new SalesRegisterRepository().GetGINRegisterData( itmid, OrganizationId));
        }

    }
}