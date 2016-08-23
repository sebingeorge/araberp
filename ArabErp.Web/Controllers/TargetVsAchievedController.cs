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
    public class TargetVsAchievedController : BaseController
    {
        // GET: TargetVsAchieved
        public ActionResult Index()
        {
            FillMonth();
            return View();
        }

        public void FillMonth()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.MonthDropdown(OrganizationId);
            ViewBag.MonthList = new SelectList(result, "Id", "Name");
        }
        public ActionResult TargetVsAchievedRegister(int Id=0)
        {

            return PartialView("_TargetVsAchievedRegister", new SalesRegisterRepository().GetTargetVsAchieved(OrganizationId, Id));
        }
    }
}