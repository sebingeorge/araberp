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

        [HttpPost]
        public ActionResult CreateWorkShopRequest(SaleOrder model)
        {
            WorkShopRequest objWorkShopRequest = new WorkShopRequest();
            objWorkShopRequest.CustomerOrderRef = model.CustomerOrderRef;
            objWorkShopRequest.SaleOrderId = model.SaleOrderId;
            objWorkShopRequest.SaleOrderRefNo = model.SaleOrderRefNo;
            objWorkShopRequest.CustomerOrderRef = model.CustomerName;




            objWorkShopRequest.WorkShopRequestDate = System.DateTime.Today;
            objWorkShopRequest.Items = new List<WorkShopRequestItem>();
            objWorkShopRequest.Items.Add(new WorkShopRequestItem());
            return View(objWorkShopRequest);
        }
        public ActionResult WorkShopRequestPending()
        {

            var rep = new SaleOrderRepository();

          
            var slist = rep.GetSaleOrders();

            
            return View(slist);
        }

       
    }
}