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
    }
}