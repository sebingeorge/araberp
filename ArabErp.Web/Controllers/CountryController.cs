using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CountryController : BaseController
    {
        // GET: Country
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CountryList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CountryRepository();
            var List = repo.FillCountryList();
            return PartialView("CountryListView", List);
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Country Co = new Country();
            Co.CountryRefNo = new CountryRepository().GetRefNo(Co);
            return View(Co);
        }
        [HttpPost]
        public ActionResult Create(Country model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CountryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Country", "CountryName", model.CountryName, null, null);
            if (!isexists)
            {
                var result = new CountryRepository().InsertCountry(model);
                if (result.CountryId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["CountryRefNo"] = result.CountryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CountryRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CountryRefNo"] = null;
                return View("Create", model);
            }

        }
        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Country objCountry = new CountryRepository().GetCountry(Id);
            return View("Create", objCountry);
        }
        [HttpPost]
        public ActionResult Edit(Country model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();


            var repo = new CountryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Country", "CountryName", model.CountryName, "CountryId", model.CountryId);
            if (!isexists)
            {
                var result = new CountryRepository().UpdateEmployeeLocation(model);
                if (result.CountryId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CountryRefNo"] = result.CountryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CountryRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CountryRefNo"] = null;
                return View("Create", model);
            }

        }
        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            Country objCountry = new CountryRepository().GetEmployeeLocation(Id);
            return View("Create", objCountry);

        }

        [HttpPost]
        public ActionResult Delete(Country model)
        {
            int result = new CountryRepository().DeleteCountry(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CountryRefNo"] = model.CountryRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Designation It Is Already In Use";
                    TempData["CountryRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CountryRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

    }
}