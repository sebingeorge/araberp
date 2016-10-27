﻿using System;
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
    public class GrnWareHouseController : BaseController
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

                    if (list[0].isDirectPurchase) //if direct purchase
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.DirectPurchaseRequestId).ToList();
                        if (id.Count > 0)
                        {
                            List<GRNAddition> Additions = new List<GRNAddition>();
                            Additions.Add(new GRNAddition());

                            List<GRNDeduction> Deductions = new List<GRNDeduction>();
                            Deductions.Add(new GRNDeduction());

                            model.isDirectPurchaseGRN = true;
                            model.Items = new GRNRepository().GetDirectGRNItems(id);
                            model.GRNDate = DateTime.Today;
                            model.Additions = Additions;
                            model.Deductions = Deductions;
                            SupplierDropdown();
                        }
                        else
                        {
                            throw new NullReferenceException("1No purchase requests were selected. Please select atleast one purchase request and try again.");
                        }
                    }
                    else //if not direct purchase
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.SupplyOrderId).ToList();
                        if (id.Count > 0)
                        {
                            List<GRNAddition> Additions = new List<GRNAddition>();
                            Additions.Add(new GRNAddition());

                            List<GRNDeduction> Deductions = new List<GRNDeduction>();
                            Deductions.Add(new GRNDeduction());

                            model = new GRNRepository().GetGRNDetails(list[0].SupplierId);
                            model.GRNDate = DateTime.Today;
                            model.Items = new GRNRepository().GetGRNItem(id);
                            model.Additions = Additions;
                            model.Deductions = Deductions;
                        }
                        else
                        {
                            throw new NullReferenceException("1No supply orders were selected. Please select atleast one supply order and try again.");
                        }
                    }
                }
                FillWarehouse();
                FillCurrency();
                FillAdditionDeduction();
                FillEmployee();

                model.GRNNo = DatabaseCommonRepository.GetNextDocNo(11, OrganizationId);

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
                //if (!ModelState.IsValid)
                //{
                //    return View(model);
                //}

                if (model == null || model.Items == null || model.Items.Count == 0) throw new NullReferenceException();
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string result = new GRNRepository().InsertGRN(model);
                if (result != "")
                {
                    TempData["success"] = "Saved succesfully. The GRN Reference No. is " + result;
                    TempData["error"] = "";
                    if (model.isDirectPurchaseGRN)
                        return RedirectToAction("PendingDirectPurchase");
                    return RedirectToAction("PendingGrnWareHouse");
                }
                else
                    throw new Exception();
            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                if (nx.Message.StartsWith("1"))
                    TempData["error"] = nx.Message.Substring(1);
                else TempData["error"] = "Some required value was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            FillWarehouse();
            FillCurrency();
            SupplierDropdown();
            FillAdditionDeduction();
            FillEmployee();

            PendingForGRN pending = new PendingForGRN();
            if (model.isDirectPurchaseGRN)
                foreach (var item in model.Items)
                    pending.DirectPurchaseRequestId = item.DirectPurchaseRequestId;
            else
                foreach (var item in model.Items)
                    pending.SupplyOrderId = item.SupplyOrderId;

            return View("Create", model);
        }

        public ActionResult Edit(int id = 0)
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request.";
                return RedirectToAction("Index", "Home");
            }
            GRNRepository repo = new GRNRepository();
            FillWarehouse();
            FillCurrency();
            FillEmployee();
            FillAdditionDeduction();

            GRN model = repo.GetGRNDetails(id);
            model.Items = repo.GetGRNItems(id);
            model.Additions = repo.GetGRNAdditions(id);

            if (model.Additions.Count==0)
            model.Additions.Add(new GRNAddition());

            model.Deductions = repo.GetGRNDeductions(id);

            if (model.Deductions.Count==0)
            model.Deductions.Add(new GRNDeduction());

            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(GRN model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillWarehouse();
            FillCurrency();
            SupplierDropdown();
            FillAdditionDeduction();
            FillEmployee();

            var repo = new GRNRepository();
            var result1 = new GRNRepository().CHECK(model.GRNId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["GRNNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result = new GRNRepository().UpdateGRN(model);
                    var result3 = new GRNItemRepository().DeleteGRNADDDED(model.GRNId);
                    var result2 = new GRNItemRepository().DeleteGRNItem(model.GRNId);
                    var result4 = new StockUpdateRepository().DeleteGRNStockUpdate(model.GRNId);
                    var result5 = new GRNRepository().InsertGRNDT(model);
                    if (result5.GRNId > 0)
                    {
                        TempData["success"] = "Updated successfully. The GRN Reference No. is " + result.GRNNo;
                        TempData["GRNNo"] = result.GRNNo;
                        return RedirectToAction("PreviousList");
                        //return View("Edit", model);
                    }
                    else
                    {
                        throw new Exception();
                    }
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
                return RedirectToAction("PreviousList");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new GRNRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["GRNNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result5 = new GRNItemRepository().DeleteGRNADDDED(Id);
                var result2 = new GRNItemRepository().DeleteGRNItem(Id);
                var result4 = new StockUpdateRepository().DeleteGRNStockUpdate(Id);
                var result3 = new GRNRepository().DeleteGRNHD(Id);

                if (Id > 0)
                {
                    TempData["success"] = "Deleted succesfully";
                    return RedirectToAction("PreviousList");

                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["GRNNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

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

        public JsonResult GetSupplierCurrency(int id) //SupplierId is received here
        {
            try
            {
                Supplier model = new SupplierRepository().GetSupplierCurrency(id);
                return Json(new { currencyId = model.CurrencyId, currencyName = model.CurrencyName }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { currencyId = "", currencyName = "" }, JsonRequestBehavior.AllowGet);
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

        public void FillAdditionDeduction()
        {
            ViewBag.additionList = new SelectList(new DropdownRepository().AdditionDropdown(), "Id", "Name");
            ViewBag.deductionList = new SelectList(new DropdownRepository().DeductionDropdown(), "Id", "Name");
        }

        public void FillEmployee()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }

        //public ActionResult Print(int Id)
        //{

        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SupplyOrder.rpt"));

        //    DataSet ds = new DataSet();
        //    ds.Tables.Add("Head");

        //    ds.Tables.Add("Items");

        //    //-------HEAD
        //    ds.Tables["Head"].Columns.Add("SupplyOrderNo");
        //    ds.Tables["Head"].Columns.Add("SupplyOrderDate");
        //    ds.Tables["Head"].Columns.Add("QuotaionNoAndDate");
        //    ds.Tables["Head"].Columns.Add("SpecialRemarks");
        //    ds.Tables["Head"].Columns.Add("PaymentTerms");
        //    ds.Tables["Head"].Columns.Add("DeliveryTerms");
        //    ds.Tables["Head"].Columns.Add("RequiredDate");
        //    ds.Tables["Head"].Columns.Add("CurrencyName");
        //    ds.Tables["Head"].Columns.Add("EmployeeName");
        //    ds.Tables["Head"].Columns.Add("CountryName");
        //    //Organization
        //    ds.Tables["Head"].Columns.Add("DoorNo");
        //    ds.Tables["Head"].Columns.Add("Street");
        //    ds.Tables["Head"].Columns.Add("State");
        //    ds.Tables["Head"].Columns.Add("Country");
        //    ds.Tables["Head"].Columns.Add("Phone");
        //    ds.Tables["Head"].Columns.Add("Fax");
        //    ds.Tables["Head"].Columns.Add("Email");
        //    ds.Tables["Head"].Columns.Add("ContactPerson");
        //    ds.Tables["Head"].Columns.Add("Zip");
        //    ds.Tables["Head"].Columns.Add("OrganizationName");
        //    ds.Tables["Head"].Columns.Add("Image1");
        //    ds.Tables["Head"].Columns.Add("SupplierName");

        //    ds.Tables["Head"].Columns.Add("SupDoorNo");
        //    ds.Tables["Head"].Columns.Add("SupState");
        //    ds.Tables["Head"].Columns.Add("SupCountryName");
        //    ds.Tables["Head"].Columns.Add("SupPhone");
        //    ds.Tables["Head"].Columns.Add("SupFax");
        //    ds.Tables["Head"].Columns.Add("SupEmail");
        //    ds.Tables["Head"].Columns.Add("SupPostBoxNo");



        //    //-------DT
        //    ds.Tables["Items"].Columns.Add("PRNODATE");
        //    ds.Tables["Items"].Columns.Add("ItemName");
        //    ds.Tables["Items"].Columns.Add("BalQty");
        //    ds.Tables["Items"].Columns.Add("OrderedQty");
        //    ds.Tables["Items"].Columns.Add("Rate");
        //    ds.Tables["Items"].Columns.Add("Discount");
        //    ds.Tables["Items"].Columns.Add("Amount");
        //    ds.Tables["Items"].Columns.Add("ItemRefNo");
        //    ds.Tables["Items"].Columns.Add("UnitName");



        //    GRNRepository repo = new GRNRepository();
        //    var Head = repo.GetGRNDetailsHDPrint(Id, OrganizationId);

        //    DataRow dr = ds.Tables["Head"].NewRow();
        //    dr["SupplyOrderNo"] = Head.SupplyOrderNo;
        //    dr["SupplyOrderDate"] = Head.SupplyOrderDate.ToString("dd-MMM-yyyy");
        //    dr["SupplierName"] = Head.SupplierName;
        //    dr["QuotaionNoAndDate"] = Head.QuotaionNoAndDate;
        //    dr["SpecialRemarks"] = Head.SpecialRemarks;
        //    dr["PaymentTerms"] = Head.PaymentTerms;
        //    dr["DeliveryTerms"] = Head.DeliveryTerms;
        //    dr["RequiredDate"] = Head.RequiredDate;
        //    dr["CurrencyName"] = Head.CurrencyName;
        //    dr["EmployeeName"] = Head.EmployeeName;
        //    dr["CountryName"] = Head.CountryName;
        //    // dr["CurrencyName"] = Head.CurrencyName;

        //    dr["DoorNo"] = Head.DoorNo;
        //    dr["Street"] = Head.Street;
        //    dr["State"] = Head.State;
        //    dr["Country"] = Head.CountryName;
        //    dr["Phone"] = Head.Phone;
        //    dr["Fax"] = Head.Fax;
        //    dr["Email"] = Head.Email;
        //    dr["ContactPerson"] = Head.ContactPerson;
        //    dr["Zip"] = Head.Zip;
        //    dr["CurrencyName"] = Head.CurrencyName;

        //    dr["SupDoorNo"] = Head.SupDoorNo;
        //    dr["SupState"] = Head.SupState;
        //    dr["SupCountryName"] = Head.SupCountryName;
        //    dr["SupPhone"] = Head.SupPhone;
        //    dr["SupFax"] = Head.SupFax;
        //    dr["SupEmail"] = Head.SupEmail;
        //    dr["SupPostBoxNo"] = Head.SupPostBoxNo;

        //    dr["OrganizationName"] = Head.OrganizationName;
        //    dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
        //    ds.Tables["Head"].Rows.Add(dr);

        //    SupplyOrderItemRepository repo1 = new SupplyOrderItemRepository();
        //    var Items = repo1.GetSupplyOrderItemsDTPrint(Id);
        //    foreach (var item in Items)
        //    {
        //        var pritem = new SupplyOrderItem
        //        {
        //            PRNODATE = item.PRNODATE,
        //            ItemName = item.ItemName,
        //            BalQty = item.BalQty,
        //            OrderedQty = item.OrderedQty,
        //            Rate = item.Rate,
        //            Discount = item.Discount,
        //            Amount = item.Amount,
        //            ItemRefNo = item.ItemRefNo,
        //            UnitName = item.UnitName


        //        };


        //        DataRow dri = ds.Tables["Items"].NewRow();
        //        dri["PRNODATE"] = pritem.PRNODATE;
        //        dri["ItemName"] = pritem.ItemName;
        //        dri["BalQty"] = pritem.BalQty;
        //        dri["OrderedQty"] = pritem.OrderedQty;
        //        dri["Rate"] = pritem.Rate;
        //        dri["Discount"] = pritem.Discount;
        //        dri["Amount"] = pritem.Amount;
        //        dri["ItemRefNo"] = pritem.ItemRefNo;
        //        dri["UnitName"] = pritem.UnitName;

        //        ds.Tables["Items"].Rows.Add(dri);
        //    }

        //    ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SupplyOrder.xml"), XmlWriteMode.WriteSchema);

        //    rd.SetDataSource(ds);

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();


        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf", String.Format("SupplyOrder{0}.pdf", Id.ToString()));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }
}