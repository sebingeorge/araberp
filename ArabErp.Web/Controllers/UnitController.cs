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
            Unit Unit = new Unit();
            Unit.UnitRefNo = new UnitRepository().GetRefNo(Unit);
            return View(Unit);
        }
        [HttpPost]
        public ActionResult Create(Unit model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new UnitRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Unit", "UnitName", model.UnitName, null, null);
            if (!isexists)
            {
                var result = new UnitRepository().InsertUnit(model);
                if (result.UnitId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["UnitRefNo"] = result.UnitRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["UnitRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["UnitRefNo"] = null;
                return View("Create", model);
            }

        }

    
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

            model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();


            var repo = new UnitRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Unit", "UnitName", model.UnitName, "UnitId", model.UnitId);
            if (!isexists)
            {
                var result = new UnitRepository().UpdateUnit(model);
                if (result.UnitId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["UnitRefNo"] = result.UnitRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["UnitRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["UnitRefNo"] = null;
                return View("Create", model);
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
            int result = new UnitRepository().DeleteUnit(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["UnitRefNo"] = model.UnitRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Unit. It Is Already In Use";
                    TempData["UnitRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["UnitRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

   
    }
}