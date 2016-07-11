using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class SupplyOrderController : BaseController
    {
        // GET: SupplyOrder
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(IList<PendingPurchaseRequest> PendingPurchaseRequestItemsSelected)
        {
            SupplyOrder supplyorder = new SupplyOrder();

            supplyorder.SupplyOrderNo = "LPO/" + DatabaseCommonRepository.GetNextReferenceNo(typeof(SupplyOrder).Name);

            SupplyOrderRepository rep = new SupplyOrderRepository();
            if (PendingPurchaseRequestItemsSelected != null)
            {
                if (PendingPurchaseRequestItemsSelected.Count > 0)
                {
                    List<int> selectedpurchaserequests = (from PendingPurchaseRequest p in PendingPurchaseRequestItemsSelected
                                                          where p.Select
                                                          select p.PurchaseRequestId).ToList<int>();
                    supplyorder.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests);
                }
            }
            supplyorder.SupplyOrderDate = System.DateTime.Today;
            supplyorder.RequiredDate = System.DateTime.Today;

            FillDropdowns();

            return View(supplyorder);
        }
        public ActionResult PendingSupplyOrder()
        {

            SupplyOrderRepository rep = new SupplyOrderRepository();

            IEnumerable<PendingPurchaseRequest> model = rep.GetPendingPurchaseRequest();

            return View(model);
        }

        public void FillSupplier()
        {
            var repo = new SupplierRepository();
            List<Dropdown> list = repo.FillSupplier();
            ViewBag.SupplierList = new SelectList(list, "Id", "Name");
        }

        [HttpPost]
        public ActionResult Save(SupplyOrder model)
        {
            try
            {
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                string referenceNo = new SupplyOrderRepository().InsertSupplyOrder(model);
                if (referenceNo != "")
                {
                    TempData["error"] = "";
                    TempData["success"] = "Saved successfully. Reference No. is " + referenceNo;
                    return RedirectToAction("PendingSupplyOrder");
                }
                else
                {
                    TempData["error"] = "Some error occured while saving. Please try again.";
                    TempData["success"] = "";
                }
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                TempData["success"] = "";
            }
            FillDropdowns();
            return View("Create", model);
        }

        public ActionResult LocalSupplyOrder()
        {
            FillSupplier();
            GetMaterialDropdown();
            List<SupplyOrderItem> list = new List<SupplyOrderItem>();
            list.Add(new SupplyOrderItem());
            return View("CreateLocalSupplyOrder", new SupplyOrder { SupplyOrderItems = list, SupplyOrderDate = DateTime.Now, RequiredDate = DateTime.Now });
        }
        [HttpPost]
        public ActionResult LocalSupplyOrder(SupplyOrder model)
        {
            return View("LocalSupplyOrder");
        }

        private void GetMaterialDropdown()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }

        public ActionResult PreviousList()
        {
            try
            {

                return View(new SupplyOrderRepository().GetPreviousList());

            }
            catch (Exception)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while retreiving the previous list. Please try again.";
                return View(new SupplyOrderPreviousList());
            }
        }

        public void FillCurrency()
        {
            ViewBag.currencyList = new SelectList(new DropdownRepository().CurrencyDropdown(), "Id", "Name");
        }

        public void FillDropdowns()
        {
            FillSupplier();
            FillCurrency();
        }

        public ActionResult PendingApproval()
        {
            return View(new SupplyOrderRepository().GetPendingApproval());
        }

        public ActionResult Approve(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    SupplyOrder supplyorder = new SupplyOrder();
                    supplyorder = new SupplyOrderRepository().GetSupplyOrder(id);
                    supplyorder.SupplyOrderItems = new SupplyOrderItemRepository().GetSupplyOrderItems(id);
                    FillDropdowns();
                    return View(supplyorder);
                }
                else
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    TempData["success"] = "";
                }
            }
            catch (InvalidOperationException iox)
            {
                TempData["error"] = "Sorry, we could not find the requested item. Please try again.|" + iox.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please try again after sometime.|" + sx.Message;
            }
            catch (NullReferenceException nx)
            {
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }

            TempData["success"] = "";
            return RedirectToAction("PendingApproval");
        }
        [HttpPost]
        public ActionResult Approve(SupplyOrder model)
        {
            int id = new SupplyOrderRepository().Approve(model.SupplyOrderId);
            if (id > 0)
            {
                TempData["success"] = "Approved successfully";
                TempData["error"] = "";
                return RedirectToAction("PendingApproval");
            }
            else
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while approving the order. Please try again.";
                return View(model);
            }
        }
    }
}