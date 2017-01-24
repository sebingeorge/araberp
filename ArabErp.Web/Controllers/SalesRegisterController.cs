using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;

namespace ArabErp.Web.Controllers
{
    public class SalesRegisterController : BaseController
    {
        // GET: SalesRegister
        public ActionResult Index()
        {
            ViewBag.startdate = Convert.ToDateTime("01/" + DateTime.Today.Month + "/" + DateTime.Today.Year);
            FillCustomer();
            return View();
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public ActionResult SaleRegister(DateTime? from, DateTime? to, int? project, int id = 0)
        {
            from = from ?? Convert.ToDateTime("01/" + DateTime.Today.Month + "/" + DateTime.Today.Year);
            to = to ?? DateTime.Today;
            return PartialView("_SaleRegister", new SalesRegisterRepository().GetSalesRegister(from, to, id, OrganizationId, project));
        }

        public ActionResult Print(int? project,DateTime? from, DateTime? to, int id = 0, string name = "" )
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesRegister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("Customer");
            ds.Tables["Head"].Columns.Add("type");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("SalesInvoiceRefNo");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("CustomerName");
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("Discount");
            ds.Tables["Items"].Columns.Add("Unit");
            ds.Tables["Items"].Columns.Add("TotalAmount");


            SupplyOrderRegisterRepository repo = new SupplyOrderRegisterRepository();
            var Head = repo.GetSupplyOrderRegisterHD(from, to, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["Customer"] = name;
            dr["type"] = project == null ? "All" : project == 1 ? "Project" : "Transport";
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSalesRegisterDTPrint(from, to, id, OrganizationId);
            var Items = repo1.GetSalesRegister(from, to, id, OrganizationId, project);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    SalesInvoiceRefNo = item.SalesInvoiceRefNo,
                    SalesInvoiceDate = item.SalesInvoiceDate,
                    CustomerName = item.CustomerName,
                    WorkDescr = item.WorkDescr,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Discount = item.Discount,
                    TotalAmount = item.TotalAmount,
                    UnitName = item.UnitName
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SalesInvoiceRefNo"] = SupplyOrderRegItem.SalesInvoiceRefNo;
                dri["Date"] = SupplyOrderRegItem.SalesInvoiceDate.ToString("dd-MMM-yyyy");
                dri["CustomerName"] = SupplyOrderRegItem.CustomerName;
                dri["WorkDescr"] = SupplyOrderRegItem.WorkDescr;
                dri["Quantity"] = SupplyOrderRegItem.Quantity;
                dri["Discount"] = SupplyOrderRegItem.Discount;
                dri["Unit"] = SupplyOrderRegItem.UnitName;
                dri["rate"] = SupplyOrderRegItem.Rate;
                dri["Amount"] = SupplyOrderRegItem.Amount;
                dri["TotalAmount"] = SupplyOrderRegItem.TotalAmount;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");//, String.Format("SalesRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}