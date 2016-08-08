using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

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
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["OrganizationRefNo"] = null;
                    return View("Edit", model);
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

        public ActionResult Delete(int Id)
        {
            dropdown();
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
        }
        public ActionResult FillOrganizationList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new OrganizationRepository();
            var List = repo.FillOrganizationList();
            return PartialView("OrganizationListView", List);
        }

    }
}