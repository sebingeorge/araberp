using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ItemBatchController : BaseController
    {
        // GET: ItemBatch
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Pending()
        {
            return View(new ItemBatchRepository().PendingGRNItems());
        }

        public ActionResult Create(int id = 0)
        {
            if (id != 0)
            {
                //do get here
            }
            return View();
        }
        [HttpPost]
        public ActionResult Create(ItemBatch model)
        {
            //do post here
            return View();
        }
    }
}