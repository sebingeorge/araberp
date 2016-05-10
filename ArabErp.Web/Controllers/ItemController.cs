using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ItemController : Controller
    {
        // GET: Item
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Save(Item oitem)
        {

            oitem.OrganizationId = 1;
            oitem.ItemGroupId = 3;
            oitem.ItemSubGroupId = 1;
            new JobCardRepository().SaveItem(oitem);

            return View("Create");
        }

        public ActionResult View(int ItemId)
        {

            Item objItem =new JobCardRepository().GetItem(ItemId);

            return View("Create", objItem);
        }
    }
}