using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class ItemCategoryController : Controller
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
    }
}