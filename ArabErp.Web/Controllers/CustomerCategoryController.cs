using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}