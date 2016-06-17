using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SalesQuotationController : Controller
    {
        // GET: SalesQuotation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            return View(salesquotation);
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

        public void FillSalesQuotationRejectReason()
        {
            var repo = new SalesQuotationRepository();
            var list = repo.FillSalesQuotationRejectReason();
            ViewBag.SalesQuotationRejectReasonlist = new SelectList(list, "Id", "Name");
        }

        [HttpPost]
        public ActionResult Save(SalesQuotation model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new SalesQuotationRepository().InsertSalesQuotation(model);
            return RedirectToAction("Create");
        }
    }
}