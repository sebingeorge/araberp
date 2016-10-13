using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CompanyController : BaseController
    {
        // GET: Company
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            Company Company = new Company();
            Company.cmpUsercode = "C/" + DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Company).Name);
            return View(Company);
        }
        [HttpPost]
        public ActionResult Create(Company model)
        {
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var repo = new CompanyRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "mstAccCompany", "cmpUsercode", model.cmpUsercode, null, null);
            if (!isexists)
            {
                var result = new CompanyRepository().InsertCompany(model);
                if (result.cmpCode > 0)
                {
                    TempData["Success"] = "Added Successfully!";
                    TempData["cmpUsercode"] = result.cmpUsercode;
                    return RedirectToAction("Index");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["cmpUsercode"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["cmpUsercode"] = null;
                return View("Create", model);
            }
        }
        public ActionResult CompanyList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CompanyRepository();
            var List = repo.CompanyList();
            return PartialView("CompanyListView", List);
        }

        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            Company objCompany = new CompanyRepository().GetCompany(Id);
           
            return View("Create", objCompany);
        }
        [HttpPost]
        public ActionResult Edit(Company model)
        {
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var repo = new CompanyRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "mstAccCompany", "cmpUsercode", model.cmpUsercode, "cmpCode", model.cmpCode);
            if (!isexists)
            {
                var result = new CompanyRepository().UpdateCompany(model);
                if (result.cmpCode > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["cmpUsercode"] = result.cmpUsercode;
                    return RedirectToAction("Index");
                }
                else
                {
                   
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["cmpUsercode"] = null;
                    return View("Create", model);
                }
            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["cmpUsercode"] = null;
                return View("Create", model);
            } 
        }
        public ActionResult Delete(int Id)
        {
             ViewBag.Title = "Delete";
             Company objCompany = new CompanyRepository().GetCompany(Id);
             return View("Create", objCompany);
        }

        [HttpPost]

        public ActionResult Delete(Company model)
        {
            int result = new CompanyRepository().DeleteCompany(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["cmpUsercode"] = model.cmpUsercode;
                return RedirectToAction("Index");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Customer. It Is Already In Use";
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";          
                }
                return RedirectToAction("Index");
            }

        }

    
    
    }
}