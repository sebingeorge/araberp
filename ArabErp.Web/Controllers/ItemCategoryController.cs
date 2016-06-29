using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class ItemCategoryController : BaseController
    {
        // GET: ItemCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            ItemCategory ItemCategory = new ItemCategory();
            ItemCategory.itmCatRefNo= new ItemCategoryRepository().GetRefNo(ItemCategory);
            return View(ItemCategory);
        }

        [HttpPost]
        public ActionResult Create(ItemCategory model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var result = new ItemCategoryRepository().InsertItemCategory(model);

            if (result.itmCatId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["itmCatRefNo"] = result.itmCatRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["itmCatRefNo"] = null;
                return View("Create", model);
            }
        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            ItemCategory objItemCategory = new ItemCategoryRepository().GetItemCategory(Id);
            return View("Create", objItemCategory);
        }

        [HttpPost]
        public ActionResult Edit(ItemCategory model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


            var result = new ItemCategoryRepository().UpdateItemCategory(model);

            if (result.itmCatId > 0)
            {
                TempData["Success"] = "Updated Successfully!";
                TempData["itmCatRefNo"] = result.itmCatRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["itmCatRefNo"] = null;
                return View("Edit", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            ItemCategory objItemCategory = new ItemCategoryRepository().GetItemCategory(Id);
            return View("Create", objItemCategory);

        }

        [HttpPost]
        public ActionResult Delete(ItemCategory model)
        {
            int result = new ItemCategoryRepository().DeleteItemCategory(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["itmCatRefNo"] = model.itmCatRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This ItemCategory. It Is Already In Use";
                    TempData["itmCatRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["itmCatRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

     
        public ActionResult FillItemCategoryList(int? page)
          {
          int itemsPerPage = 10;
          int pageNumber = page ?? 1;
          var repo = new ItemCategoryRepository();
          var List = repo.FillItemCategoryList();
          return PartialView("ItemCategoryListView", List);
          }
       
        

    }
}