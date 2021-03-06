﻿using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class SalesInvoiceController : BaseController
    {
        SalesInvoiceRepository Repo = new SalesInvoiceRepository();
        // GET: SalesInvoice
        public ActionResult Index(string type)
        {
            ViewBag.type = type;
            FillSalesInvoice(type);
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PreviousList(string type, DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PreviousListGrid", new SalesInvoiceRepository().PreviousList(type: type, from: from, to: to, id: id, OrganizationId: OrganizationId));
        }

        public ActionResult PendingSalesInvoice(string invType, string InstallType = "all")

        {

            //ViewBag.saleOrderList = new SelectList(Repo.GetSalesInvoiceCustomerList(invType, OrganizationId), "SaleOrderId", "SaleOrderRefNoWithDate");
            //var List = Repo.GetSalesInvoiceCustomerList(invType);
            return View("PendingSalesInvoice");

        }
        public ActionResult PendingSalesInvoiceDt(int SalesOrderId = 0, string invType = "", string DeliveryNo = "", string CustomerName = "", string RegNo = "", string InstallType = "all")
        {

            var List = Repo.GetPendingSalesInvoiceList(SalesOrderId, invType, DeliveryNo, CustomerName, RegNo, InstallType);
            foreach (var item in List)
            {
                item.invType = invType;

            }


            return PartialView("_PendingSalesInvoiceList", List);

        }

        public JsonResult GetDueDate(DateTime date, int SaleOrderId)
        {
            DateTime duedate = (new SalesInvoiceRepository()).GetDueDate(date, SaleOrderId);
            return Json(duedate.ToString("dd/MMMM/yyyy"), JsonRequestBehavior.AllowGet);
        }

        public void FillSalesInvoice(string type)
        {
            ViewBag.salesInvoiceList = new SelectList(new DropdownRepository().SalesInvoiceDropdown(OrganizationId, type), "Id", "Name");
        }


        public ActionResult Create(List<SalesInvoiceItem> ObjSaleInvoiceItem)
        {
            ObjSaleInvoiceItem = ObjSaleInvoiceItem.Where(x => x.SelectStatus).Select(x => x).ToList();
            SalesInvoice saleinvoice = new SalesInvoice();
            SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
            SalesInvoiceItemRepository SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
            if (ObjSaleInvoiceItem.Count > 0)
            {
                List<int> SelectedSaleOrderItemId = (from SalesInvoiceItem s in ObjSaleInvoiceItem
                                                     where s.SelectStatus
                                                     select s.SaleOrderItemId).ToList<int>();
                saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(ObjSaleInvoiceItem[0].SaleOrderId, ObjSaleInvoiceItem[0].invType);
                saleinvoice.InvoiceType = ObjSaleInvoiceItem[0].invType;

                string internalId = "";
                if (saleinvoice.InvoiceType == "Inter")
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(44, OrganizationId);
                }
                else
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(7, OrganizationId);
                }
                saleinvoice.SalesInvoiceDate = System.DateTime.Today;
                saleinvoice.SalesInvoiceRefNo = internalId;

                saleinvoice.SaleInvoiceItems = SalesInvoiceItemRepo.GetSelectedSalesInvoiceDT(SelectedSaleOrderItemId, ObjSaleInvoiceItem[0].SaleOrderId);

                //SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
                //SalesInvoice saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(SelectedSaleOrderItemId);
                //int deliveryChallanId = new DeliveryChallanRepository().GetDeliveryChallanIdFromJobCardId()
                //saleinvoice.PrintDescriptions = new SalesInvoiceRepository().GetPrintDescriptions(SelectedSaleOrderItemId);
                saleinvoice.PrintDescriptions = new List<PrintDescription>();
                var PrintDescriptionsFromDB = new SalesInvoiceRepository().GetPrintDescriptions(SelectedSaleOrderItemId);

                //combining same print descriptions
                foreach (var item in PrintDescriptionsFromDB)
                {
                    if (!saleinvoice.PrintDescriptions.Select(x => x.Description.Trim()).Contains(item.Description.Trim()))
                    {
                        item.Quantity = PrintDescriptionsFromDB.Where(x => x.Description.Trim() == item.Description.Trim()).Count();
                        item.Amount = (item.Quantity ?? 0) * item.PriceEach;
                        saleinvoice.PrintDescriptions.Add(item);
                    }
                }
                //
            }
            if (saleinvoice.InvoiceType == "Inter")
            {
                saleinvoice.isProjectBased = 1;
                if (saleinvoice.PrintDescriptions == null || saleinvoice.PrintDescriptions.Count == 0)
                    saleinvoice.PrintDescriptions.Add(new PrintDescription());
            }
            else if (saleinvoice.InvoiceType == "Final")
            {
                saleinvoice.isProjectBased = 0;
            }
            return View("Create", saleinvoice);
        }

        [HttpPost]
        public ActionResult Save(SalesInvoice model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();

            SalesInvoice Result = SalesInvoiceRepo.InsertSalesInvoice(model);


            if (Result.SalesInvoiceId > 0)
            {
                TempData["success"] = "Saved successfully (" + model.SalesInvoiceRefNo + ")";
                TempData["error"] = null;

                TempData["SalesInvoiceRefNo"] = null;
                return RedirectToAction("PendingSalesInvoice", new { invType = model.InvoiceType });
            }
            else
            {
                TempData["success"] = null;
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }


        }

        public ActionResult Edit(int id, string type)
        {
            try
            {
                if (id != 0)
                {
                    SalesInvoice saleinvoice = new SalesInvoice();
                    saleinvoice = new SalesInvoiceRepository().GetInvoiceHd(id, type);
                    saleinvoice.SaleInvoiceItems = new SalesInvoiceRepository().GetInvoiceItems(id);
                    saleinvoice.InvoiceType = type;
                    List<int> saleOrderItemIds = (from p in saleinvoice.SaleInvoiceItems select p.SaleOrderItemId).ToList();
                    if (saleinvoice.InvoiceType == "Inter")
                        saleinvoice.PrintDescriptions = new SalesInvoiceRepository().GetPrintDescriptions(saleOrderItemIds);
                    else
                        saleinvoice.PrintDescriptions = new SalesInvoiceRepository().GetPrintDescriptionsInvoice(saleinvoice.SalesInvoiceId);//print descriptions for inter invoice
                    return View(saleinvoice);
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(SalesInvoice model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            try
            {

                if (model.SaleInvoiceItems != null && model.SaleInvoiceItems.Count > 0)
                {
                    new SalesInvoiceItemRepository().DeleteSalesInvoiceItem(model.SalesInvoiceId);
                }

                new SalesInvoiceRepository().UpdateSalesInvoice(model);

                TempData["success"] = "Updated Successfully (" + model.SalesInvoiceRefNo + ")";
                TempData["SalesInvoiceRefNo"] = model.SalesInvoiceRefNo;
                return RedirectToAction("Index", new { type = model.InvoiceType });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }
        public ActionResult PendingApproval()
        {
            return View("PendingApproval", new SalesInvoiceRepository().ApprovalList(OrganizationId: OrganizationId));
        }
        public ActionResult Approval(int? id, string type)
        {
            //    FillCustomer();
            //    FillCurrency();
            //    FillCommissionAgent();
            //    FillUnit();
            //    FillEmployee();

            //    FillVehicle();
            var repo = new SalesInvoiceRepository();
            SalesInvoice model = repo.GetInvoiceHd(id ?? 0, type);
            model.SaleInvoiceItems = repo.GetInvoiceItems(id ?? 0);

            //var saleinvoice = new SalesInvoiceRepository().GetInvoiceHd(id, type);
            //model.Items = repo.GetSaleOrderItem(SaleOrderId ?? 0);

            //FillWrkDesc();
            return View("Approval", model);
        }

        //public ActionResult UpdateApprovalStatus(int? id, string type)
        //{
        //    var repo = new SalesInvoiceRepository();
        //    SalesInvoice si = (new SalesInvoiceRepository()).GetInvoiceHd(id ?? 0, type);
        //    new SalesInvoiceRepository().UpdateSIApproval(id ?? 0);
        //    return RedirectToAction("PendingApproval");
        //}

        public ActionResult UpdateApprovalStatus(int? id, string type)
        {
            SalesInvoice model = new SalesInvoice();
            model.IsApprovedDate = System.DateTime.Now;
            model.IsApprovedBy = UserID;
            var repo = new SalesInvoiceRepository();
            SalesInvoice si = (new SalesInvoiceRepository()).GetInvoiceHd(id ?? 0, type);
            new SalesInvoiceRepository().UpdateSIApproval(id ?? 0, model.IsApprovedDate, model.IsApprovedBy);
            TempData["success"] = "Approved successfully";
            return RedirectToAction("PendingApproval");
        }

        public ActionResult InvoiceReport(int Id)
        {

            ReportDocument rd = new ReportDocument();
            //rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesInvoice.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");
            ds.Tables.Add("DeliveryChallans");

            #region creating Head table
            ds.Tables["Head"].Columns.Add("SalesInvoiceRefNo");
            ds.Tables["Head"].Columns.Add("SalesInvoiceDate");
            ds.Tables["Head"].Columns.Add("VehicleOutPassNo");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("Address");
            ds.Tables["Head"].Columns.Add("CustomerOrderRef");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("RegistrationNo");
            ds.Tables["Head"].Columns.Add("JobCardNo");
            ds.Tables["Head"].Columns.Add("TotalAmount");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("OrganizationRefNo");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("Country");
            ds.Tables["Head"].Columns.Add("Currency");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("CreateUser");
            ds.Tables["Head"].Columns.Add("CreateSig");
            ds.Tables["Head"].Columns.Add("CreatedDes");
            ds.Tables["Head"].Columns.Add("ApproveUser");
            ds.Tables["Head"].Columns.Add("ApproveSig");
            ds.Tables["Head"].Columns.Add("ApproveDes");
            ds.Tables["Head"].Columns.Add("JobCardNum");
            #endregion

            #region creating Item Table
            ds.Tables["Items"].Columns.Add("PrintDescription");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("UoM");
            ds.Tables["Items"].Columns.Add("PriceEach");
            ds.Tables["Items"].Columns.Add("Amount");
            #endregion

            #region creating DeliveryChallans Table
            ds.Tables["DeliveryChallans"].Columns.Add("DeliveryChallanRefNo");
            ds.Tables["DeliveryChallans"].Columns.Add("JobCardNo");
            ds.Tables["DeliveryChallans"].Columns.Add("Chassiss_RegNo");
            #endregion

            #region store data to Head table
            SalesInvoiceRepository repo = new SalesInvoiceRepository();
            var Head = repo.GetSalesInvoiceHdforPrint(Id, OrganizationId);
            DataRow dr = ds.Tables["Head"].NewRow();
            dr["SalesInvoiceRefNo"] = Head.SalesInvoiceRefNo;
            dr["SalesInvoiceDate"] = Head.SalesInvoiceDate.ToString("dd-MMM-yyyy");
            dr["VehicleOutPassNo"] = Head.DeliveryChallanRefNo;
            dr["CustomerName"] = Head.Customer;
            dr["Address"] = Head.CustomerAddress;
            dr["CustomerOrderRef"] = Head.CustomerOrderRef;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["RegistrationNo"] = Head.RegistrationNo;
            dr["JobCardNo"] = Head.JobCardNo;
            dr["TotalAmount"] = Head.TotalAmount;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["OrganizationRefNo"] = Head.OrganizationRefNo;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["Country"] = Head.CountryName;
            dr["Currency"] = Head.CurrencyName;
            dr["Phone"] = Head.Phone;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Zip"] = Head.Zip;
            dr["CreateUser"] = Head.CreateUser;
            dr["CreateSig"] = Server.MapPath("~/App_images/") + Head.CreateSig;
            dr["CreatedDes"] = Head.CreatedDes;
            dr["ApproveUser"] = Head.ApproveUser;
            dr["ApproveSig"] = Server.MapPath("~/App_images/") + Head.ApproveSig;
            dr["ApproveDes"] = Head.ApprovedDes;
            dr["JobCardNum"] = Head.JobCardNum;
            ds.Tables["Head"].Rows.Add(dr);
            #endregion

            #region store data to Items Table
            SalesInvoiceItemRepository repo1 = new SalesInvoiceItemRepository();
            var Items = repo1.GetSalesInvoiceItemforPrint(Id);
            foreach (var item in Items)
            {
                //var InvItem = new SalesInvoiceItem { Quantity = item.Quantity, Rate = item.Rate, Amount = item.Amount, Unit = item.Unit, WorkDescription = item.WorkDescription, WorkDescriptionRefNo = item.WorkDescriptionRefNo };
                DataRow dri = ds.Tables["Items"].NewRow();
                dri["PrintDescription"] = item.Description;
                dri["Quantity"] = item.Quantity;
                dri["PriceEach"] = item.PriceEach;
                dri["Amount"] = item.Amount;
                dri["UoM"] = item.UoM;

                ds.Tables["Items"].Rows.Add(dri);
            }
            #endregion

            #region store data to DeliveryChallans table
            if (Head.InvoiceType == "Final")
            {

                var list = new SalesInvoiceRepository().GetDeliveryChallansFromInvoice(Id);
                foreach (var item in list)
                {
                    dr = ds.Tables["DeliveryChallans"].NewRow();
                    dr["DeliveryChallanRefNo"] = item.DeliveryChallanRefNo;
                    dr["JobCardNo"] = item.JobCardNo;
                    dr["Chassiss_RegNo"] = item.ChassisNo + (item.ChassisNo != "" && item.RegistrationNo != "" ? " - " : "") + item.RegistrationNo;
                    ds.Tables["DeliveryChallans"].Rows.Add(dr);
                }
            }
            #endregion
            if (Head.InvoiceType == "Final")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesInvoice.rpt"));
                ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesInvoice.xml"), XmlWriteMode.WriteSchema);
            }
            else
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "InterInvoice.rpt"));
                ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "InterInvoice.xml"), XmlWriteMode.WriteSchema);
            }


            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");//, String.Format("SalesInvoice{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult Delete(int Id, string type)
        {
            try
            {
                if (Id == 0) return RedirectToAction("PendingSalesInvoice", new { invType = type });
                string ref_no = new SalesInvoiceRepository().DeleteInvoice(Id);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("PendingSalesInvoice", new { invType = type });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = Id });
            }
        }

        public ActionResult getMaterialCost(int id)//JobCardId is received here
        {
            var list = new SalesInvoiceRepository().getMaterialCost(id);
            if (list == null) list = new List<MaterialCostForService>();
            return PartialView("_MaterialCostGrid", list);//, new SalesInvoiceRepository().getMaterialCost(id));
        }

        public ActionResult getLabourCost(int id)
        {
            var list = new SalesInvoiceRepository().getLabourCost(id);
            if (list == null) list = new List<LabourCostForService>();
            return PartialView("_LabourCostGrid", list);
        }

    }
}