﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ExpenseController : BaseController
    {
        // GET: Expense
        public ActionResult Index()
        {
            return View((new ExpenseRepository()).GetList());
        }
        public ActionResult Create()
        {
            FillSupplier();
            FillAddition();
            FillDeduction();
            FillSO();
            FillJC();
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
                ExpenseRepository repo = new ExpenseRepository();
                repo.Insert(model);
                return RedirectToAction("Create");
            }
            else
            {
                FillSupplier();
                FillAddition();
                FillDeduction();
                FillSO();
                FillJC();
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
    }
}