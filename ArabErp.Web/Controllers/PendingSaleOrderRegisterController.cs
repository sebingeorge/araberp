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
    public class PendingSaleOrderRegisterController : BaseController
    {
        // GET: PendingSaleOrderRegister
        //PendingSaleOrderRegister
        public ActionResult Index(string rptType)
        {
            ViewBag.type = rptType;
            ViewBag.startdate = FYStartdate;
            FillCustomer();
            return View();
        }
        //SaleOrderVarianceRegister
      
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SOCustomerDropDown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
        public ActionResult PendingSaleOrderRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSaleOrderRegister", new SalesRegisterRepository().GetPendingSO(from, to, id, OrganizationId));
        }
        public ActionResult SaleOrderVarianceRegister(DateTime? from, DateTime? to, int id = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSaleOrderRegister", new SalesRegisterRepository().GetPendingSO(from, to, id, OrganizationId));
        }
        public ActionResult Print(DateTime? from, DateTime? to, int id = 0, string name = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PendingSaleOrderRegister.rpt"));

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
            ds.Tables["Items"].Columns.Add("Customer");
            ds.Tables["Items"].Columns.Add("SO.NO");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("QtyAsPerSO");
            ds.Tables["Items"].Columns.Add("QtySold");
            ds.Tables["Items"].Columns.Add("Balance");
            ds.Tables["Items"].Columns.Add("Status");
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
            var Items = repo1.GetPendingSODTPrint(from, to, id, OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    CustomerName = item.CustomerName,
                    SaleOrderRefNo = item.SaleOrderRefNo,
                    SaleOrderDate = item.SaleOrderDate,
                    WorkDescr = item.WorkDescr,
                    Quantity = item.Quantity,
                    INVQTY = item.INVQTY,
                    BALQTY = item.BALQTY,
                    Status=item.Status,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Customer"] = SupplyOrderRegItem.CustomerName;
                dri["SO.NO"] = SupplyOrderRegItem.SaleOrderRefNo;
                dri["Date"] = SupplyOrderRegItem.SaleOrderDate.ToString("dd-MMM-yyyy");
                dri["WorkDescr"] = SupplyOrderRegItem.WorkDescr;
                dri["QtyAsPerSO"] = SupplyOrderRegItem.Quantity;
                dri["QtySold"] = SupplyOrderRegItem.INVQTY;
                dri["Balance"] = SupplyOrderRegItem.BALQTY;
                dri["Status"] = SupplyOrderRegItem.Status;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PendingSaleOrderRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PendingSaleOrderRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
      
    }
}