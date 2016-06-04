using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SaleOrderController : Controller
    {
        // GET: SaleOrder
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Create()
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();
            SaleOrder saleOrder = new SaleOrder();

            saleOrder.SaleOrderDate = System.DateTime.Today;
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            return View(saleOrder);
        }
        public ActionResult DisplaySOList()
        {
            FillCurrency();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            SaleOrder saleOrder = new SaleOrder();
         
            saleOrder.Items = new List<SaleOrderItem>();
            var item = new SaleOrderItem();
           
            saleOrder.Items.Add(item);
            return PartialView("_DisplaySOList", saleOrder);
        }
        public void FillWrkDesc()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDesc();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillCustomer()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
        public void FillVehicle()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillVehicle();
            ViewBag.vehiclelist = new SelectList(list, "Id", "Name");
        }
        public void FillCommissionAgent()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCommissionAgent();
            ViewBag.commissionagentlist = new SelectList(list, "Id", "Name");
        }
        public void FillEmployee()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillEmployee();
            ViewBag.employeelist = new SelectList(list, "Id", "Name");
        }
        public void FillUnit()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillUnit();
            ViewBag.unitlist = new SelectList(list, "Id", "Name");
        }
        public void FillCurrency()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }
        [HttpPost]
        public ActionResult Save(SaleOrder model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new SaleOrderRepository().InsertSaleOrder(model);
            FillWrkDesc();
            FillUnit();
            FillCustomer();
            FillVehicle();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();
            //TempData["Success"] = "Added Successfully!";
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.SaleOrderDate = System.DateTime.Today;
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            return View("Create", saleOrder);
        }
        public JsonResult GetCustomerDetailsByKey(string cusKey)
        {
            int res = (new SaleOrderRepository()).GetCurrencyIdByCustKey(cusKey);
            string address = (new SaleOrderRepository()).GetCusomerAddressByKey(cusKey);
            return Json(new { Success = true, Result = res, Address = address }, JsonRequestBehavior.AllowGet);
        }

    }
}