﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class BoxController : Controller
    {
        // GET: Box
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Box Box = new Box();
            Box.BoxRefNo = new BoxRepository().GetRefNo(Box);
            return View(Box);
        }
        [HttpPost]
        public ActionResult Create(Box model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var result = new BoxRepository().InsertBox(model);

            if (result.BoxId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["BoxRefNo"] = result.BoxRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["BoxRefNo"] = null;
                return View("Create", model);
            }
        }

        public ActionResult FillBoxList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new BoxRepository();
            var List = rep.FillBox();
            return PartialView("BoxListView", List);
        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Box objBox = new BoxRepository().GetBox(Id);
            return View("Create", objBox);
        }

        [HttpPost]
        public ActionResult Edit(Box model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


            var result = new BoxRepository().UpdateBox(model);

            if (result.BoxId > 0)
            {
                TempData["Success"] = "Updated Successfully!";
                TempData["BoxRefNo"] = result.BoxRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["BoxRefNo"] = null;
                return View("Edit", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            Box objBox = new BoxRepository().GetBox(Id);
            return View("Create", objBox);

        }

        [HttpPost]
        public ActionResult Delete(Box model)
        {
            int result = new BoxRepository().DeleteBox(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["BoxRefNo"] = model.BoxRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Box It Is Already In Use";
                    TempData["BoxRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BoxRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}