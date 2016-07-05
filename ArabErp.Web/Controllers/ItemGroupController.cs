using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ItemGroupController : BaseController
    {
        // GET: ItemGroup
        public ActionResult Index()
        {
            return View();
        }
         public ActionResult Create()
        {
            ViewBag.Title = "Create";
            ItemGroup ItemGroup = new ItemGroup();
            ItemGroup.ItemGroupRefNo = new ItemGroupRepository().GetRefNo(ItemGroup);
            dropdown();
            return View(ItemGroup);
        }
         [HttpPost]
         public ActionResult Create(ItemGroup model)
         {
             model.OrganizationId = 1;
             model.CreatedDate = System.DateTime.Now;
             model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

             var repo = new ItemGroupRepository();
             bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemGroup", "ItemGroupName", model.ItemGroupName, null, null);
             if (!isexists)

             {
                 var result = new ItemGroupRepository().InsertItemGroup(model);
                 if (result.ItemGroupId > 0)
                 {

                     TempData["Success"] = "Added Successfully!";
                     TempData["ItemGroupRefNo"] = result.ItemGroupRefNo;
                     return RedirectToAction("Create");
                 }

                 else
                 {
                     dropdown();
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["ItemGroupRefNo"] = null;
                     return View("Create", model);
                 }

             }
             else
             {

                 dropdown();
                 TempData["error"] = "This Item Group Name Alredy Exists!!";
                 TempData["ItemGroupRefNo"] = null;
                 return View("Create", model);
             }

         }


         public ActionResult Edit(int Id)
         {
             dropdown();
             ViewBag.Title = "Edit";
             ItemGroup objItemGroup = new ItemGroupRepository().GetItemGroup(Id);
             return View("Create", objItemGroup);
         }

         [HttpPost]
         public ActionResult Edit(ItemGroup model)
        {

             model.OrganizationId = 1;
             model.CreatedDate = System.DateTime.Now;
             model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
             var repo = new ItemGroupRepository();


             bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemGroup", "ItemGroupName", model.ItemGroupName, "ItemGroupId", model.ItemGroupId);
             if (!isexists)
             {
                 var result = new ItemGroupRepository().UpdateItemGroup(model);

                 if (result.ItemGroupId > 0)
                 {
                     TempData["Success"] = "Updated Successfully!";
                     TempData["ItemGroupRefNo"] = result.ItemGroupRefNo;
                     return RedirectToAction("Create");
                 }
                 else
                 {
                     dropdown();
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["ItemGroupRefNo"] = null;
                     return View("Edit", model);
                 }
             }
             else
             {
                 dropdown();
                 TempData["error"] = "This Item Name Alredy Exists!!";
                 TempData["ItemGroupRefNo"] = null;
                 return View("Create", model);
             }

         }



         public ActionResult Delete(int Id)
         {
             dropdown();
             ViewBag.Title = "Delete";
             ItemGroup objItemGroup = new ItemGroupRepository().GetItemGroup(Id);
             return View("Create", objItemGroup);

         }

         [HttpPost]
         public ActionResult Delete(ItemGroup model)
         {
             int result = new ItemGroupRepository().DeleteItemGroup(model);

             if (result == 0)
             {
                 TempData["Success"] = "Deleted Successfully!";
                 TempData["ItemGroupRefNo"] = model.ItemGroupRefNo;
                 return RedirectToAction("Create");
             }
             else
             {
                 if (result == 1)
                 {
                     TempData["error"] = "Sorry!! You Cannot Delete This Item Group It Is Already In Use";
                     TempData["ItemGroupRefNo"] = null;
                 }
                 else
                 {
                     TempData["error"] = "Oops!!..Something Went Wrong!!";
                     TempData["ItemGroupRefNo"] = null;
                 }
                 return RedirectToAction("Create");
             }

         }


         public void dropdown()
         {
             var repo = new ItemGroupRepository();
             var List = repo.FillCategory();
             ViewBag.ItemCategory = new SelectList(List, "Id", "Name");
         }
        public ActionResult FillItemGroupList(int?page)
         {
             int itemsPerPage = 10;
             int pageNumber = page ?? 1;
             var repo = new ItemGroupRepository();
             var List = repo.FillItemGroupList();
             return PartialView("ItemGroupListView", List);
         }
    }
}