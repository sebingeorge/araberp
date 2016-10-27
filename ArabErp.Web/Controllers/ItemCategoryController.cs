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
            ItemCategory.CreatedBy = UserID.ToString();
            ItemCategory.itmCatRefNo = new ItemCategoryRepository().GetRefNo(ItemCategory);
            return View(ItemCategory);
        }

        [HttpPost]
        public ActionResult Create(ItemCategory model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new ItemCategoryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemCategory", "CategoryName", model.CategoryName, null, null);
            if (!isexists)
            {
                var result = new ItemCategoryRepository().InsertItemCategory(model);
                if (result.itmCatId > 0)
                {

                    TempData["Success"] = "Saved Successfully! Reference No. is " + result.itmCatRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Some error occurred. Please try again.";
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This material/spare category name already exists!";
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

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new ItemCategoryRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemCategory", "CategoryName", model.CategoryName, "itmCatId", model.itmCatId);
            if (!isexists)
            {
                var result = new ItemCategoryRepository().UpdateItemCategory(model);
                if (result.itmCatId > 0)
                {

                    TempData["Success"] = "Updated Successfully! (" + result.itmCatRefNo + ")";
                    TempData["itmCatRefNo"] = result.itmCatRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Some error occurred. Please try again.";
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This material/spare category name already exists!";
                TempData["itmCatRefNo"] = null;
                return View("Create", model);
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
            var result = new ItemCategoryRepository().DeleteItemCategory(model);

            if (result.itmCatId > 0)
            {
                TempData["Success"] = "Deleted Successfully! (" + result.itmCatRefNo + ")";
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Some error occurred. Please try again.";
                return View("Create", model);
            }

        }


        public ActionResult FillItemCategoryList(int? page, string name = "")
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            return PartialView("ItemCategoryListView", new ItemCategoryRepository().FillItemCategoryList(name));
            //var repo = new ItemCategoryRepository();
            //var List = repo.FillItemCategoryList();
            //return PartialView("ItemCategoryListView", List);
        }



    }
}