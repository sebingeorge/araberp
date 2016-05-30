using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class UnitController : Controller
    {
        // GET: Unit
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Save(Unit objUnit)
        {
            var repo = new UnitRepository();
            new UnitRepository().InsertUnit(objUnit);
            return View("Create");
        }
        public ActionResult FillUnitList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new UnitRepository();
            var List = rep.FillUnitList();
            return PartialView("UnitListView", List);
        }
    }
}