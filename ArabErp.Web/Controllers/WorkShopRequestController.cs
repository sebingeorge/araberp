using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
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

        [HttpGet]
        public ActionResult CreateWorkShopRequest(int SaleOrderId)
        {
            SaleOrderRepository rep = new SaleOrderRepository();

            SaleOrder model = rep.GetSaleOrderForWorkshopRequest(SaleOrderId);

            WorkShopRequest objWorkShopRequest = new WorkShopRequest();
            objWorkShopRequest.CustomerOrderRef = model.CustomerOrderRef;
            objWorkShopRequest.SaleOrderId = model.SaleOrderId;
            objWorkShopRequest.SoNoWithDate = model.SoNoWithDate;
            objWorkShopRequest.CustomerOrderRef = model.CustomerOrderRef;
            objWorkShopRequest.CustomerName= model.CustomerName;
            objWorkShopRequest.CustomerId = model.CustomerId;
            objWorkShopRequest.WorkDescription = rep.GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(SaleOrderId).WorkDescription;
            objWorkShopRequest.WorkShopRequestDate = System.DateTime.Today;
            objWorkShopRequest.Items = new List<WorkShopRequestItem>();
            objWorkShopRequest.Items.Add(new WorkShopRequestItem());
            return View(objWorkShopRequest);
        }
        public ActionResult WorkShopRequestPending(int? page)
        {

            var rep = new SaleOrderRepository();


            var slist = rep.GetSaleOrdersPendingWorkshopRequest();

            var pager = new Pager(slist.Count(), page);

            var viewModel = new PagedSaleOrderViewModel
            {
                SaleOrders = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };

            return View(viewModel);
        }

      

               public ActionResult WorkShopRequestData(SaleOrder model)
        {

            var repo = new WorkShopRequestRepository();
            var WSList = repo.GetWorkShopRequestData(model.SaleOrderId);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in WSList)
            {
                model.Items.Add(new SaleOrderItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName,ItemId=item.ItemId });

            }
            return PartialView("_DisplayWorkShopRequestData", model);
        }
               [HttpPost]
               public ActionResult Save(WorkShopRequest model)
               {

                   model.OrganizationId = 1;
                   model.CreatedDate = System.DateTime.Now;
                   model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                   new WorkShopRequestRepository().InsertWorkShopRequest(model);
                  return RedirectToAction("WorkShopRequestPending");
               }
    }

}