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
    public class SalesAnalysisCustomerRegisterController : BaseController
    {
        // GET: SalesAnalysisCustomerRegister
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
        public ActionResult SalesAnalysisCustomer(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SalesAnalysisCustomer", new SalesRegisterRepository().GetSalesAnalysisCustomerWise(from, to, id, OrganizationId));
        }
        public ActionResult Print(DateTime? from, DateTime? to,int id, string name)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesAnalysisCustomerWise.rpt"));

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

            ds.Tables["Items"].Columns.Add("INV.NO");
            ds.Tables["Items"].Columns.Add("Customer");
            ds.Tables["Items"].Columns.Add("Amount"); 
            ds.Tables["Items"].Columns.Add("per");
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["CusHead"] =name;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetSalesAnalysisCustomerWiseDTPrint(from, to, id,OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    SaleOrderRefNo = item.SaleOrderRefNo,
                    WorkDescrShortName = item.WorkDescrShortName,
                    Quantity = item.Quantity,
                    Amount = item.Amount,
                    Perc = item.Perc,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["INV.NO"] = SupplyOrderRegItem.SalesInvoiceRefNo;
                dri["Customer"] = SupplyOrderRegItem.CustomerName;
                dri["Amount"] = SupplyOrderRegItem.Amount; 
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesAnalysisCustomerWise.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SalesAnalysisCustomerWise.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}