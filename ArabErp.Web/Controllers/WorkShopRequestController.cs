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

        [HttpPost]
        public ActionResult CreateWorkShopRequest(SaleOrder model)
        {
            WorkShopRequest objWorkShopRequest = new WorkShopRequest();
            objWorkShopRequest.CustomerOrderRef = model.CustomerOrderRef;
            objWorkShopRequest.SaleOrderId = model.SaleOrderId;
            objWorkShopRequest.SaleOrderRefNo = model.SaleOrderRefNo;
            objWorkShopRequest.CustomerOrderRef = model.CustomerName;
            objWorkShopRequest.CustomerId = model.CustomerId;


            FillUnit();
            FillItem();


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

        public ActionResult Save(WorkShopRequest model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new WorkShopRequestRepository().InsertWorkShopRequest(model);


           return RedirectToAction("WorkShopRequestPending");
        }

        public void FillUnit()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillUnit();
            ViewBag.unitlist = new SelectList(List, "Id", "Name");
        }
        public void FillItem()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItem();
            ViewBag.ItemList = new SelectList(List, "Id", "Name");
        }
    }



}