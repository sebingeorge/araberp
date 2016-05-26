using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class GrnWareHouseController : Controller
    {
        // GET: GrnWareHouse
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult PendingGrnWareHouse()
        {
            return View();
        }
    }
}