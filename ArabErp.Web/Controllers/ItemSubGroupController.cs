using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ItemSubGroupController : BaseController
    {
        // GET: ItemSubGroup
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillItemGroup();
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(ItemSubGroup).Name);
            return View(new ItemSubGroup { ItemSubGroupRefNo = "ISG/" + internalid });
        }
        [HttpPost]
        public ActionResult Create(ItemSubGroup model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new ItemSubGroupRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemSubGroup", "ItemSubGroupName", model.ItemSubGroupName, null, null);
            if (!isexists)
            {
                var result = new ItemSubGroupRepository().InsertItemSubGroup(model);
                if (result.ItemSubGroupId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["ItemSubGroupRefNo"] = result.ItemSubGroupRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillItemGroup();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["ItemSubGroupRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillItemGroup();
                TempData["error"] = "This Item SubGroup Name Alredy Exists!!";
                TempData["ItemSubGroupRefNo"] = null;
                return View("Create", model);
            }

        }

   
        public ActionResult Edit(int Id)
        {
            FillItemGroup();
            ItemSubGroup model = new ItemSubGroupRepository().GetItemSubGroup(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(ItemSubGroup model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new ItemSubGroupRepository();


            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "ItemSubGroup", "ItemSubGroupName", model.ItemSubGroupName, "ItemSubGroupId", model.ItemSubGroupId);
            if (!isexists)
            {
                var result = new ItemSubGroupRepository().UpdateItemSubGroup(model);

                if (result.ItemSubGroupId > 0)
                {
                    TempData["Success"] = "Updated Successfully!";
                    TempData["ItemSubGroupRefNo"] = result.ItemSubGroupRefNo;
                    return RedirectToAction("Create");
                }
                else
                {
                    FillItemGroup();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["ItemSubGroupRefNo"] = null;
                    return View("Edit", model);
                }
            }
            else
            {
                FillItemGroup();
                TempData["error"] = "This Item Name Alredy Exists!!";
                TempData["ItemSubGroupRefNo"] = null;
                return View("Create", model);
            }

        }

     
        public ActionResult Delete(int Id)
        {
            FillItemGroup();
            ItemSubGroup model = new ItemSubGroupRepository().GetItemSubGroup(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Delete(ItemSubGroup model)
        {

            var result = new ItemSubGroupRepository().DeleteItemSubGroup(model);


            if (result.ItemSubGroupId > 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["ItemSubGroupRefNo"] = model.ItemSubGroupRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["ItemSubGroupRefNo"] = null;
                return View("Create", model);
            }

        }
        public void FillItemGroup()
        {
            var repo = new ItemSubGroupRepository();
            var List = repo.FillGroup();
            ViewBag.ItemGroup = new SelectList(List, "Id", "Name");
        }

        public ActionResult FillItemSubGroupList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new ItemSubGroupRepository();
            var List = repo.FillItemSubGroupList();
            return PartialView("ItemSubGroupListView", List);
        }

    }
}