using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class WorkShopRequestController : Controller
    {
        // GET: WorkShopRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateWorkShopRequest()
        {
            WorkShopRequest objWorkShopRequest = new WorkShopRequest();

            objWorkShopRequest.WorkShopRequestDate = System.DateTime.Today;
            objWorkShopRequest.Items = new List<WorkShopRequestItem>();
            objWorkShopRequest.Items.Add(new WorkShopRequestItem());
            return View(objWorkShopRequest);
        }
        public ActionResult WorkShopRequestPending()
        {

            var rep = new SaleOrderRepository();

            List<SaleOrder> GetSaleOrders = rep.GetSaleOrders();

            
            return View(GetSaleOrders);
        }

       
    }
}