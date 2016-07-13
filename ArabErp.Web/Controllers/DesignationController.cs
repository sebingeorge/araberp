using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class DesignationController : BaseController
    {
        // GET: Designation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FillDesignationList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new DesignationRepository();
            var List = repo.FillDesignationList();
            return PartialView("DesignationListView",List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Designation Designation = new Designation();
            Designation.DesignationRefNo = new DesignationRepository().GetRefNo(Designation);
            return View(Designation);
        }
        [HttpPost]
        public ActionResult Create(Designation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new DesignationRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Designation", "DesignationName", model.DesignationName, null, null);
            if (!isexists)
            {
                var result = new DesignationRepository().InsertDesignation(model);
                if (result.DesignationId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["DesignationRefNo"] = result.DesignationRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["DesignationRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["DesignationRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new DesignationRepository().InsertDesignation(model);

        //    if (result.DesignationId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["DesignationRefNo"] = result.DesignationRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["DesignationRefNo"] = null;
        //        return View("Create", model);
        //    }
        //}

        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Designation objDesignation = new DesignationRepository().GetDesignation(Id);
            return View("Create", objDesignation);
        }

        [HttpPost]
        public ActionResult Edit(Designation model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


            var repo = new DesignationRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Designation", "DesignationName", model.DesignationName, "DesignationId", model.DesignationId);
            if (!isexists)
            {
                var result = new DesignationRepository().UpdateDesignation(model);
                if (result.DesignationId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["DesignationRefNo"] = result.DesignationRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["DesignationRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["DesignationRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new DesignationRepository().UpdateDesignation(model);

        //    if (result.DesignationId > 0)
        //    {
        //        TempData["Success"] = "Updated Successfully!";
        //        TempData["DesignationRefNo"] = result.DesignationRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["DesignationRefNo"] = null;
        //        return View("Edit", model);
        //    }

        //}

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            Designation objDesignation = new DesignationRepository().GetDesignation(Id);
            return View("Create", objDesignation);

        }

        [HttpPost]
        public ActionResult Delete(Designation model)
        {
            int result = new DesignationRepository().DeleteDesignation(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["DesignationRefNo"] = model.DesignationRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Designation It Is Already In Use";
                    TempData["DesignationRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["DesignationRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}