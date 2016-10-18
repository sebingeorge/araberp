using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class EmployeeLocationController : BaseController
    {
        // GET: EmployeeLocation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult EmployeeLocationList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new EmployeeLocationRepository();
            var List = repo.FillEmployeeLocationList();
            return PartialView("EmployeeLocationListView", List);
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            EmployeeLocation Employee = new EmployeeLocation();
            Employee.LocationRefNo = new EmployeeLocationRepository().GetRefNo(Employee);
            return View(Employee);
        }
        [HttpPost]
        public ActionResult Create(EmployeeLocation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new EmployeeLocationRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "EmployeeLocation", "LocationName", model.LocationName, null, null);
            if (!isexists)
            {
                var result = new EmployeeLocationRepository().InsertEmployeeLocation(model);
                if (result.LocationId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["LocationRefNo"] = result.LocationRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["LocationRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["LocationRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            EmployeeLocation objEmployeeLocation = new EmployeeLocationRepository().GetEmployeeLocation(Id);
            return View("Create", objEmployeeLocation);
        }
        [HttpPost]
        public ActionResult Edit(EmployeeLocation model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();


            var repo = new EmployeeLocationRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "EmployeeLocation", "LocationName", model.LocationName, "LocationId", model.LocationId);
            if (!isexists)
            {
                var result = new EmployeeLocationRepository().UpdateEmployeeLocation(model);
                if (result.LocationId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CountryRefNo"] = result.LocationRefNo;
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
            EmployeeLocation objEmployeeLocation = new EmployeeLocationRepository().GetEmployeeLocation(Id);
            return View("Create", objEmployeeLocation);

        }

        [HttpPost]
        public ActionResult Delete(EmployeeLocation model)
        {
            int result = new EmployeeLocationRepository().DeleteEmployeeLocation(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CountryRefNo"] = model.LocationRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Designation It Is Already In Use";
                    TempData["LocationRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["LocationRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}