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
            FillGRNNo();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousListGrid", new ItemBatchRepository().PreviousList(id, from, to, OrganizationId));
        }

        public ActionResult Pending(int type = 0)//type 0 means GRN, 1 means Opening Stock
        {
            return
                type == 0 ?
                View(new ItemBatchRepository().PendingGRNItems()) :
                View("PendingOpeningStock", new ItemBatchRepository().PendingGRNItems(type));
        }

        public ActionResult Create(int id = 0, int type = 0)
        {
            try
            {
                if (id != 0)
                {
                    ItemBatch model = new ItemBatch();
                    if (type == 0)
                    {
                        model = new ItemBatchRepository().GetGRNItem(grnItemId: id);
                    }
                    else if (type == 1)
                    {
                        model = new ItemBatchRepository().GetOpeningStockItem(OpeningStockItemId: id);
                    }
                    model.isOpeningStock = type;
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
            HashSet<string> temp = new HashSet<string>();
            foreach (var item in model)
            {
                temp.Add(item.SerialNo);
            }
            if (temp.Count != model.Count)
            {
                TempData["error"] = "Serial numbers cannot be same. Please enter different serial numbers.";
                return View(model);
            }
            foreach (ItemBatch item in model)
            {
                item.CreatedBy = UserID.ToString();
                item.OrganizationId = OrganizationId;
                item.CreatedDate = DateTime.Now;
            }
            try
            {
                string existingSerialNos = new ItemBatchRepository().IsSerialNoExists(model.Select(m => m.SerialNo).ToList());
                if (existingSerialNos == null)
                {
                    new ItemBatchRepository().InsertItemBatch(model);
                    TempData["success"] = "Saved successfully";
                    TempData["error"] = "";
                    return RedirectToAction("Pending", new { type = model[0].isOpeningStock });
                }
                TempData["error"] = existingSerialNos + " already exists.";
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
            return View();
        }

        public ActionResult PendingReservationGrid(string saleOrder = "", string item = "")
        {
            return PartialView("_PendingReservationGrid", new ItemBatchRepository().GetUnreservedItems(saleOrder: saleOrder.Trim(), itemName: item.Trim()));
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
                    new ItemBatchRepository().ReserveItemBatch(selectedmodel, UserID.ToString());
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
            return View();
        }

        public ActionResult ReservedListGrid(string saleOrder = "", string serialNo = "")
        {
            return PartialView("_ReservedListGrid", new ItemBatchRepository().GetReservedItems(saleOrder: saleOrder.Trim(), serialNo: serialNo.Trim()));
        }

        public ActionResult UnReserve(int id = 0)//sale order id is received here
        {
            if (id != 0)
            {
                return View(new ItemBatchRepository().GetItemBatchForUnReservation(SaleOrderId: id));
            }
            else
            {
                TempData["success"] = "";
                TempData["error"] = "That was an invalid request. Please try again.";
                return RedirectToAction("ReservedList");
            }
        }

        [HttpPost]
        public ActionResult UnReserve(IList<ItemBatch> model)
        {
            try
            {
                List<int> selected = (from item in model where item.isSelected select item.ItemBatchId).ToList<int>();
                if (new ItemBatchRepository().UnReserveItems(selected, UserID.ToString()) <= 0) throw new Exception();
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
            return PartialView("_MaterialListGrid", new ItemBatchRepository().GetMaterialList(serialno.Trim(), item, type, saleorder));
        }

        /// <summary>
        /// Get Sale Order Ref. No. and Date, GRN Ref. No. and Date, Work Desc. Ref. No and Short Name
        /// </summary>
        /// <param name="id">SaleOrderId OR SaleOrderItemId</param>
        /// <param name="type">0 if SaleOrderId, 1 if SaleOrderItemId</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetItemBatchDetails(int id, int type)
        {
            ItemBatch model = new ItemBatchRepository().GetItemBatchDetails(id, type);
            return Json(new
            {
                WorkDescRefNo = model.WorkDescrRefNo + " - " + model.WorkDescrShortName,
                SaleOrderRefNo = model.SaleOrderRefNo + " - " + model.SaleOrderDate,
                GRNRefNo = model.GRNNo + " - " + model.GRNDate.ToString("dd MMMM yyyy")
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FGTrackingPopup(int id = 0)
        {
            if (id != 0)
                return PartialView("_FGTrackingDetails", new ItemBatchRepository().FGTrackingDetailsByItemBatchId(id));
            return View();
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

        public void FillGRNNo()
        {
            ViewBag.grnList = new SelectList(new DropdownRepository().GRNDropdown(), "Id", "Name");
        }
        #endregion
    }
}