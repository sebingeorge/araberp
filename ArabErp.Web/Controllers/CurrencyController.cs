using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CurrencyController : Controller
    {
        // GET: Currency
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            var repo = new CurrencyRepository();
            var sym = repo.FillSymbol();
            ViewBag.symbols = new SelectList(sym.Symbols, "SymbolId", "SymbolName");
            return View(sym);
        }
        public ActionResult Save(Currency objCurrency)
        {
            dropdown();
            new CurrencyRepository().InsertCurrency(objCurrency);
            return View("Create");
        }
        public void dropdown()
        {
            var repo = new CurrencyRepository();
            var sym = repo.FillSymbol();
            ViewBag.symbols = new SelectList(sym.Symbols, "SymbolId", "SymbolName");

        }
        public ActionResult FillCurrencyList()
        {
            //var list = _GradeVsDisplayOrderManager.GetGradeVsDisplayOrderList();
            var repo = new CurrencyRepository();
            var List = repo.FillCurrencyList();
            return PartialView("_CurrencyListView", List);
        }
       


    }
}