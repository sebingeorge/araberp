using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SaleOrderStatusController : Controller
    {
        // GET: SaleOrderStatus
        public ActionResult Index()
        {
            SaleOrderStatusRepository repo = new SaleOrderStatusRepository();
            return View(repo.GetSaleOrderStatus());
        }
    }
}