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
    public class PurchaseBillRegisterController : BaseController
    {
        // GET: PurchaseBillRegister
        public ActionResult Index()
        {
            FillItem();
            FillSupplier();
            ViewBag.startdate = FYStartdate;
           
            return View();
        }
        public ActionResult PurchaseBillRegister(DateTime? from, DateTime? to, int id = 0,int itmid = 0)
        {
            from = from ?? FYStartdate; ;
            to = to ?? DateTime.Today;
            return PartialView("_PurchaseBillRegister", new PurchaseBillRegisterRepository().GetPurchaseBillRegisterData(from,to,id,itmid,OrganizationId));
        }


        public ActionResult PurchaseBillDetailed()
        {
            FillItem();
            FillItemCategory();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PurchaseBillDetailedList(DateTime? from, DateTime? to, int id = 0, int itmid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PurchaseBillDetailedList", new PurchaseBillRegisterRepository().PurchaseBillDetailedData(from, to, id, itmid, OrganizationId));
        }

        public ActionResult PurchaseBillSummary()
        {
            FillItemCategory();
            FillSupplier();
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult PurchaseBillSummaryList(DateTime? from, DateTime? to, int id = 0, int supid = 0)
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_PurchaseBillSummaryList", new PurchaseBillRegisterRepository().PurchaseBillSummaryData(from, to, id, supid, OrganizationId));
        }



        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PBItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.GrnSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PBItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }
        public ActionResult Print(DateTime? from, DateTime? to, int itmid = 0, string itmName = "", int SupId = 0, string SupName = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PurchaseBillRegister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("SupplierHead");
            ds.Tables["Head"].Columns.Add("ItemHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("BillNo&Date");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Supplier");
            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("rate");
            ds.Tables["Items"].Columns.Add("TotalAmount");

            SupplyOrderRegisterRepository repo = new SupplyOrderRegisterRepository();
            var Head = repo.GetSupplyOrderRegisterHD(from, to, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["SupplierHead"] = SupName;
            dr["ItemHead"] = itmName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            PurchaseBillRegisterRepository repo1 = new PurchaseBillRegisterRepository();
            var Items = repo1.GetPurchaseBillRegisterDataDTPrint(from, to, SupId, itmid, OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new PurchaseBillRegister
                {
                    PurchaseBillNoDate = item.PurchaseBillNoDate,
                    PurchaseBillDate = item.PurchaseBillDate,
                    SupplierName = item.SupplierName,
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["BillNo&Date"] = SupplyOrderRegItem.PurchaseBillNoDate;
                dri["Date"] = SupplyOrderRegItem.PurchaseBillDate.ToString("dd-MMM-yyyy");
                dri["Supplier"] = SupplyOrderRegItem.SupplierName;
                dri["Material"] = SupplyOrderRegItem.ItemName;
                dri["Quantity"] = SupplyOrderRegItem.Quantity;
                dri["rate"] = SupplyOrderRegItem.Rate;
                dri["TotalAmount"] = SupplyOrderRegItem.Amount;
               
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PurchaseBillRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PurchaseBillRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Print1(DateTime? from, DateTime? to, int itmid = 0, string itmName = "", int SupId = 0, string SupName = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PurchaseBillSummary.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("SupplierHead");
            ds.Tables["Head"].Columns.Add("ItemHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            
            ds.Tables["Items"].Columns.Add("Datewise");
            ds.Tables["Items"].Columns.Add("Amount");
            SupplyOrderRegisterRepository repo = new SupplyOrderRegisterRepository();
            var Head = repo.GetSupplyOrderRegisterHD(from, to, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["SupplierHead"] = SupName;
            dr["ItemHead"] = itmName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            PurchaseBillRegisterRepository repo1 = new PurchaseBillRegisterRepository();
            var Items = repo1.PurchaseBillSummaryDataDTPrint(from, to, SupId, itmid, OrganizationId);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new PurchaseBillRegister
                {
                    PurchaseBillDate = item.PurchaseBillDate,
                    Amount = item.Amount,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Datewise"] = SupplyOrderRegItem.PurchaseBillDate;
                dri["Amount"] = SupplyOrderRegItem.Amount;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PurchaseBillSummary.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PurchaseBillSummary.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}