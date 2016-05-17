using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CustomerCategoryController : Controller
    {
        // GET: CustomerCategory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        {
            return View();
        }
        public ActionResult Save(CustomerCategory objCustomerCategory)
        {
            var repo = new CustomerCategoryRepository();
            new CustomerCategoryRepository().InsertCustomerCategory(objCustomerCategory);
            return View("Create");
        }
    }
}