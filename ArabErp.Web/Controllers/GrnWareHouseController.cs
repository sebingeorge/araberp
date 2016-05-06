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
        public ActionResult CreateGrnWareHouse()
        {
            return View();
        }
        public ActionResult PendingGrnWareHouse()
        {
            return View();
        }
    }
}