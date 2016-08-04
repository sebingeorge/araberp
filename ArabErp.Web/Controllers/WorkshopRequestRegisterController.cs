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
    public class WorkshopRequestRegisterController : BaseController
    {
        // GET: WorkshopRequestRegister
        public ActionResult Index()
        {
            FillWRNo();
            FillItemsinWR();
            return View();
        }
        public void FillWRNo()
        {
            ViewBag.WRNoList = new SelectList(new DropdownRepository().WRNODropdown(OrganizationId), "Id", "Name");
        }
        public void FillItemsinWR()
        {
            ViewBag.ItmList = new SelectList(new DropdownRepository().WRItemDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult WorkShopRequestRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            return PartialView("_WorkShopRequestRegister", new WorkshopRequestRegisterRepository().GetWorkshopRegisterData(from, to, id, itmid, OrganizationId));
        }
    }
}