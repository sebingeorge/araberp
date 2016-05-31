using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class StockPointController : Controller
    {
        // GET: StockPoint
        public ActionResult Index()
        {
            return View();
            
        }
        public ActionResult create()
        {
            return View();
        }
        public ActionResult Save(Stockpoint objStockpoint)
        {
            var repo = new StockpointRepository();
            new StockpointRepository().InsertStockpoint(objStockpoint);
            return View("Create");
        }
        public ActionResult FillStockpointList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo=new StockpointRepository ();
            var List=repo.FillStockpointList();
            return PartialView("StockpointListView",List);
        }

    }
}