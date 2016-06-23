using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ConsumptionController : Controller
    {
        // GET: Consumption
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            JobCardDropdown();
            ItemDropdown();
            List<ConsumptionItem> Items = new List<ConsumptionItem>();
            Items.Add(new ConsumptionItem());
            return View(new Consumption { ConsumptionItems = Items, ConsumptionDate = DateTime.Today });
        }

        //public ActionResult ConsumptionGrid()
        //{
        //    return PartialView("_ConsumptionGrid");
        //}

        public void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public void JobCardDropdown()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardDropdown(), "Id", "Name");
        }
    }
}