﻿using System;
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
            FillVehicle();
            FillCommissionAgent();
            FillEmployee();
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            return View(saleOrder);
        }
        public ActionResult DisplaySOList()
        {
            FillWrkDesc();
            FillUnit();
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            saleOrder.Items.Add(new SaleOrderItem());
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
            var repo = new SaleOrderRepository();
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
        [HttpPost]
        public ActionResult Save(SaleOrder model)
        {
            FillWrkDesc();
            FillUnit();
            FillCustomer();
            FillVehicle();
            FillCommissionAgent();
            FillEmployee();
            return View("Create", model);
        }
    }
}