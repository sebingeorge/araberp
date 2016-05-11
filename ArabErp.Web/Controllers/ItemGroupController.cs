using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ItemGroupController : Controller
    {
        // GET: ItemGroup
        public ActionResult Index()
        {
            return View();
        }
         public ActionResult Create()
        {
            return View();
        }
    }
}