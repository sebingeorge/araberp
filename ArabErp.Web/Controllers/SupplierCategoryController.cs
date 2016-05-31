using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SupplierCategoryController : Controller
    {
        // GET: SupplierCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        { 
            return View();
        }
        public ActionResult Save(SupplierCategory objSupplierCategory)
        {
            var repo = new SupplierCategoryRepository();
            new SupplierCategoryRepository().InsertSupplierCategory(objSupplierCategory);
            return View("Create");
        }
        public ActionResult FillSupplierCategoryList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new SupplierCategoryRepository();
            var List = repo.FillSupplierCategoryList();
            return PartialView("SupplierCategoryListView", List);
        }
    }
}