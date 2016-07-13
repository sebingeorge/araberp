using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class BayController : BaseController
    {
        // GET: Bay
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FillBayList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new BayRepository();
            var List = rep.GetBays();
            return PartialView("BayListView", List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Bay Bay = new Bay();
            Bay.BayRefNo = new BayRepository().GetRefNo(Bay);
            return View(Bay);
        }
        [HttpPost]
        public ActionResult Create(Bay model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new BayRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Bay", "BayName", model.BayName, null, null);
            if (!isexists)
            {
                var result = new BayRepository().InsertBay(model);
                if (result.BayId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["BayRefNo"] = result.BayRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BayRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["BayRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new BayRepository().InsertBay(model);

        //    if (result.BayId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["BayRefNo"] = result.BayRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["BayRefNo"] = null;
        //        return View("Create", model);
        //    }
        //}

 
        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Bay objBay = new BayRepository().GetBay(Id);
            return View("Create", objBay);
        }

        [HttpPost]
        public ActionResult Edit(Bay model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new BayRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Bay", "BayName", model.BayName,"BayId", model.BayId);
            if (!isexists)
            {
                var result = new BayRepository().UpdateBay(model);
                if (result.BayId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["BayRefNo"] = result.BayRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BayRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["BayRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new BayRepository().UpdateBay(model);

        //    if (result.BayId > 0)
        //    {
        //        TempData["Success"] = "Updated Successfully!";
        //        TempData["BayRefNo"] = result.BayRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["BayRefNo"] = null;
        //        return View("Edit", model);
        //    }

        //}

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            Bay objBay = new BayRepository().GetBay(Id);
            return View("Create", objBay);

        }

        [HttpPost]
        public ActionResult Delete(Bay model)
        {
            int result = new BayRepository().DeleteBay(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["BayRefNo"] = model.BayRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Bay It Is Already In Use";
                    TempData["BayRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BayRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

    }
}