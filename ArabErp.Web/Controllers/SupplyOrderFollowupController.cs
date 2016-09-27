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
    public class SupplyOrderFollowupController : BaseController
    {
        // GET: SupplyOrderFollowup
        public ActionResult Index()
        {
            FillBatch();
            return View();
        }

        //public ActionResult Pending()
        //{
        //    SupplyOrderFollowupRepository repo = new SupplyOrderFollowupRepository();
        //    var model = repo.GetSupplyOrderFollowup(OrganizationId);
        //    return View(model);
            
        //}
      //  [HttpPost]
        public ActionResult Save(SupplyOrderFollowUpList model)
        {

            foreach (SupplyOrderFollowup item in model.SupplyOrderFollowups)
            {
                if (item.ExpectedDate == DateTime.MinValue) continue;
                item.CreatedDate = System.DateTime.Now;
                item.CreatedBy = UserID.ToString();
                item.OrganizationId = OrganizationId;
                item.SupplyOrderFollowupDate = System.DateTime.Now;
                
            }
            
            var rtn = new SupplyOrderFollowupRepository().InsertSupplyOrderFollowup(model.SupplyOrderFollowups);
      
            TempData["Success"] = "Added Successfully!";

            return RedirectToAction("Index");
        }
        public ActionResult ItemList(int? page, string name = "", string suppliername = "", int? batch=null)
       {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            return PartialView("_Pending", new SupplyOrderFollowupRepository().GetSupplyOrderFollowup(OrganizationId, name,suppliername,batch));
        }

        public void FillBatch()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 0, Name = "Non-Batch" });
            types.Add(new Dropdown { Id = 1, Name = "Batch" });
            ViewBag.BatchList = new SelectList(types, "Id", "Name");
        }
        //public ActionResult Create(int id = 0)
        //{
        //    try
        //    {
        //        if (id != 0)
        //        {
        //            ItemBatch model = new ItemBatchRepository().GetGRNItem(grnItemId: id);
        //            List<ItemBatch> list = new List<ItemBatch>();
        //            for (int i = 0; i < model.Quantity; i++)
        //                list.Add(model);
        //            return View(list);
        //        }
        //        throw new NullReferenceException();
        //    }
        //    catch (NullReferenceException)
        //    {
        //        TempData["success"] = "";
        //        TempData["error"] = "Some required data was missing. Please try again";
        //    }
        //    catch (SqlException sx)
        //    {
        //        TempData["success"] = "";
        //        TempData["error"] = "Some error occured while connecting to database. Check your network connection and try again|" + sx.Message;
        //    }
        //    catch (Exception)
        //    {
        //        TempData["success"] = "";
        //        TempData["error"] = "Some error occured. Please try again";
        //    }
        //    return RedirectToAction("Pending");
        //}
    }
}