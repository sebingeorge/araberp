using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            try
            {
                if (id != 0)
                {
                    return View(new ItemBatchRepository().GetGRNItem(grnItemId: id));
                }
                throw new NullReferenceException();
            }
            catch (NullReferenceException)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again";
            }
            catch (SqlException)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Check your network connection and try again";
            }
            catch (Exception)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again";
            }
            return RedirectToAction("Pending");
        }
        [HttpPost]
        public ActionResult Create(ItemBatch model)
        {
            //do post here
            return View();
        }
    }
}