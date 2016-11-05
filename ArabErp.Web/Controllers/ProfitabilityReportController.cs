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
    public class ProfitabilityReportController : BaseController
    {
        // GET: ProfitabilityReport
        public ActionResult Index()
        {
            ProfitabilityReportRepository repo = new ProfitabilityReportRepository();
            return View(repo.GetProfitabilityReport());
        }


        public ActionResult Print()
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProfitabilityReport.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD


            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Items"].Columns.Add("SaleOrderDate");
            ds.Tables["Items"].Columns.Add("CustomerName");
            ds.Tables["Items"].Columns.Add("OrderAmount");
            ds.Tables["Items"].Columns.Add("Purchase");
            ds.Tables["Items"].Columns.Add("Expense");
            ds.Tables["Items"].Columns.Add("Labour");
            ds.Tables["Items"].Columns.Add("SalesInvoice");

            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            ProfitabilityReportRepository repo1 = new ProfitabilityReportRepository();
            var Items = repo1.GetProfitabilityReportDTPrint();

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new ProfitabilityReport
                {
                    SaleOrderRefNo = item.SaleOrderRefNo,
                    SaleOrderDate = item.SaleOrderDate,
                    CustomerName = item.CustomerName,
                    OrderAmount = item.OrderAmount,
                    Purchase = item.Purchase,
                    Expense = item.Expense,
                    Labour = item.Labour,
                    SalesInvoice = item.SalesInvoice,
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SaleOrderRefNo"] = SupplyOrderRegItem.SaleOrderRefNo;
                dri["SaleOrderDate"] = SupplyOrderRegItem.SaleOrderDate;
                dri["CustomerName"] = SupplyOrderRegItem.CustomerName;
                dri["OrderAmount"] = SupplyOrderRegItem.OrderAmount;
                dri["Purchase"] = SupplyOrderRegItem.Purchase;
                dri["Expense"] = SupplyOrderRegItem.Expense;
                dri["Labour"] = SupplyOrderRegItem.Labour;
                dri["SalesInvoice"] = SupplyOrderRegItem.SalesInvoice;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProfitabilityReport.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("ProfitabilityReport.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}