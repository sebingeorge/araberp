using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class FreezerUnitController : BaseController
    {
        // GET: FreezerUnit
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FillFreezerUnit(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new FreezerUnitRepository();
            var List = rep.FillFreezerUnit();
            return PartialView("FreezerUnitListView", List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            FreezerUnit FreezerUnit = new FreezerUnit();
            FreezerUnit.FreezerUnitRefNo = new FreezerUnitRepository().GetRefNo(FreezerUnit);
            return View(FreezerUnit);
        }
        [HttpPost]
        public ActionResult Create(FreezerUnit model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();


            var repo = new FreezerUnitRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "FreezerUnit", "FreezerUnitName", model.FreezerUnitName, null, null);
            if (!isexists)
            {
                var result = new FreezerUnitRepository().InsertFreezerUnit(model);
                if (result.FreezerUnitId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["FreezerUnitRefNo"] = result.FreezerUnitRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["FreezerUnitRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["FreezerUnitRefNo"] = null;
                return View("Create", model);
            }

        }


        
        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            FreezerUnit objFreezerUnit = new FreezerUnitRepository().GetFreezerUnit(Id);
            return View("Create", objFreezerUnit);
        }

        [HttpPost]
        public ActionResult Edit(FreezerUnit model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();


            var repo = new FreezerUnitRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "FreezerUnit", "FreezerUnitName", model.FreezerUnitName, "FreezerUnitId", model.FreezerUnitId);
            if (!isexists)
            {
                var result = new FreezerUnitRepository().UpdateFreezerUnit(model);
                if (result.FreezerUnitId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["FreezerUnitRefNo"] = result.FreezerUnitRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["FreezerUnitRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["FreezerUnitRefNo"] = null;
                return View("Create", model);
            }

        }
      
        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            FreezerUnit objFreezerUnit = new FreezerUnitRepository().GetFreezerUnit(Id);
            return View("Create", objFreezerUnit);

        }

        [HttpPost]
        public ActionResult Delete(FreezerUnit model)
        {
            int result = new FreezerUnitRepository().DeleteFreezerUnit(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["FreezerUnitRefNo"] = model.FreezerUnitRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Freezer Unit. It Is Already In Use";
                    TempData["FreezerUnitRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["FreezerUnitRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}