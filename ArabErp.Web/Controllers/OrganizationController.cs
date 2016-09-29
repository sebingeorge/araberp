using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using System.IO;

namespace ArabErp.Web.Controllers
{
    public class OrganizationController : BaseController
    {
        // GET: Organization
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Organization Organization = new Organization();
            Organization.OrganizationRefNo = "ORG/" + DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Organization).Name);
            dropdown();
            FillCountryDropdown();
            FillCompanyDropdown();
            return View(Organization);
        }


        [HttpPost]
        public ActionResult Create(Organization model)
        {
            model.CreatedBy = UserID.ToString();      
            var repo = new OrganizationRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Organization", "OrganizationName", model.OrganizationName, null, null);
            if (!isexists)
            {
                var result = new OrganizationRepository().InsertOrganization(model);
                if (result.OrganizationId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["OrganizationRefNo"] = result.OrganizationRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    dropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["OrganizationRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                dropdown();
                TempData["error"] = "This Organization Name Alredy Exists!!";
                TempData["OrganizationRefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult Edit(int Id)
        {
            dropdown();
            FillCountryDropdown();
            FillCompanyDropdown();
            ViewBag.Title = "Edit";
            Organization objOrganization = new OrganizationRepository().GetOrganization(Id);
            return View("Create", objOrganization);
        }

        [HttpPost]
        public ActionResult Edit(Organization model)
        {

             var repo = new OrganizationRepository();
            model.CreatedBy = UserID.ToString();

            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Organization", "OrganizationName", model.OrganizationName, "OrganizationId", model.OrganizationId);
            if (!isexists)
            {
                var result = new OrganizationRepository().UpdateOrganization(model);

                if (result.OrganizationId > 0)
                {
                    TempData["Success"] = "Updated Successfully!";
                    TempData["OrganizationRefNo"] = result.OrganizationRefNo;
                    return RedirectToAction("Create");
                }
                else
                {
                    dropdown();
                    FillCountryDropdown();
                    FillCompanyDropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["OrganizationRefNo"] = null;
                    return View("Edit", model);
                }
            }
            else
            {
                dropdown();
                FillCountryDropdown();
                FillCompanyDropdown();
                TempData["error"] = "This Organization Name Alredy Exists!!";
                TempData["OrganizationRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            dropdown();
            FillCountryDropdown();
            FillCompanyDropdown();
            ViewBag.Title = "Delete";
            Organization objOrganization = new OrganizationRepository().GetOrganization(Id);
            return View("Create", objOrganization);

        }

        [HttpPost]
        public ActionResult Delete(Organization model)
        {
            model.CreatedBy = UserID.ToString();
            int result = new OrganizationRepository().DeleteOrganization(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["OrganizationRefNo"] = model.OrganizationRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Organization It Is Already In Use";
                    TempData["OrganizationRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["OrganizationRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

        public void dropdown()
        {
            var repo = new OrganizationRepository();
            var List = repo.FillCurrency();
            ViewBag.Currency = new SelectList(List, "Id", "Name");

            var fyRepo = new FinancialYearRepository();
            var financialYear = fyRepo.GetFinancialYear();
            ViewBag.FinancialYear = new SelectList(financialYear, "FyId", "FyName");
        }
        public ActionResult FillOrganizationList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new OrganizationRepository();
            var List = repo.FillOrganizationList();
            return PartialView("OrganizationListView", List);
        }

        public void FillCountryDropdown()
        {
            OrganizationRepository rep = new OrganizationRepository();
            var cus = rep.FillCountryDropdown();
            ViewBag.CustomerCountry = new SelectList(cus, "Id", "Name");
        }
        public void FillCompanyDropdown()
        {
            OrganizationRepository rep = new OrganizationRepository();
            var cum = rep.FillCompanyDropdown();
            ViewBag.CustomerCompany = new SelectList(cum, "Id", "Name");
        }
        public ActionResult SaveUploadFiles(HttpPostedFileBase file)
        {
            string fileName = SaveUploadImage(file);
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        private string SaveUploadImage(HttpPostedFileBase file)
        {
            string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string qualifiedName = Server.MapPath("~/App_Images/") + uniqueName;
            file.SaveAs(qualifiedName);
            return uniqueName;
        }





    }
}