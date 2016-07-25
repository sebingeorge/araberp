using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class CustomerReceiptController :BaseController
    {
        // GET: CustomerReceipt
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateRequest()
        {
            FillCustomer();
            FillSO();
            FillJC();
            FillSI();
            return View("Create", new CustomerReceipt { CustomerReceiptDate = DateTime.Today });
        }
        [HttpPost]
        public ActionResult CreateRequest(CustomerReceipt model)
        {
            FillSO();
            FillJC();
            FillSI();
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            if (new CustomerReceiptRepository().InsertCustomerReceipt(model) > 0)
            {
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("CreateRequest");
            }
            else
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }
        }
    
        public void FillCustomer()
        {
            SaleOrderRepository repo = new SaleOrderRepository();
            List<Dropdown> list = repo.FillCustomer();
            ViewBag.Customer = new SelectList(list, "Id", "Name");
        }
        public void FillSO()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillSO();
            ViewBag.SO = new SelectList(list, "Id", "Name");
        }
        public void FillJC()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillJC();
            ViewBag.JC = new SelectList(list, "Id", "Name");
        }
        public void FillSI()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillSI();
            ViewBag.SI = new SelectList(list, "Id", "Name");
        }
    }
}