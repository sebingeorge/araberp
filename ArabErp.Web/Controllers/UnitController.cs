using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class UnitController : BaseController
    {
        // GET: Unit
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            return View();
        }
         [HttpPost]
        public ActionResult Create(Unit model)
        {
            //if (ModelState.IsValid)
            //{
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

                var repo = new UnitRepository();
                new UnitRepository().InsertUnit(model);
                return RedirectToAction("Create");
            //}
            //else
            //{
            //    return View(model);
            //}
        }

        //public ActionResult Save(Unit objUnit)
        //{
        //    var repo = new UnitRepository();
        //    new UnitRepository().InsertUnit(objUnit);
        //    return View("Create");
        //}
        public ActionResult FillUnitList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new UnitRepository();
            var List = rep.FillUnitList();
            return PartialView("UnitListView", List);
        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Unit objUnit = new UnitRepository().GetUnit(Id);
            return View("Create", objUnit);
        }

        [HttpPost]
        public ActionResult Edit(Unit model)
        {

            if (ModelState.IsValid)
            {
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

                var repo = new UnitRepository();
                new UnitRepository().UpdateUnit(model);
                return RedirectToAction("Create");
            }
            else
            {
                return View(model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            Unit objUnit = new UnitRepository().GetUnit(Id);
            return View("Create", objUnit);

        }

        [HttpPost]
        public ActionResult Delete(Unit model)
        {

            if (ModelState.IsValid)
            {
                var repo = new UnitRepository();
                new UnitRepository().DeleteUnit(model);
                return RedirectToAction("Create");
            }
            else
            {
            return View(model);
            }

        }

    }
}