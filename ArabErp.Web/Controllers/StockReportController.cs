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
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class StockReportController : BaseController
    {
        // GET: StockReport
        public ActionResult Index()
        {
            //StockReportRepository repo = new StockReportRepository();
            //return View(repo.GetStockReport());
            FillItemCategory();
            FillItemGroup();
            FillItemSubGroup();
            return View();
        }

        public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }

        public void FillItemGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemGroupDropdown();
            ViewBag.ItemGrpList = new SelectList(result, "Id", "Name");
        }

        public void FillItemSubGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemSubgroupDropdown();
            ViewBag.ItemSubGrpList = new SelectList(result, "Id", "Name");
        }

        public ActionResult Stockreport(string itmid = "", string PartNo = "", int itmcatid = 0, int itmGrpId = 0, int itmSubGrpId = 0)
        {

            return PartialView("_StockReportRegister", new StockReportRepository().GetStockData(itmid, PartNo,itmcatid,itmGrpId,itmSubGrpId));
        }
        public ActionResult DrillDown(int itemId)
        {
            StockReportRepository repo = new StockReportRepository();
            return View(repo.GetStockReportItemWise(itemId));
        }

        public ActionResult Print(string itmid="",string PartNo="")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "StockSummary.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
         
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("PartNo");

            //-------DT

            ds.Tables["Items"].Columns.Add("Item");
            ds.Tables["Items"].Columns.Add("IN");
            ds.Tables["Items"].Columns.Add("OUT");
            ds.Tables["Items"].Columns.Add("Balance");
            ds.Tables["Items"].Columns.Add("PartNum");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
          
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["Item"] = itmid;
            dr["PartNo"] = PartNo;
            ds.Tables["Head"].Rows.Add(dr);


            StockReportRepository repo1 = new StockReportRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetStockReportDTPrint(itmid,PartNo);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new StockReportSummary
                {
                    ItemName = item.ItemName,
                    InQuantity = item.InQuantity,
                    OutQuantity = item.OutQuantity,
                    Balance = item.Balance,
                    PartNo = item.PartNo,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Item"] = SupplyOrderRegItem.ItemName;
                dri["IN"] = SupplyOrderRegItem.InQuantity;
                dri["OUT"] = SupplyOrderRegItem.OutQuantity;
                dri["Balance"] = SupplyOrderRegItem.Balance;
                dri["PartNum"] = SupplyOrderRegItem.PartNo;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "StockSummary.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("StockSummary.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}