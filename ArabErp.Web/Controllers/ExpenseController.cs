using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class ExpenseController : BaseController
    {
        // GET: Expense
        public ActionResult Index()
        {
            FillExpenseNo();
            FillEBSupplier();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int supid = 0)
        {
            return PartialView("_PreviousList", new ExpenseRepository().GetList( id: id,supid:supid, from: from, to: to));
        }

        public ActionResult Create()
        {
            string internalId = "";
            internalId = DatabaseCommonRepository.GetNextDocNo(14, OrganizationId);

            FillDropdowns();
            ExpenseBill expense = new ExpenseBill();
            expense.ExpenseDate = expense.ExpenseBillDate = expense.ExpenseBillDueDate = DateTime.Now;
            expense.ExpenseNo = internalId;

            expense.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;
            expense.ExpenseBillItem = new List<ExpenseBillItem>();
            expense.deductions = new List<ExpenseBillItem>();
            expense.ExpenseBillItem.Add(new ExpenseBillItem());
            expense.deductions.Add(new ExpenseBillItem());
            return View(expense);
        }

        private void FillDropdowns()
        {
            FillSupplier();
            FillAddition();
            FillDeduction();
            FillSO();
            FillJC();
            FillCurrency();
        }

        [HttpPost]
        public ActionResult Create(ExpenseBill model)
        {
            model.CreatedBy = UserID.ToString();
            model.CreatedDate = DateTime.Now;
            model.OrganizationId = OrganizationId;
            if(ModelState.IsValid)
            {
                ExpenseRepository repo = new ExpenseRepository();
                TempData["success"] = "Saved Successfully. Reference no. is " + repo.Insert(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillSupplier();
                FillAddition();
                FillDeduction();
                FillSO();
                FillJC();
                FillCurrency();
                TempData["error"] = "Some error occurred. Please try again.";
                return View(model);
            }
        }
        public void FillSupplier()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillSupplier();
            ViewBag.SupplierList = new SelectList(list, "Id", "Name");
        }
        public void FillAddition()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillAddition();
            ViewBag.Additions = new SelectList(list, "Id", "Name");            
        }
        public void FillDeduction()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = repo.FillDeduction();
            ViewBag.Deductions = new SelectList(list, "Id", "Name");
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
        public void FillCurrency()
        {
            var repo = new ExpenseRepository();
            var List = repo.FillCurrency();
            ViewBag.Currency = new SelectList(List, "Id", "Name");
        }
        public void FillExpenseNo()
        {
            ViewBag.ExpenseNoList = new SelectList(new DropdownRepository().ExpenseBillNoDropdown(), "Id", "Name");
        }
        public void FillEBSupplier()
        {
            ViewBag.SupplierList = new SelectList(new DropdownRepository().ExpenseBillSupplierDropdown(), "Id", "Name");
         }
        public JsonResult GetDueDate(DateTime date, int supplierId)
        {
            //var res = (new PurchaseBillRepository()).GetCurrencyIdByCustKey(cusKey);
            DateTime duedate = (new ExpenseRepository()).GetDueDate(date, supplierId);
            //var Result = new { VehicleId = List.VehicleModelId, VehicleName = List.VehicleModelName };
            //return Json(Result, JsonRequestBehavior.AllowGet);

            return Json(duedate.ToString("dd/MMMM/yyyy"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingApproval()
        {
            return View(new ExpenseRepository().GetUnapprovedExpenseBills());
        }

        public ActionResult Approve(int id=0)
        {
            if (id != 0)
            {
                FillDropdowns();
                var model = new ExpenseRepository().GetExpenseBill(id);
                ViewBag.mode = "Approve";
                return View("Create", model);
            }
            else
            {
                TempData["error"] = "That was an invalid request. Please try again.";
                return RedirectToAction("PendingApproval");
            }
        }

        [HttpPost]
        public ActionResult Approve(ExpenseBill model)
        {
            model.ApprovedBy = UserID;
            int i = new ExpenseRepository().Approve(model);
            if (i>0)
            {
                TempData["success"] = "Approved Successfully";
                return RedirectToAction("PendingApproval"); 
            }
            else
            {
                FillDropdowns();
                TempData["error"] = "Some error occured while approving. Please try again.";
                return View("Create", model);
            }
        }

        public ActionResult Edit(int id = 0)
        {
            if (id != 0)
            {
                FillDropdowns();
                var model = new ExpenseRepository().GetExpenseBill(id);

                return View("Edit", model);
            }
            else
            {
                TempData["error"] = "That was an invalid request. Please try again.";
                return RedirectToAction("Create");
            }
        }

        [HttpPost]
        public ActionResult Edit(ExpenseBill model)
        {
            ViewBag.Title = "Edit";
            model.CreatedBy = UserID.ToString();
            model.CreatedDate = DateTime.Now;

            FillDropdowns();

            var repo = new ExpenseRepository();

            var result1 = new ExpenseRepository().CHECK(model.ExpenseId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["PurchaseRequestNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result2 = new ExpenseRepository().DeleteExpenseBillDT(model.ExpenseId);
                    string result3 = new ExpenseRepository().UpdateExpenseBillHD(model);
                    string id = new ExpenseRepository().InsertDT(model);
                    
                //TempData["success"] = "Updated Successfully (" + model.SalesInvoiceRefNo + ")";
                //TempData["SalesInvoiceRefNo"] = model.SalesInvoiceRefNo;

                    TempData["success"] = "Updated successfully. Purchase Request Reference No. is " + result3;
                    TempData["error"] = "";
                    return RedirectToAction("Create");
                }
                catch (SqlException sx)
                {
                    TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
                }
                catch (NullReferenceException nx)
                {
                    TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
                }
                return RedirectToAction("Create");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new ExpenseRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["SupplyOrderNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result2 = new ExpenseRepository().DeleteExpenseBillDT(Id);
                var result3 = new ExpenseRepository().DeleteExpenseBillHD(Id);

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SupplyOrderNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }
    }
}