using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using System.Collections;

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
            FillBayType();
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
                    FillBayType();
                    TempData["Success"] = "Added Successfully!";
                    TempData["BayRefNo"] = result.BayRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillBayType();
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
 
        public ActionResult Edit(int Id)
        {
            FillBayType();
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
                    FillBayType();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BayRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                FillBayType();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["BayRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            FillBayType();
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
                    FillBayType();
                    TempData["error"] = "Sorry!! You Cannot Delete This Bay It Is Already In Use";
                    TempData["BayRefNo"] = null;
                }
                else
                {
                    FillBayType();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["BayRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

        public void FillBayType()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 0, Name = "Transport" });
            types.Add(new Dropdown { Id = 1, Name = "Bus" });
            ViewBag.baytype = new SelectList(types, "Id", "Name");
        }
       
    }
}