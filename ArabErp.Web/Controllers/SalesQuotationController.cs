﻿using ArabErp.DAL;
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
            ViewBag.SubmitAction = "Save";
            return View(salesquotation);
        }

        [HttpGet]
        public ActionResult Approve(int SalesQuotationId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(SalesQuotationId);
            salesquotation.CustomerAddress= sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);


            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(SalesQuotationId);
            ViewBag.SubmitAction = "Approve";
            return View("Create",salesquotation);
        }

        public ActionResult Approve(SalesQuotation model)
        {

            var repo = new SalesQuotationRepository();

            repo.ApproveSalesQuotation(model);

            
            return RedirectToAction("ListSalesQuotations");
        }

        public ActionResult ListSalesQuotations()
        {
        
            var repo = new SalesQuotationRepository();

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationApproveList();
      
            return View(salesquotations);
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
        public ActionResult Create(SalesQuotation model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            SalesQuotation result= new SalesQuotationRepository().InsertSalesQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["QuotationRefNo"] = result.QuotationRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                //SalesQuotation salesquotation = new SalesQuotation();
                //salesquotation.QuotationDate = System.DateTime.Today;
                //salesquotation.PredictedClosingDate = System.DateTime.Today;
                //salesquotation.QuotationValidToDate = System.DateTime.Today;
                //salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
                //SaleOrder saleOrder = new SaleOrder();
                //saleOrder.SaleOrderDate = System.DateTime.Today;
                //saleOrder.Items = new List<SaleOrderItem>();
                //saleOrder.Items.Add(new SaleOrderItem());
                return View("Create", model);
            }
          
        }
    }
}