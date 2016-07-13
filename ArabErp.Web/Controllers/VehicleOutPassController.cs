using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class VehicleOutPassController : BaseController
    {
        // GET: VehicleOutPass
        public ActionResult Index()
        {
            CustomerDropdown();
            return View();
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public void CustomerDropdown()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown(), "Id", "Name");
        }
        public ActionResult PendingVehicleOutPass(int customerId)
        {
            if (customerId == 0)
            {
                return PartialView("_PendingVehicleOutPass", new List<PendingJC>());
            }
            return PartialView("_PendingVehicleOutPass", new VehicleOutPassRepository().PendingVehicleOutPass(customerId));
        }
        public ActionResult Save(int id = 0)
        {
            if (id != 0)
            {
                EmployeeDropdown();
                return View(new VehicleOutPass { JobCardId = id });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(VehicleOutPass model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if (new VehicleOutPassRepository().InsertVehicleOutPass(model) > 0)
                return RedirectToAction("Index");
            else return View(new VehicleOutPass { JobCardId = model.JobCardId });
        }
        public JsonResult GetCustomerAddress(int id = 0)
        {
            return Json(new CustomerRepository().GetCustomerAddress(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJobCardDetails(int id = 0)
        {
            var data = new VehicleOutPassRepository().GetJobCardDetails(id);
            return Json(new { 
                JobCardNoDate = data.JobCardNoDate,
                SaleOrderNoDate = data.SaleOrderNoDate,
                VehicleModel = data.VehicleModel,
                WorkDescr = data.WorkDescr,
                CustomerName = data.CustomerName,
                RegistrationNo = data.RegistrationNo
            }, JsonRequestBehavior.AllowGet);
        }
    }
}