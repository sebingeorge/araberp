using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class EmployeeCategoryController : BaseController
    {
        // GET: EmployeeCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(EmployeeCategory).Name);
            return View(new EmployeeCategory { EmpCategoryRefNo = "EMPC/" + internalid });
        }

        [HttpPost]
        public ActionResult Create(EmployeeCategory model)
        {
            var repo = new EmployeeCategoryRepository();

            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "EmployeeCategory", "EmpCategoryName", model.EmpCategoryName, null, null);
            if (!isexists)
            {
                var result = new EmployeeCategoryRepository().InsertEmployeeCategory(model);
                if (result.EmpCategoryId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["RefNo"] = result.EmpCategoryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult FillEmployeeCategoryList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new EmployeeCategoryRepository();
            var List = repo.FillEmployeeCategoryList();
            return PartialView("EmployeeCategoryListView", List);
        }
        public ActionResult Edit(int Id)
        {
           
            EmployeeCategory model = new EmployeeCategoryRepository().GetEmployeeCategory(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(EmployeeCategory model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new EmployeeCategoryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "EmployeeCategory", "EmpCategoryName", model.EmpCategoryName, " EmpCategoryId",model.EmpCategoryId);
            if (!isexists)
            {
                var result = new EmployeeCategoryRepository().UpdateEmployeeCategory(model);
                if (result.EmpCategoryId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["RefNo"] = result.EmpCategoryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

        
        public ActionResult Delete(int Id)
        {

            EmployeeCategory model = new EmployeeCategoryRepository().GetEmployeeCategory(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Delete(EmployeeCategory model)
        {

            var result = new EmployeeCategoryRepository().DeleteEmployeeCategory(model);


            if (result.EmpCategoryId > 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["RefNo"] = model.EmpCategoryRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }
    }
}