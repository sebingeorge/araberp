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

        public ActionResult CustomerReceiptList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CustomerReceiptRepository();
            var List = repo.GetCustomerReceipt();
            return PartialView("_CustomerReceiptListView", List);
        }

        public ActionResult CreateRequest()
        {
            ViewBag.Title = "Create";
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
            model.CreatedBy = UserID.ToString();
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

        public ActionResult Edit(int Id)
        {
            //int Id = 0;
            FillCustomer();
            FillSO();
            FillJC();
            FillSI();
            ViewBag.Title = "Edit";
            CustomerReceipt objCustomerReceipt = new CustomerReceiptRepository().GetCustomerReceipt(Id);
            return View("Create", objCustomerReceipt);


        }

        [HttpPost]
        public ActionResult Edit(CustomerReceipt model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CustomerReceiptRepository();
            //bool isexists = repo.IsFieldExists(repo.ConnectionString(), "CustomerReceipt", "CustomerReceiptRefNo", model.CustomerReceiptRefNo, "CustomerReceiptId", model.CustomerReceiptId);
            //if (!isexists)
            {
                var result = new CustomerReceiptRepository().UpdateCustomerReceipt(model);
                if (result.CustomerReceiptId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CustomerReceiptRefNo"] = result.CustomerReceiptRefNo;
                    return RedirectToAction("Index");
                }

                else
                {
                    FillCustomer();
                    FillSO();
                    FillJC();
                    FillSI();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerReceiptRefNo"] = null;
                    return View("Create", model);
                }

            }
            //else
            //{

            //    FillCustomer();
            //    FillSO();
            //    FillJC();
            //    FillSI();
            //    TempData["error"] = "This Ref No Alredy Exists!!";
            //    TempData["CustomerReceiptRefNo"] = null;
            //    return View("Create", model);
            //}

       }

        public ActionResult Delete(int Id)
        {
            //int Id = 0;
            FillCustomer();
            FillSO();
            FillJC();
            FillSI();
            ViewBag.Title = "Delete";
            CustomerReceipt objCustomerReceipt = new CustomerReceiptRepository().GetCustomerReceipt(Id);
            return View("Create", objCustomerReceipt);
        }

        [HttpPost]
        public ActionResult Delete(CustomerReceipt model)
        {
            int result = new CustomerReceiptRepository().DeleteCustomerReceipt(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CustomerReceiptRefNo"] = model.CustomerReceiptRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Customer Receipt. It Is Already In Use";
                    TempData["CustomerReceiptRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerReceiptRefNo"] = null;
                }
                return RedirectToAction("Index");
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