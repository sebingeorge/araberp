﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class VehicleInPassController : Controller
    {
        // GET: VehicleInPass
        public ActionResult Index()
        {
            CustomerDropdown();
            return View();
        }
        public void CustomerDropdown()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown(), "Id", "Name");
        }
        public PartialViewResult PendingVehicleInPass(int customerId)
        {
            if (customerId == 0)
            {
                List<PendingSO> list = new List<PendingSO>();
                return PartialView("_PendingVehicleInPass", list);
            }
            return PartialView("_PendingVehicleInPass", new VehicleInPassRepository().PendingVehicleInpass(customerId));
        }
        public ActionResult Save(int id=0)
        {
            if (id != 0)
            {
                EmployeeDropdown();
                return View(new VehicleInPass { SaleOrderItemId = id });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(VehicleInPass model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if(new VehicleInPassRepository().InsertVehicleInPass(model) > 0)
                return RedirectToAction("Index");
            else return View(new VehicleInPass { SaleOrderItemId = model.SaleOrderItemId });
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public JsonResult GetSaleOrderItemDetails(int id = 0)
        {
            var data = new VehicleInPassRepository().GetSaleOrderItemDetails(id);
            return Json(new
            {
                SaleOrderRefNo = data.SaleOrderRefNo,
                WorkDescr = data.WorkDescr,
                VehicleModelName = data.VehicleModelName,
                CustomerName = data.CustomerName
            }, JsonRequestBehavior.AllowGet);
        }
    }
}