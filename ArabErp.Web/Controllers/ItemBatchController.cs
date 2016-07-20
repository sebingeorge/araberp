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
        public ActionResult Reserve(int id = 0, int item = 0)//SOI id and material id is received here
        {
            if (id != 0 && item != 0)
            {
                var items = new ItemBatchRepository().GetItemBatchForReservation(id, item);
                return View(items);
            }
            else
            {
                TempData["success"] = "";
                TempData["error"] = "That was an invalid request. Please try again.";
                return RedirectToAction("PendingReservation");
            }
        }

        [HttpPost]
        public ActionResult Reserve(IList<ItemBatch> model)
        {
            int? sid = 0;

            sid = model.First().SaleOrderItemId;

            try
            {
                IList<ItemBatch> selectedmodel = (from i in model where i.isSelected == true select i).ToList<ItemBatch>();

                if (selectedmodel.Count > 0)
                {
                    new ItemBatchRepository().ReserveItemBatch(selectedmodel);
                }
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("PendingReservation");

            }

            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again|" + ex.Message;
            }
            return View(model);
        }
        public ActionResult ReservedList()
        {
            return View(new ItemBatchRepository().GetReservedItems());
        }

        public ActionResult UnReserve(int id = 0)
        {
            if (id != 0)
            {
                return View(new ItemBatchRepository().GetItemBatchForUnReservation(SaleOrderId: id));
            }
            else
            {
                return RedirectToAction("ReservedList");
            }
        }

        [HttpPost]
        public ActionResult UnReserve(IList<ItemBatch> model)
        {
            try
            {
                List<int> selected = (from item in model where item.isSelected select item.ItemBatchId).ToList<int>();
                new ItemBatchRepository().UnReserveItems(selected);
                TempData["success"] = "Unreserved successfully";
                TempData["error"] = "";
                return RedirectToAction("ReservedList");
            }
            catch (Exception)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return View("UnReserve", model);
            }
        }

        /// <summary>
        /// List of materials that have a serial number
        /// </summary>
        /// <returns></returns>
        public ActionResult MaterialList()
        {
            FillMaterial(); FillSaleOrder();
            return View();
        }

        public ActionResult MaterialListGrid(string serialno = "", int item = 0, int type = 0, int saleorder = 0)
        {
            return PartialView("_MaterialListGrid", new ItemBatchRepository().GetMaterialList(serialno, item, type, saleorder));
        }

        #region Dropdowns
        public void FillMaterial()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }

        public void FillSaleOrder()
        {
            ViewBag.saleOrderList = new SelectList(new DropdownRepository().SaleOrderDropdown1(), "Id", "Name");
        }
        #endregion
    }
}