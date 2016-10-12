using ArabErp.DAL;
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
            return View();
        }

        public ActionResult PreviousList(string type, DateTime? from, DateTime? to, int id = 0)
        {
            return PartialView("_PreviousListGrid", new SalesInvoiceRepository().PreviousList(type: type, from: from, to: to, id: id, OrganizationId: OrganizationId));
        }
     
        public ActionResult PendingSalesInvoice(string invType)
        {
            ViewBag.saleOrderList = new SelectList(Repo.GetSalesInvoiceCustomerList(invType), "SaleOrderId", "SaleOrderRefNoWithDate");
            //var List = Repo.GetSalesInvoiceCustomerList(invType);
            return View("PendingSalesInvoice");

        }
        public ActionResult PendingSalesInvoiceDt(int SalesOrderId, string Customer, string SaleOrderRefNoWithDate, string invType)
        {
            ViewBag.CustomerName = Customer;
            ViewBag.SaleOrderRefNoWithDate = SaleOrderRefNoWithDate;
            var List = Repo.GetPendingSalesInvoiceList(SalesOrderId, invType);
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
            SalesInvoice saleinvoice = new SalesInvoice();
            SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
            SalesInvoiceItemRepository SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
            if (ObjSaleInvoiceItem.Count > 0)
            {

                saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(ObjSaleInvoiceItem[0].SaleOrderId, ObjSaleInvoiceItem[0].invType);
                saleinvoice.InvoiceType = ObjSaleInvoiceItem[0].invType;

                string internalId = "";
                internalId = DatabaseCommonRepository.GetNextDocNo(7, OrganizationId);
                saleinvoice.SalesInvoiceDate = System.DateTime.Today;
                saleinvoice.SalesInvoiceRefNo = internalId;
                List<int> SelectedSaleOrderItemId = (from SalesInvoiceItem s in ObjSaleInvoiceItem
                                                     where s.SelectStatus
                                                     select s.SaleOrderItemId).ToList<int>();

                saleinvoice.SaleInvoiceItems = SalesInvoiceItemRepo.GetSelectedSalesInvoiceDT(SelectedSaleOrderItemId, ObjSaleInvoiceItem[0].SaleOrderId);

                //SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
                //SalesInvoice saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(SelectedSaleOrderItemId);



            }
            if (saleinvoice.InvoiceType == "Inter" || saleinvoice.InvoiceType == "Final")
            {
                saleinvoice.isProjectBased = 1;
            }
            else
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
                TempData["success"] = "Saved successfully";
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

        public ActionResult Edit(int id ,string type)
        {
            try
            {
                if (id != 0)
                {
                    SalesInvoice saleinvoice = new SalesInvoice();
                    saleinvoice = new SalesInvoiceRepository().GetInvoiceHd(id, type);


                    saleinvoice.SaleInvoiceItems = new SalesInvoiceRepository().GetInvoiceItems(id);

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

                TempData["success"] = "Updated Successfully ";
                TempData["SalesInvoiceRefNo"] = model.SalesInvoiceRefNo;
                return RedirectToAction("PendingSalesInvoice", new { invType = model.InvoiceType });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }

        public ActionResult Print(int Id)
        {
            
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesInvoice.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

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
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("WorkDescription");
            ds.Tables["Items"].Columns.Add("WorkDescriptionRefNo");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("Unit");

            SalesInvoiceRepository repo = new SalesInvoiceRepository();
            var Head = repo.GetSalesInvoiceHdforPrint(Id,OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["SalesInvoiceRefNo"] = Head.SalesInvoiceRefNo;
            dr["SalesInvoiceDate"] = Head.SalesInvoiceDate.ToString("dd-MMM-yyyy");
            dr["VehicleOutPassNo"] = Head.VehicleOutPassNo;
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
            dr["Country"] = Head.Country;
            dr["Phone"] = Head.Phone;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Zip"] = Head.Zip;
      
            ds.Tables["Head"].Rows.Add(dr);

        
                SalesInvoiceItemRepository repo1 = new SalesInvoiceItemRepository();
                 var Items = repo1.GetSalesInvoiceItemforPrint(Id);
                foreach (var item in Items)
            {
                var InvItem = new SalesInvoiceItem { Quantity = item.Quantity, Rate = item.Rate, Amount = item.Amount, Unit = item.Unit, WorkDescription = item.WorkDescription, WorkDescriptionRefNo = item.WorkDescriptionRefNo };
                DataRow dri = ds.Tables["Items"].NewRow();

                dri["WorkDescription"] = InvItem.WorkDescription;
                dri["WorkDescriptionRefNo"] = InvItem.WorkDescriptionRefNo;
                dri["Quantity"] = InvItem.Quantity;
                dri["Rate"] = InvItem.Rate;
                dri["Amount"] = InvItem.Amount;
                dri["Unit"] = InvItem.Unit;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesInvoice.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SalesInvoice{0}.pdf", Id.ToString()));
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        public ActionResult Delete(int Id,string type)
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
              
    }
}