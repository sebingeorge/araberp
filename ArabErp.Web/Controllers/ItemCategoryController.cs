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
            return View();
        }
      
        public ActionResult Save(ItemCategory objItemCategory)
        {
            var repo = new ItemCategoryRepository();
            new ItemCategoryRepository().InsertItemCategory(objItemCategory);
            return View("Create");
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