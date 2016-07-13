using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CurrencyController : BaseController
    {
        // GET: Currency
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillCurrencySymbols();
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Currency).Name);
            
            return View(new Currency { CurrencyRefNo="CUR/"+internalid});
        }
        [HttpPost]
        public ActionResult Create(Currency model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            FillCurrencySymbols();

            var repo = new CurrencyRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Currency", "CurrencyName", model.CurrencyName, null, null);
            if (!isexists)
            {
                var result = new CurrencyRepository().InsertCurrency(model);
                if (result.CurrencyId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["RefNo"] = result.CurrencyRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                  
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
              
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result=new CurrencyRepository().InsertCurrency(model);
        //      if (result.CurrencyId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["RefNo"] = result.CurrencyRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        FillCurrencySymbols();
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["RefNo"] = null;
        //        return View("Create", model);
        //    }
        //}
        public ActionResult Edit(int Id)
        {
            FillCurrencySymbols();
            Currency model = new CurrencyRepository().GetCurrency(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(Currency model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new CurrencyRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Currency", "CurrencyName", model.CurrencyName, "CurrencyId",model.CurrencyId);
            if (!isexists)
            {
                var result = new CurrencyRepository().UpdateCurrency(model);
                if (result.CurrencyId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["RefNo"] = result.CurrencyRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillCurrencySymbols();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                FillCurrencySymbols();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new CurrencyRepository().UpdateCurrency(model);


        //    if (result.CurrencyId > 0)
        //    {
        //        TempData["Success"] = "Updated Successfully!";
        //        TempData["RefNo"] = result.CurrencyRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        FillCurrencySymbols();
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["RefNo"] = null;
        //        return View("Create", model);
        //    }

        //}
        public ActionResult Delete(int Id)
        {
            FillCurrencySymbols();
            Currency model = new CurrencyRepository().GetCurrency(Id);
            //FillDesignationDropdown();
            //FillCategoryDropdown();
            //FillLocationDropdown();


            return View("Create", model);


        }
        [HttpPost]
        public ActionResult Delete(Currency model)
        {

            var result = new CurrencyRepository().DeleteCurrency(model);


            if (result.CurrencyId > 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["RefNo"] = model.CurrencyRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                FillCurrencySymbols();
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

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