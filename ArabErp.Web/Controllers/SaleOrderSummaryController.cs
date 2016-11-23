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
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class SaleOrderSummaryController  : BaseController
    {
   
        // GET: SaleOrderSummary
        public ActionResult Index()
        {
            ViewBag.startdate = FYStartdate;
            FillCustomer();
            return View();
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
        public ActionResult SaleRegisterSummary(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SaleRegisterSummary", new SalesRegisterRepository().GetSalesRegisterSummary(from, to, id, OrganizationId));
        }

        public ActionResult Print(DateTime? from, DateTime? to, int id = 0, string name = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SaleOrderSummary.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("CusHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("SalesInvoiceRefNo");
            ds.Tables["Items"].Columns.Add("SalesInvoiceDate");
            ds.Tables["Items"].Columns.Add("CustomerName");
            ds.Tables["Items"].Columns.Add("TotalAmount");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["CusHead"] = name;
       
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetSalesRegisterSummaryDTPrint(from, to, id ,OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    SalesInvoiceRefNo = item.SalesInvoiceRefNo,
                    SalesInvoiceDate = item.SalesInvoiceDate,
                    CustomerName = item.CustomerName,
                    TotalAmount = item.TotalAmount,
        
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SalesInvoiceRefNo"] = SupplyOrderRegItem.SalesInvoiceRefNo;
                dri["SalesInvoiceDate"] = SupplyOrderRegItem.SalesInvoiceDate.ToString("dd-MMM-yyyy");
                dri["CustomerName"] = SupplyOrderRegItem.CustomerName;
                dri["TotalAmount"] = SupplyOrderRegItem.TotalAmount;
                
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SaleOrderSummary.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SaleOrderSummary.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}