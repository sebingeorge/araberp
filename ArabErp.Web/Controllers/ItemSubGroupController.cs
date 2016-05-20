using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class ItemSubGroupController : Controller
    {
        // GET: ItemSubGroup
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            dropdown();
            return View();
        }

        public ActionResult Save(ItemSubGroup objItemSubGroup)
        {
            dropdown();
            new ItemSubGroupRepository().InsertItemSubGroup(objItemSubGroup);
            return View("Create");
        }

        public void dropdown()
        {
            var repo = new ItemSubGroupRepository();
            var List = repo.FillGroup();
            ViewBag.ItemGroup = new SelectList(List, "Id", "Name");
        }

        public ActionResult FillItemSubGroupList()
        {

            var repo = new ItemSubGroupRepository();
            var List = repo.FillItemSubGroupList();
            return PartialView("ItemSubGroupListView", List);
        }

    }
}