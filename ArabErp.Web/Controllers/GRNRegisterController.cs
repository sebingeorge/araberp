using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Data;
using System.Data.SqlClient;


namespace ArabErp.Web.Controllers
{
    public class GRNRegisterController : BaseController
    {
        // GET: GRNRegister
        public ActionResult Index()
        {
            FillItemGroup();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public void FillItemGroup()
        {
            ViewBag.ItmGrpList = new SelectList(new DropdownRepository().ItemGroupDropdown(), "Id", "Name");
        }

        public ActionResult GRNRegister(DateTime? from, DateTime? to, int id = 0, string material = "",string partno="" ,string supplier = "")
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_GRNRegister", new GRNRegisterRepository().GetGRNRegister(from, to, id, material,partno,supplier));
        }

        public ActionResult Summary(DateTime fromDate, DateTime ToDate, string Supplier = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "GRNSummary.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("Supplier");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            //-------DT

            ds.Tables["Items"].Columns.Add("GRNNo");
            ds.Tables["Items"].Columns.Add("GRNDate");
            ds.Tables["Items"].Columns.Add("Supplier");
            ds.Tables["Items"].Columns.Add("LPONo");
            ds.Tables["Items"].Columns.Add("DCNo");
            ds.Tables["Items"].Columns.Add("Amount");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = fromDate.ToString("dd-MMM-yyyy");
            dr["To"] = ToDate.ToString("dd-MMM-yyyy");
            dr["Supplier"] = Supplier;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            GRNRegisterRepository repo1 = new GRNRegisterRepository();

            var Items = repo1.GRNSummary(from: fromDate, to: ToDate, supplier: Supplier);

            foreach (var item in Items)
            {
                var GRNSummaryItem = new GRNRegister
                {
                    GRNNo = item.GRNNo,
                    GRNDate = item.GRNDate,
                    SupplierName = item.SupplierName,
                    SupplyOrderNo = item.SupplyOrderNo,
                    DCNo = item.DCNo,
                    Amount = item.Amount,
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["GRNNo"] = GRNSummaryItem.GRNNo;
                dri["GRNDate"] = GRNSummaryItem.GRNDate.ToString("dd-MMM-yyyy");
                dri["Supplier"] = GRNSummaryItem.SupplierName;
                dri["LPONo"] = GRNSummaryItem.SupplyOrderNo;
                dri["DCNo"] = GRNSummaryItem.DCNo;
                dri["Amount"] = GRNSummaryItem.Amount;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "GRNSummary.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("GRNSummary.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Detailed(DateTime fromDate, DateTime ToDate, string matGrpName = "",int matGrpid=0,
            string matName = "",string PartNo = "", string supplier = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "GRNDetailed.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("MaterialGroup");
            ds.Tables["Head"].Columns.Add("MaterialName");
            ds.Tables["Head"].Columns.Add("PartNo");
            ds.Tables["Head"].Columns.Add("Supplier");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            //-------DT

            ds.Tables["Items"].Columns.Add("GRNNo");
            ds.Tables["Items"].Columns.Add("GRNDate");
            ds.Tables["Items"].Columns.Add("Supplier");
            ds.Tables["Items"].Columns.Add("LPONo");
            ds.Tables["Items"].Columns.Add("DCNo");
            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("Qty");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("Unit");
   
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = fromDate.ToString("dd-MMM-yyyy");
            dr["To"] = ToDate.ToString("dd-MMM-yyyy");
            dr["MaterialGroup"] = matGrpName;
            dr["MaterialName"] = matName;
            dr["PartNo"] = PartNo;
            dr["Supplier"] = supplier;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            GRNRegisterRepository repo1 = new GRNRegisterRepository();


            var Items = repo1.GRNDetailed(from: fromDate, to: ToDate, id: matGrpid, material: matName, partno: PartNo, supplier: supplier);

            foreach (var item in Items)
            {
                var GRNDetailedItem = new GRNRegister
                {
                    GRNNo = item.GRNNo,
                    GRNDate = item.GRNDate,
                    SupplierName = item.SupplierName,
                    SupplyOrderNo = item.SupplyOrderNo,
                    DCNo = item.DCNo,
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    UnitName = item.UnitName,
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["GRNNo"] = GRNDetailedItem.GRNNo;
                dri["GRNDate"] = GRNDetailedItem.GRNDate.ToString("dd-MMM-yyyy");
                dri["Supplier"] = GRNDetailedItem.SupplierName;
                dri["LPONo"] = GRNDetailedItem.SupplyOrderNo;
                dri["DCNo"] = GRNDetailedItem.DCNo;
                dri["Material"] = GRNDetailedItem.ItemName;
                dri["PartNo"] = GRNDetailedItem.PartNo;
                dri["Qty"] = GRNDetailedItem.Quantity;
                dri["Rate"] = GRNDetailedItem.Rate;
                dri["Amount"] = GRNDetailedItem.Amount;
                dri["Unit"] = GRNDetailedItem.UnitName;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "GRNDetailed.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("GRNDetailed.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}