using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CustomerCategoryController : BaseController
    {
        // GET: CustomerCategory
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FillCustomerCategoryList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CustomerCategoryRepository();
            var List = repo.FillCustomerCategoryList();
            return PartialView("CustomerCategoryListView", List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            CustomerCategory CustomerCategory = new CustomerCategory();
            CustomerCategory.CusCategoryRefNo = new CustomerCategoryRepository().GetRefNo(CustomerCategory);
            return View(CustomerCategory);
        }
        [HttpPost]
        public ActionResult Create(CustomerCategory model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CustomerCategoryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "CustomerCategory", "CusCategoryName", model.CusCategoryName, null, null);
            if (!isexists)
            {
                var result = new CustomerCategoryRepository().InsertCustomerCategory(model);
                if (result.CusCategoryId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["CusCategoryRefNo"] = result.CusCategoryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CusCategoryRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CusCategoryRefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            CustomerCategory objCustomerCategory = new CustomerCategoryRepository().GetCustomerCategory(Id);
            return View("Create", objCustomerCategory);
        }

        [HttpPost]
        public ActionResult Edit(CustomerCategory model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CustomerCategoryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "CustomerCategory", "CusCategoryName", model.CusCategoryName, "CusCategoryId", model.CusCategoryId);
            if (!isexists)
            {
                var result = new CustomerCategoryRepository().UpdateCustomerCategory(model);
                if (result.CusCategoryId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CusCategoryRefNo"] = result.CusCategoryRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CusCategoryRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["CusCategoryRefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            CustomerCategory objCustomerCategory = new CustomerCategoryRepository().GetCustomerCategory(Id);
            return View("Create", objCustomerCategory);

        }

        [HttpPost]
        public ActionResult Delete(CustomerCategory model)
        {
            int result = new CustomerCategoryRepository().DeleteCustomerCategory(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CusCategoryRefNo"] = model.CusCategoryRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Customer Category It Is Already In Use";
                    TempData["CusCategoryRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CusCategoryRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

    }
}