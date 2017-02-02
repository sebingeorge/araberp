using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Data;

namespace ArabErp.Web.Controllers
{
    public class ProformaInvoiceController : BaseController
    {
        // GET: ProformaInvoice
        public ActionResult Index(int type)
        {
            FillProformaInvoice(type);
            FillCustomer(type);
            ViewBag.type = type;
            return View();
        }

        public ActionResult PreviousList(int type, DateTime? from, DateTime? to, int customer = 0, int id = 0)
        {
            return PartialView("_PreviousListGrid", new ProformaInvoiceRepository().PreviousList(OrganizationId: OrganizationId, type: type, from: from, to: to, customer: customer, id: id));
        }
        public ActionResult PendingProforma(int? ProjectBased)
        {
            var repo = new ProformaInvoiceRepository();
            ViewBag.ProjectBased = ProjectBased;
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForPerforma(ProjectBased ?? 0);
            return View(pendingSO);
        }
        public ActionResult Create(int? SaleOrderId)
        {
            ProformaInvoiceRepository repo = new ProformaInvoiceRepository();
            CurrencyRepository repo1 = new CurrencyRepository();
            ProformaInvoice model = repo.GetSaleOrderForPorforma(SaleOrderId ?? 0);
            //model.SymbolName = repo1.GetCurrencyFrmOrganization(OrganizationId).SymbolName;
            var PIList = repo.GetPorformaInvoiceData(model.SaleOrderId);
            model.Items = new List<ProformaInvoiceItem>();
            foreach (var item in PIList)
            {
                model.Items.Add(new ProformaInvoiceItem { WorkDescription = item.WorkDescription, VehicleModelName = item.VehicleModelName, Quantity = item.Quantity, UnitName = item.UnitName, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount, SaleOrderItemId = item.SaleOrderItemId });

            }

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(6, OrganizationId);

            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occurred. Please try again.|" + ex.Message;
            }

            model.ProformaInvoiceRefNo = internalId;
            model.ProformaInvoiceDate = DateTime.Now;

            model.PrintDescriptions = new ProformaInvoiceRepository().GetPrintDescriptions(SaleOrderId);
            if (model.PrintDescriptions == null || model.PrintDescriptions.Count == 0)
                model.PrintDescriptions.Add(new PrintDescription());
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(ProformaInvoice model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string id = new ProformaInvoiceRepository().InsertProformaInvoice(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Proforma Invoice No. is " + id.Split('|')[1];
                    //TempData["error"] = "";
                    return RedirectToAction("PendingProforma", new { ProjectBased = model.isProjectBased });
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
            TempData["success"] = "";


            return View(model);
        }

        public void FillProformaInvoice(int type)
        {
            ViewBag.proformaInvoiceList = new SelectList(new DropdownRepository().ProformaInvoiceDropdown(OrganizationId: OrganizationId, type: type), "Id", "Name");
        }
        private void FillCustomer(int type)
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerForProformaInvoice(OrganizationId: OrganizationId, type: type), "Id", "Name");
        }
        public ActionResult Edit( int type,int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home", new {type = type});
            ProformaInvoice model = new ProformaInvoiceRepository().GetProformaInvoiceHdDetails(id);
            //model.SymbolName = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).SymbolName;
            model.Items = new ProformaInvoiceRepository().GetProformaInvoiceItemDetails(id);
            model.PrintDescriptions = new ProformaInvoiceRepository().GetPrintDescription(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(ProformaInvoice model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
          
              {
                try
                {
                    var result2 = new ProformaInvoiceRepository().DeleteProformaInvoiceDT(model.ProformaInvoiceId);
                    string id =   new ProformaInvoiceRepository().UpdateProformaInvoiceHD(model);
                    string id1 =  new ProformaInvoiceRepository().InsertProformaInvoiceDT(model);

                    TempData["success"] = "Updated successfully. Proforma Invoice Reference No. is " + id;
                    TempData["error"] = "";
                    return RedirectToAction("Index", new { type =model.isProjectBased});
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

        public ActionResult ProformaInvoice(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProformaInvoiceee.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("ProformaInvoiceRefNo");
            ds.Tables["Head"].Columns.Add("ProformaInvoiceDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("Address");
            ds.Tables["Head"].Columns.Add("CustomerAddress");
            ds.Tables["Head"].Columns.Add("CustomerOrderRef");
            ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
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
          
            //-------DT
            ds.Tables["Items"].Columns.Add("PrintDescription");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("UoM");
            ds.Tables["Items"].Columns.Add("PriceEach");
            ds.Tables["Items"].Columns.Add("Amount");

            ProformaInvoiceRepository repo = new ProformaInvoiceRepository();
            var Head = repo.GetProformaInvoiceHD(Id);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["ProformaInvoiceRefNo"] = Head.ProformaInvoiceRefNo;
            dr["ProformaInvoiceDate"] = Head.ProformaInvoiceDate.ToString("dd-MMM-yyyy");
            dr["CustomerName"] = Head.CustomerName;
            dr["Address"] = Head.CustomerAddress;
            dr["CustomerAddress"] = Head.CustomerAddress;
            dr["CustomerOrderRef"] = Head.CustomerOrderRef;
            dr["SaleOrderRefNo"] = Head.SaleOrderRefNo;
            dr["PaymentTerms"] = Head.PaymentTerms;
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
            ds.Tables["Head"].Rows.Add(dr);


            ProformaInvoiceItemRepository repo1 = new ProformaInvoiceItemRepository();
            var Items = repo1.GetProformaInvoiceItemDT(Id);
            foreach (var item in Items)
            {
               
                DataRow dri = ds.Tables["Items"].NewRow();
                dri["PrintDescription"] = item.Description;
                dri["Quantity"] = item.Quantity;
                dri["PriceEach"] = item.PriceEach;
                dri["Amount"] = item.Amount;
                dri["UoM"] = item.UoM;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProformaInvoiceee.xml"), XmlWriteMode.WriteSchema);

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
    }
}