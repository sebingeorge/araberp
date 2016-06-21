using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class GrnWareHouseController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: GrnWareHouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingGrnWareHouse()
        {
            SupplierDropdown1();
            //var repo = new GRNRepository();

            //IEnumerable<PendingSupplyOrder> pendingSO = repo.GetGRNPendingList();
            //return View(pendingSO);
            return View();
        }


        public ActionResult PreviousList()
        {
            var repo = new GRNRepository();
            IEnumerable<GRN> GRNList = repo.GetGRNPreviousList();
            return View("PreviousList", GRNList);
        }


        public ActionResult Create(IList<PendingForGRN> list)
        {
            try
            {
                GRN model = new GRN();
                if (list.Count > 0)
                {
                    List<int?> id;

                    if (list[0].isDirectPurchase)
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.DirectPurchaseRequestId).ToList();
                        if (id.Count > 0)
                        {
                            model.isDirectPurchaseGRN = true;
                            model.Items = new GRNRepository().GetDirectGRNItems(id);
                            model.GRNDate = DateTime.Today;
                            SupplierDropdown();
                        }
                        else
                        {
                            throw new NullReferenceException("1No purchase requests were selected. Please select atleast one purchase request and try again.");
                        }
                    }
                    else
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.SupplyOrderId).ToList();
                        if (id.Count > 0)
                        {
                            model = new GRNRepository().GetGRNDetails(list[0].SupplierId);
                            model.GRNDate = DateTime.Today;
                            model.Items = new GRNRepository().GetGRNItem(id);
                        }
                        else
                        {
                            throw new NullReferenceException("1No supply orders were selected. Please select atleast one supply order and try again.");
                        }
                    }
                }

                //GRNRepository repo = new GRNRepository();
                FillWarehouse();
                FillCurrency();

                //GRN model = repo.GetGRNDetails(SupplyOrderId ?? 0);
                //var GRNList = repo.GetGRNItem(SupplyOrderId ?? 0);
                //model.Items = new List<GRNItem>();
                //foreach (var item in GRNList)
                //{
                //    var grnitem = new GRNItem
                //    {
                //        SupplyOrderItemId = item.SupplyOrderItemId,
                //        ItemName = item.ItemName,
                //        ItemId = item.ItemId,
                //        PartNo = item.PartNo,
                //        Remarks = item.Remarks,
                //        PendingQuantity = item.PendingQuantity,
                //        Quantity = item.Quantity,
                //        Unit = item.Unit,
                //        Rate = item.Rate,
                //        Discount = item.Discount,
                //        Amount = item.Amount
                //    };
                //    model.Items.Add(grnitem);

                //}
                return View(model);
            }
            catch (NullReferenceException nx)
            {
                if (nx.Message.StartsWith("1"))
                    TempData["error"] = nx.Message.Substring(1);
                else TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            TempData["success"] = "";

            try
            {
                if (list[0].isDirectPurchase)
                    return RedirectToAction("PendingDirectPurchase");
                return RedirectToAction("PendingGrnWareHouse");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public void FillWarehouse()
        {
            GRNRepository repo = new GRNRepository();
            var result = repo.GetWarehouseList();
            ViewBag.WarehouseList = new SelectList(result, "StockPointId", "StockPointName");
        }

        public void FillCurrency()
        {
            var repo = new GRNRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }

        public ActionResult Save(GRN model)
        {
            try
            {
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                if (new GRNRepository().InsertGRN(model) > 0)
                {
                    TempData["success"] = "Saved succesfully";
                    TempData["error"] = "";
                    if (model.isDirectPurchaseGRN)
                        return RedirectToAction("PendingDirectPurchase");
                    return RedirectToAction("PendingGrnWareHouse");
                }
            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                if (nx.Message.StartsWith("1"))
                    TempData["error"] = nx.Message;
                else TempData["error"] = "Some required value was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            FillCurrency();
            SupplierDropdown();
            return View("Create", model);
        }

        public ActionResult Modify(int? GRNId)
        {
            GRNRepository repo = new GRNRepository();
            FillWarehouse();
            FillCurrency();

            GRN model = repo.GetGRNDISPLAYDetails(GRNId ?? 0);
            model.Items = repo.GetGRNDISPLAYItem(GRNId ?? 0);

            return View("Create", model);
        }

        public ActionResult PendingDirectPurchase()
        {
            return View();
        }
        public PartialViewResult PendingGrid(int supplierId = 0)
        {
            try
            {
                return PartialView("_PendingGrid", new GRNRepository().GetGRNPendingList(supplierId));
            }
            catch (Exception)
            {
                return PartialView("_PendingGrid", new List<PendingForGRN>());
            }
        }

        public ActionResult PendingDirectPurchaseGrid()
        {
            try
            {
                return PartialView("_PendingDirectPurchase", new GRNRepository().GetPendingDirectPurchase());
            }
            catch (Exception)
            {
                return PartialView("_PendingDirectPurchase", new List<PendingForGRN>());
            }
        }
        /// <summary>
        /// All active suppliers
        /// </summary>
        public void SupplierDropdown()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().SupplierDropdown(), "Id", "Name");
        }
        /// <summary>
        /// Suppliers who have pending sale orders
        /// </summary>
        public void SupplierDropdown1()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().SupplierDropdown1(), "Id", "Name");
        }
    }
}