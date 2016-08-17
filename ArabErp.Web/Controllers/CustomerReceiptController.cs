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
    public class CustomerReceiptController : BaseController
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

        public ActionResult CreateRequest(int Code = 0)
        {
            string internalId = "";
            internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(CustomerReceipt).Name);

            ViewBag.Title = "Create";
            FillCustomer();
            FillSaleOrder(Code);
            FillJobCard(Code);
            FillSalesInvoice(Code);
            //FillSO();
            //FillJC();
            //FillSI();
            return View("Create", new CustomerReceipt { CustomerReceiptDate = DateTime.Today, CustomerReceiptRefNo = "CR/" + internalId });
        }

        public ActionResult Customer()
        {
            FillCustomer();
            return PartialView("_CustomerDropdown");
        }

        public ActionResult SaleOrder(int Code = 0, int id = 0)
        {
            FillSaleOrder(Code);
            return PartialView("_SaleOrderDropdown", new CustomerReceipt { SaleOrderId = id });
        }

        public ActionResult JobCard(int Code = 0, int id = 0)
        {
            FillJobCard(Code);
            return PartialView("_JobCardDropdown", new CustomerReceipt { JobCardId = id });
        }

        public ActionResult SalesInvoice(int Code=0, int id=0)
        {
            FillSalesInvoice(Code);
            return PartialView("_SalesInvoiceDropdown", new CustomerReceipt { SalesInvoiceId = id });
        }

        [HttpPost]
        public ActionResult CreateRequest(CustomerReceipt model, int Code = 0)
        {
            FillSaleOrder(Code);
            FillJobCard(Code);
            FillSalesInvoice(Code);
            //FillSO();
            //FillJC();
            //FillSI();
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

        public ActionResult Edit(int Id, int Code = 0)
        {
            //int Id = 0;
            FillCustomer();
            FillSaleOrder(Code);
            FillJobCard(Code);
            FillSalesInvoice(Code);
            //FillSO();
            //FillJC();
            //FillSI();
            ViewBag.Title = "Edit";
            CustomerReceipt objCustomerReceipt = new CustomerReceiptRepository().GetCustomerReceipt(Id);
            return View("Create", objCustomerReceipt);


        }

        [HttpPost]
        public ActionResult Edit(CustomerReceipt model,int Code = 0)
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
                    FillSaleOrder(Code);
                    FillJobCard(Code);
                    FillSalesInvoice(Code);
                    //FillSO();
                    //FillJC();
                    //FillSI();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CustomerReceiptRefNo"] = null;
                    return View("Create", model);
                }

            }
    

        }

        public ActionResult Delete(int Id, int Code = 0)
        {
            //int Id = 0;
            FillCustomer();
            FillSaleOrder(Code);
            FillJobCard(Code);
            FillSalesInvoice(Code);
            //FillSO();
            //FillJC();
            //FillSI();
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
        public void FillSaleOrder(int Id)
        {
            CustomerReceiptRepository Repo = new CustomerReceiptRepository();
            var List = Repo.FillSO(Id);
            ViewBag.CSO = new SelectList(List, "Id", "Name");
        }
        public void FillJobCard(int Id)
        {
            CustomerReceiptRepository Repo = new CustomerReceiptRepository();
            var List = Repo.FillJC(Id);
            ViewBag.CJC = new SelectList(List, "Id", "Name");
        }

        public void FillSalesInvoice(int Id)
        {
            CustomerReceiptRepository Repo = new CustomerReceiptRepository();
            var List = Repo.FillSI(Id);
            ViewBag.CSI = new SelectList(List, "Id", "Name");
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