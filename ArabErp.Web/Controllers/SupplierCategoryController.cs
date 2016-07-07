using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SupplierCategoryController : BaseController
    {
        // GET: SupplierCategory
        public ActionResult Index()
        {
            return View();
        }
         public ActionResult FillSupplierCategoryList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new SupplierCategoryRepository();
            var List = repo.FillSupplierCategoryList();
            return PartialView("SupplierCategoryListView", List);
        }

         public ActionResult Create()
         {
             ViewBag.Title = "Create";
             SupplierCategory SupplierCategory = new SupplierCategory();
             SupplierCategory.SupCategoryRefNo = new SupplierCategoryRepository().GetRefNo(SupplierCategory);
             return View(SupplierCategory);
         }
         [HttpPost]
         public ActionResult Create(SupplierCategory model)
         {
             model.OrganizationId = 1;
             model.CreatedDate = System.DateTime.Now;
             model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

             var repo = new SupplierCategoryRepository();
             bool isexists = repo.IsFieldExists(repo.ConnectionString(), "SupplierCategory", "SupCategoryName", model.SupCategoryName, null, null);
              if (!isexists)

             {
                 var result = new SupplierCategoryRepository().InsertSupplierCategory(model);
                 if (result.SupCategoryId > 0)
                 {

                     TempData["Success"] = "Added Successfully!";
                     TempData["SupCategoryRefNo"] = result.SupCategoryRefNo;
                     return RedirectToAction("Create");
                 }

                 else
                 {
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["SupCategoryRefNo"] = null;
                     return View("Create", model);
                 }

             }
             else
             {
                 TempData["error"] = "This Name Alredy Exists!!";
                 TempData["SupCategoryRefNo"] = null;
                 return View("Create", model);
             }

         }

         public ActionResult Edit(int Id)
         {
             ViewBag.Title = "Edit";
             SupplierCategory objSupplierCategory = new SupplierCategoryRepository().GetSupplierCategory(Id);
             return View("Create", objSupplierCategory);
         }

         [HttpPost]
         public ActionResult Edit(SupplierCategory model)
         {

             model.OrganizationId = 1;
             model.CreatedDate = System.DateTime.Now;
             model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


              var repo = new SupplierCategoryRepository();
              bool isexists = repo.IsFieldExists(repo.ConnectionString(), "SupplierCategory", "SupCategoryName", model.SupCategoryName, "SupCategoryId", model.SupCategoryId);
              if (!isexists)

             {
                 var result = new SupplierCategoryRepository().UpdateSupplierCategory(model);
                 if (result.SupCategoryId > 0)
                 {

                     TempData["Success"] = "Updated Successfully!";
                     TempData["SupCategoryRefNo"] = result.SupCategoryRefNo;
                     return RedirectToAction("Create");
                 }

                 else
                 {
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["SupCategoryRefNo"] = null;
                     return View("Create", model);
                 }

             }
             else
             {
                 TempData["error"] = "This Name Alredy Exists!!";
                 TempData["SupCategoryRefNo"] = null;
                 return View("Create", model);
             }

         }

         public ActionResult Delete(int Id)
         {
             ViewBag.Title = "Delete";
             SupplierCategory objSupplierCategory = new SupplierCategoryRepository().GetSupplierCategory(Id);
             return View("Create", objSupplierCategory);

         }

         [HttpPost]
         public ActionResult Delete(SupplierCategory model)
         {
             int result = new SupplierCategoryRepository().DeleteSupplierCategory(model);

             if (result == 0)
             {
                 TempData["Success"] = "Deleted Successfully!";
                 TempData["SupCategoryRefNo"] = model.SupCategoryRefNo;
                 return RedirectToAction("Create");
             }
             else
             {
                 if (result == 1)
                 {
                     TempData["error"] = "Sorry!! You Cannot Delete This Supplier Category It Is Already In Use";
                     TempData["SupCategoryRefNo"] = null;
                 }
                 else
                 {
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["SupCategoryRefNo"] = null;
                 }
                 return RedirectToAction("Create");
             }

         }


     }
}