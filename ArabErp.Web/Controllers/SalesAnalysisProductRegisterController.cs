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
    public class SalesAnalysisProductRegisterController : BaseController
    {
        // GET: SalesAnalysisProductRegister
        public ActionResult Index()
        {
            ViewBag.startdate = FYStartdate;
            FillWorkDesc();
            return View();
        }
        public void FillWorkDesc()
        {
            ViewBag.WrkList = new SelectList(new DropdownRepository().WorkDescDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult SalesAnalysisProduct(DateTime? from, DateTime? to)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SalesAnalysisProduct", new SalesRegisterRepository().GetSalesAnalysisProductWise(from, to, OrganizationId));
        }
        public ActionResult Print(DateTime? from, DateTime? to)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesAnalysisProductWise.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
     
            ds.Tables["Items"].Columns.Add("SO.NO");
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("Qty");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("per");
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetSalesAnalysisProductWiseDTPrint(from, to, OrganizationId);

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
                dri["SO.NO"] = SupplyOrderRegItem.SaleOrderRefNo;
                dri["WorkDescr"] = SupplyOrderRegItem.WorkDescrShortName;
                dri["Qty"] = SupplyOrderRegItem.Quantity;
                dri["Amount"] = SupplyOrderRegItem.Amount;
                dri["per"] = SupplyOrderRegItem.Perc;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesAnalysisProductWise.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SalesAnalysisProductWise.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}