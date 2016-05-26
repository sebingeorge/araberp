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
            FillCurrencySymbols();
            return View();
        }
        public ActionResult Save(Currency objCurrency)
        {
            FillCurrencySymbols();
            new CurrencyRepository().InsertCurrency(objCurrency);
            return View("Create");
        }
        public void FillCurrencySymbols()
        {
            var repo = new CurrencyRepository();
            var sym = repo.FillSymbol();
            ViewBag.symbols = new SelectList(sym.Symbols, "SymbolId", "SymbolName");
        }
        public ActionResult FillCurrencyList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new CurrencyRepository();
            var List = repo.FillCurrencyList();
            return PartialView("_CurrencyListView", List);
        }
       


    }
}