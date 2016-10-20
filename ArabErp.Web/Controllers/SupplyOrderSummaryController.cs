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
    public class SupplyOrderSummaryController : BaseController
    {
        // GET: SupplyOrderSummary
        public ActionResult Index()
        {
            FillSupplier();
            ViewBag.startdate = FYStartdate;
            return View();
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");

        }

        public ActionResult SupplyOrderSummary(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderSummary", new SupplyOrderRegisterRepository().GetSupplyOrderSummaryData(from, to, id, OrganizationId));
        }

        public ActionResult Print(DateTime? from, DateTime? to, string id, int SupId)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SupplyOrderSummary.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");
            //ds.Tables.Add("Supplier");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("SupplierHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("SO.No.");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Supplier");
            ds.Tables["Items"].Columns.Add("TotalAmount");

            //-------Supplier
            //ds.Tables["Supplier"].Columns.Add("Supplier");


            SupplyOrderRegisterRepository repo = new SupplyOrderRegisterRepository();
            var Head = repo.SupplyOrderSummaryHD(from, to, OrganizationId, id);
            //var Supplier = repo.Supplier(id);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["SupplierHead"] = id;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            //DataRow drs = ds.Tables["Supplier"].NewRow();
            ////drs["Supplier"] = Supplier.SupplierName;
            //ds.Tables["Supplier"].Rows.Add(drs);

            SupplyOrderRegisterRepository repo1 = new SupplyOrderRegisterRepository();
            var Items = repo1.SupplyOrderSummaryDT(from, to, id, OrganizationId, SupId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SupplyOrderRegister
                {
                    SupplyOrderNo = item.SupplyOrderNo,
                    SupplyOrderDate = item.SupplyOrderDate,
                    SupplierName = item.SupplierName,
                    TotalAmount = item.TotalAmount

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SO.No."] = SupplyOrderRegItem.SupplyOrderNo;
                dri["Date"] = SupplyOrderRegItem.SupplyOrderDate.ToString("dd-MMM-yyyy");
                dri["Supplier"] = SupplyOrderRegItem.SupplierName;
                dri["TotalAmount"] = SupplyOrderRegItem.TotalAmount;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SupplyOrderSummary.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SupplyOrderSummary.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}