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
    public class SupplyOrderRegisterController : BaseController
    {
        // GET: SupplyOrderRegister
        public ActionResult Index()
        {
            FillSupplier();
            FillItem();
            ViewBag.startdate = FYStartdate;
            return View();
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SupplyOrderSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");

        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SOItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public ActionResult SupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderRegister", new SupplyOrderRegisterRepository().GetSupplyOrderRegisterData(from, to, id, itmid, OrganizationId));
        }

        public ActionResult PengingSO()
        {

            FillSupplier();
            FillItem();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PendingSupplyOrderRegister(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PendingSupplyOrderRegister", new SupplyOrderRegisterRepository().GetPendingSupplyOrderRegister(from, to, id, itmid, OrganizationId));
        }


        public ActionResult SOVariance()
        {

            FillSupplier();
            FillItem();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult SupplyOrderVarianceReport(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_SupplyOrderVarianceReport", new SupplyOrderRegisterRepository().GetSOVarianceData(from, to, id, itmid, OrganizationId));
        }



        public ActionResult Print(DateTime? from, DateTime? to,int id = 0, int itmid = 0)
        {
        
           ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SupplyOrderRegister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("SO.No.");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Supplier");
            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("Unit");
            ds.Tables["Items"].Columns.Add("rate");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("TotalAmount");

            //ds.Tables["Head"].Columns.Add("OrganizationName");
            //ds.Tables["Head"].Columns.Add("Image1");

            SupplyOrderRegisterRepository repo = new SupplyOrderRegisterRepository();
            var Head = repo.GetSupplyOrderRegisterHD(from,to,OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SupplyOrderRegisterRepository repo1 = new SupplyOrderRegisterRepository();
             var Items = repo1.GetSupplyOrderRegisterDT(from, to,id, itmid,  OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SupplyOrderRegister
                {
                    SupplyOrderNo = item.SupplyOrderNo,
                    SupplyOrderDate = item.SupplyOrderDate,
                    SupplierName = item.SupplierName,
                    ItemName = item.ItemName,
                    OrderedQty = item.OrderedQty,
                    UnitName = item.UnitName,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    TotalAmount = item.TotalAmount
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SO.No."] = SupplyOrderRegItem.SupplyOrderNo;
                dri["Date"] = SupplyOrderRegItem.SupplyOrderDate.ToString("dd-MMM-yyyy");
                dri["Supplier"] = SupplyOrderRegItem.SupplierName;
                dri["Material"] = SupplyOrderRegItem.ItemName;
                dri["Quantity"] = SupplyOrderRegItem.OrderedQty;
                dri["Unit"] = SupplyOrderRegItem.UnitName;
                dri["rate"] = SupplyOrderRegItem.Rate;
                dri["Amount"] = SupplyOrderRegItem.Amount;
                dri["TotalAmount"] = SupplyOrderRegItem.TotalAmount;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SupplyOrderRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SupplyOrderRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}