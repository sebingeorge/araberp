using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class DeliveryChallanController : BaseController
    {
        // GET: DeliveryChallan
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
        public ActionResult PendingDeliveryChallan(int customerId)
        {
            if (customerId == 0)
            {
                return PartialView("_PendingDeliveryChallan", new List<PendingJC>());
            }
            return PartialView("_PendingDeliveryChallan", new DeliveryChallanRepository().PendingDeliveryChallan(customerId));
        }
        public ActionResult Save(int id = 0)
        {
            if (id != 0)
            {
                EmployeeDropdown();
                return View(new DeliveryChallan
                {
                    JobCardId = id,
                    DeliveryChallanDate = DateTime.Now,
                    ItemBatches = new DeliveryChallanRepository().GetSerialNos(id).ToList()
                });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(DeliveryChallan model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            foreach (ItemBatch item in model.ItemBatches)
            {
                item.WarrantyStartDate = model.DeliveryChallanDate;
                item.WarrantyExpireDate = model.DeliveryChallanDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
            }
            string ref_no = new DeliveryChallanRepository().InsertDeliveryChallan(model);
            if (ref_no.Length>0)
            {
                TempData["success"] = "Saved Successfully. The Reference No. is " + ref_no;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Some error occured while saving. Please try again";
                return View(model);
            }
        }
        public JsonResult GetCustomerAddress(int id = 0)
        {
            return Json(new CustomerRepository().GetCustomerAddress(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJobCardDetails(int id = 0)
        {
            var data = new DeliveryChallanRepository().GetJobCardDetails(id);
            return Json(new
            {
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