using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;


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
                    supplyorder.SupplyOrderItems = rep.GetPurchaseRequestItems(selectedpurchaserequests, OrganizationId);
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
            int id = new SupplyOrderRepository().Approve(model.SupplyOrderId, UserID);
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
                TempData["error"] = "Sorry! Already Used.";
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

                    TempData["error"] = "Some error occurred. Please tr again.";
                    TempData["ExpenseNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }
        //-------PRINT
        public ActionResult PurchaseOrder(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SupplyOrder.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("SupplyOrderNo");
            ds.Tables["Head"].Columns.Add("SupplyOrderDate");
            ds.Tables["Head"].Columns.Add("QuotaionNoAndDate");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("DeliveryTerms");
            ds.Tables["Head"].Columns.Add("RequiredDate");
            ds.Tables["Head"].Columns.Add("CurrencyName");
            ds.Tables["Head"].Columns.Add("CreatedUser");
            ds.Tables["Head"].Columns.Add("CreateSignature");
            ds.Tables["Head"].Columns.Add("ApprovedUser");
            ds.Tables["Head"].Columns.Add("ApproveSignature");
            ds.Tables["Head"].Columns.Add("CountryName");
            //Organization
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("Country");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("SupplierName");

            ds.Tables["Head"].Columns.Add("SupDoorNo");
            ds.Tables["Head"].Columns.Add("SupState");
            ds.Tables["Head"].Columns.Add("SupCountryName");
            ds.Tables["Head"].Columns.Add("SupPhone");
            ds.Tables["Head"].Columns.Add("SupFax");
            ds.Tables["Head"].Columns.Add("SupEmail");
            ds.Tables["Head"].Columns.Add("SupPostBoxNo");
            ds.Tables["Head"].Columns.Add("CreatedDes");
            ds.Tables["Head"].Columns.Add("ApproveDes");
            ds.Tables["Head"].Columns.Add("NetDiscount");
            ds.Tables["Head"].Columns.Add("NetAmount");


            //-------DT
            ds.Tables["Items"].Columns.Add("PRNODATE");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("BalQty");
            ds.Tables["Items"].Columns.Add("OrderedQty");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("Discount");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("ItemRefNo");
            ds.Tables["Items"].Columns.Add("UnitName");
            ds.Tables["Items"].Columns.Add("Description");



            SupplyOrderRepository repo = new SupplyOrderRepository();
            var Head = repo.GetSupplyOrderHDprint(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["SupplyOrderNo"] = Head.SupplyOrderNo;
            dr["SupplyOrderDate"] = Head.SupplyOrderDate.ToString("dd-MMM-yyyy");
            dr["SupplierName"] = Head.SupplierName;
            dr["QuotaionNoAndDate"] = Head.QuotaionNoAndDate;
            dr["SpecialRemarks"] = Head.SpecialRemarks;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["DeliveryTerms"] = Head.DeliveryTerms;
            dr["RequiredDate"] = Head.RequiredDate;
            dr["CurrencyName"] = Head.CurrencyName;
            dr["CreatedUser"] = Head.CreatedUser;
            dr["CreateSignature"] = Server.MapPath("~/App_Images/") + Head.CreatedUsersig;
            dr["ApprovedUser"] = Head.ApprovedUser;
            dr["ApproveSignature"] = Server.MapPath("~/App_Images/") + Head.ApprovedUsersig;
            dr["CreatedDes"] = Head.CreatedDes;
            dr["ApproveDes"] = Head.ApprovedDes;
            dr["CountryName"] = Head.CountryName;
            // dr["CurrencyName"] = Head.CurrencyName;

            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["Country"] = Head.CountryName;
            dr["Phone"] = Head.Phone;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Zip"] = Head.Zip;
            dr["CurrencyName"] = Head.CurrencyName;

            dr["SupDoorNo"] = Head.SupDoorNo;
            dr["SupState"] = Head.SupState;
            dr["SupCountryName"] = Head.SupCountryName;
            dr["SupPhone"] = Head.SupPhone;
            dr["SupFax"] = Head.SupFax;
            dr["SupEmail"] = Head.SupEmail;
            dr["SupPostBoxNo"] = Head.SupPostBoxNo;
            dr["NetDiscount"] = Head.NetDiscount;
            dr["NetAmount"] = Head.NetAmount;

            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            SupplyOrderItemRepository repo1 = new SupplyOrderItemRepository();
            var Items = repo1.GetSupplyOrderItemsDTPrint(Id);
            foreach (var item in Items)
            {
                var pritem = new SupplyOrderItem
                {
                    PRNODATE = item.PRNODATE,
                    PartNo = item.PartNo,
                    ItemName = item.ItemName,
                    BalQty = item.BalQty,
                    OrderedQty = item.OrderedQty,
                    Rate = item.Rate,
                    Discount = item.Discount,
                    Amount = item.Amount,
                    ItemRefNo = item.ItemRefNo,
                    UnitName = item.UnitName,
                    Description = item.Description


                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["PRNODATE"] = pritem.PRNODATE;
                dri["PartNo"] = pritem.PartNo;
                dri["ItemName"] = pritem.ItemName;
                dri["BalQty"] = pritem.BalQty;
                dri["OrderedQty"] = pritem.OrderedQty;
                dri["Rate"] = pritem.Rate;
                dri["Discount"] = pritem.Discount;
                dri["Amount"] = pritem.Amount;
                dri["ItemRefNo"] = pritem.ItemRefNo;
                dri["UnitName"] = pritem.UnitName;
                dri["Description"] = pritem.Description;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SupplyOrder.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private JsonResult GetLastSupplyOrderRate(int itemId)
        {
            var data = new SupplyOrderRepository().GetLastSupplyOrderRate(itemId, OrganizationId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApprovalCancellation()
        {
            return View();
        }

        public PartialViewResult ApprovalCancellationGrid(string Supplier = "", string LPO = "")
        {
            try
            {
                return PartialView("_ApprovalCancellationGrid", new SupplyOrderRepository().ApprovalList(Supplier, LPO));
            }
            catch (Exception)
            {
                return PartialView("_ApprovalCancellationGrid", new List<PendingForGRN>());
            }
        }
        public ActionResult ApprovalChange(int id = 0)
        {
            SupplyOrder model = new SupplyOrder();
            //model.IsApprovedDate = System.DateTime.Now;
            //model.IsApprovedBy = UserID;
            var repo = new SupplyOrderRepository();

            new SupplyOrderRepository().Approvalcancel(id);
            TempData["success"] = "Approval cancelled successfully";
            return RedirectToAction("ApprovalCancellation");
        }

        public ActionResult PendingSOSettle()
        {
            return View();
        }
        public PartialViewResult PendingSOSettlement(string Supplier = "", string LPO = "", string Item = "", string PartNo = "")
        {
            return PartialView(new SupplyOrderRepository().GetPendingSOSettlement(Supplier, LPO, Item, PartNo));
        }
        public ActionResult Settle(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    SupplyOrderPreviousList supplyorder = new SupplyOrderPreviousList();
                    supplyorder = new SupplyOrderRepository().GetSOSettlement(id);
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
            return RedirectToAction("PendingSOSettle");
        }
        [HttpPost]
        public ActionResult Settle(SupplyOrderPreviousList model)
        {
            model.SettledDate = System.DateTime.Now;
            model.SettledBy = UserID.ToString();
            try
            {
                string id = new SupplyOrderRepository().SOSettlement(model);

                TempData["success"] = "Settled successfully. Supply Order LPO.No. is " + id;
                TempData["error"] = "";
                return RedirectToAction("PendingSOSettle");
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
            return View("Settle", model.SupplyOrderItemId);
        }
    }
}