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
    public class PendingPARregisterController : BaseController
    {
        // GET: PendingPARregister
        public ActionResult Index()
        {
            return View(new PurchaseRequestRepository().GetPendingPARregisterData(OrganizationId));
            //return View();
        }

        public ActionResult Print()
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PendingPARregister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");
            //-------HEAD
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("Request No.");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("Remarks");
            ds.Tables["Items"].Columns.Add("Request Qty");
            ds.Tables["Items"].Columns.Add("GRN Qty");
            ds.Tables["Items"].Columns.Add("Settled Qty");
            ds.Tables["Items"].Columns.Add("Balance Qty");



            PurchaseRequestRepository repo = new PurchaseRequestRepository();
            var Head = repo.PurchaseRequestRegisterHD(OrganizationId);


            DataRow dr = ds.Tables["Head"].NewRow();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            PurchaseRequestRepository repo1 = new PurchaseRequestRepository();
            var Items = repo1.GetPendingPARregisterDT(OrganizationId);

            foreach (var item in Items)
            {
                var PurchaseRequestRegItem = new PurchaseRequestRegister
                {
                    PurchaseRequestNo = item.PurchaseRequestNo,
                    PurchaseRequestDate = item.PurchaseRequestDate,
                    ItemName = item.ItemName,
                    Remarks = item.Remarks,
                    ReqQty = item.ReqQty,
                    GRNQTY = item.GRNQTY,
                    SettledQty = item.SettledQty,
                    BALQTY = item.BALQTY

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Request No."] = PurchaseRequestRegItem.PurchaseRequestNo;
                dri["Date"] = PurchaseRequestRegItem.PurchaseRequestDate.ToString("dd-MMM-yyyy");
                dri["Material"] = PurchaseRequestRegItem.ItemName;
                dri["Remarks"] = PurchaseRequestRegItem.Remarks;
                dri["Request Qty"] = PurchaseRequestRegItem.ReqQty;
                dri["GRN Qty"] = PurchaseRequestRegItem.GRNQTY;
                dri["Settled Qty"] = PurchaseRequestRegItem.SettledQty;
                dri["Balance Qty"] = PurchaseRequestRegItem.BALQTY;
           

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PendingPARregister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PendingPARregister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}