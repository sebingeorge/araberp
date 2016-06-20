using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ExpenseController : Controller
    {
        // GET: Expense
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillSupplier();
            FillAddition();
            FillDeduction();
            ExpenseBill expense = new ExpenseBill();
            expense.ExpenseDate = expense.ExpenseBillDate = expense.ExpenseBillDueDate = DateTime.Now;
            expense.ExpenseBillItem = new List<ExpenseBillItem>();
            expense.deductions = new List<ExpenseBillItem>();
            expense.ExpenseBillItem.Add(new ExpenseBillItem());
            expense.deductions.Add(new ExpenseBillItem());
            return View(expense);
        }
        [HttpPost]
        public ActionResult Create(ExpenseBill model)
        {
            if(ModelState.IsValid)
            {
                return RedirectToAction("Create");
            }
            else
            {
                FillSupplier();
                FillAddition();
                FillDeduction();
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
    }
}