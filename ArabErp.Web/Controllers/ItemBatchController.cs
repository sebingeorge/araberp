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
                    ItemBatch model = new ItemBatchRepository().GetGRNItem(grnItemId: id);
                    List<ItemBatch> list = new List<ItemBatch>();
                    for (int i = 0; i < model.Quantity; i++)
                        list.Add(model);
                    return View(list);
                }
                throw new NullReferenceException();
            }
            catch (NullReferenceException)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again";
            }
            catch (SqlException sx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Check your network connection and try again|" + sx.Message;
            }
            catch (Exception)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again";
            }
            return RedirectToAction("Pending");
        }
        [HttpPost]
        public ActionResult Create(IList<ItemBatch> model)
        {
            foreach (ItemBatch item in model)
            {
                item.CreatedBy = UserID.ToString();
                item.OrganizationId = OrganizationId;
                item.CreatedDate = DateTime.Now;
            }
            try
            {
                new ItemBatchRepository().InsertItemBatch(model);
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("Pending");
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Check your network connection and try again|" + ex.Message;
            }
            return View(model);
        }

        public ActionResult PendingReservation()
        {
            return View(new ItemBatchRepository().GetUnreservedItems());
        }

        [HttpGet]
        public ActionResult Reserve(int id = 0)
        {
            if (id != 0)
            {

                var items = new ItemBatchRepository().GetItemBatchForReservation();
                for (int i = 0; i < items.Count() - 1; i++)
                    items.ElementAt(i).SaleOrderItemId = id;

                return View(items);
            }
            else throw new NullReferenceException();
        }

        [HttpPost]
        public ActionResult Reserve(IList<ItemBatch> model)
        {
            int? sid = 0;

            sid = model.First().SaleOrderItemId;

            try
            {
                IList<ItemBatch> selectedmodel = (from i in model where i.isSelected == true select i).ToList<ItemBatch>();

                if (selectedmodel.Count>0)
                {
                    //new ItemBatchRepository().ReserveItemBatch(selectedmodel);
                }
                    TempData["success"] = "Saved successfully";
                    TempData["error"] = "";
                    return RedirectToAction("Reserve",new {id=sid });

                }
               
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Check your network connection and try again|" + ex.Message;
            }
            return View(model);
        }


    }
}