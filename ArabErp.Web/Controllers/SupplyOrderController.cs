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
            FillSORefNo();
            FillSOSupplier();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int supid = 0)
        {
            return PartialView("_PreviousList", new SupplyOrderRepository().GetPreviousList(OrganizationId: OrganizationId, id: id, supid: supid, from: from, to: to));
        }



        public ActionResult Create(IList<PendingPurchaseRequest> PendingPurchaseRequestItemsSelected)
        {
            FillDropdowns();

            SupplyOrder supplyorder = new SupplyOrder();

            supplyorder.SupplyOrderNo = DatabaseCommonRepository.GetNextDocNo(9, OrganizationId);

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
            supplyorder.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;
            return View(supplyorder);
        }
        //public PartialViewResult grid(IList<PendingPurchaseRequest> PendingPurchaseRequestItemsSelected, int Id = 0)
        //{
        //    SupplyOrder supplyorder = new SupplyOrder();
        //    SupplyOrderRepository rep = new SupplyOrderRepository();
        //    if (PendingPurchaseRequestItemsSelected != null)
        //    {
        //        if (PendingPurchaseRequestItemsSelected.Count > 0)
        //        {
        //            List<int> selectedpurchaserequests = (from PendingPurchaseRequest p in PendingPurchaseRequestItemsSelected
        //                                                  where p.Select
        //                                                  select p.PurchaseRequestId).ToList<int>();
        //            supplyorder.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests, Id);
        //        }
        //    }
        //    FillCurrency();
        //    return PartialView("_grid", supplyorder);

        //}


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
                if (!ModelState.IsValid)
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                }
                else
                {
                    model.OrganizationId = OrganizationId;
                    model.CreatedDate = System.DateTime.Now;
                    model.CreatedBy = UserID.ToString();
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

        public void FillSORefNo()
        {
            ViewBag.SONoList = new SelectList(new DropdownRepository().SORefNoDropdown(), "Id", "Name");
        }
        public void FillSOSupplier()
        {
            ViewBag.SupplierList = new SelectList(new DropdownRepository().SupplyOrderSupplierDropdown(), "Id", "Name");
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
        [HttpGet]
        public JsonResult GetSupplierItemRateSettings(int Id, int ItemId)
        {
            SupplyOrderItem List = new SupplyOrderRepository().GetSupplierItemRate(Id, ItemId);
            var Result = new { Success = true, ItemId = List.ItemId, FixedRate = List.FixedRate };
            return Json(Result, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetPaymentTerm(int supplierid)
        {
            string PaymentTerms = (new SupplyOrderRepository()).GetPaymentTerm(supplierid);
            return Json(new { Success = true, PaymentTerms = PaymentTerms }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id = 0)
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
            return RedirectToAction("PendingSupplyOrder");
        }

        [HttpPost]
        public ActionResult Edit(SupplyOrder model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillDropdowns();

            var repo = new SupplyOrderRepository();

            var result1 = new SupplyOrderRepository().CHECK(model.SupplyOrderId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Approved!!";
                TempData["ExpenseNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result2 = new SupplyOrderRepository().DeleteSODT(model.SupplyOrderId);
                    string id = new SupplyOrderRepository().UpdateSOHD(model);
                    //var result3 = new SupplyOrderRepository().DeleteSOHD(model.SupplyOrderId);
                    string id1 = new SupplyOrderRepository().InsertSODT(model);

                    TempData["success"] = "Updated successfully. Purchase Request Reference No. is " + id;
                    TempData["error"] = "";
                    return RedirectToAction("Index");
                }
                catch (SqlException sx)
                {
                    TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
                }
                catch (NullReferenceException nx)
                {
                    TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
                }
                return View("Edit", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new SupplyOrderRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["ExpenseNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result2 = new SupplyOrderRepository().DeleteSODT(Id);
                var result3 = new SupplyOrderRepository().DeleteSOHD(Id);

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("PendingSupplyOrder");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["ExpenseNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }
    }
}