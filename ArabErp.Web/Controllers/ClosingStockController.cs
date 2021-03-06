﻿using ArabErp.DAL;
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
    public class ClosingStockController : BaseController
    {
        // GET: ClosingStock
        public ActionResult Index()
        {
            InitDropdown();
            FillWarehouse();
            FillItemCategory();
            ClosingStock cs = new ClosingStock();
            cs.itmCatId = 0;
            FillGroup();
            FillSubGroup();
            return View("Index", cs);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemList = new SelectList(List, "Id", "Name");

        }
        public ActionResult ClosingStockRegister(DateTime? Ason, int stkid = 0, int itmcatid = 0, string itmid = "", string PartNo = "", int itmGroup = 0, int itmSubGroup = 0)
        {
            Ason = Ason ?? DateTime.Today;
            return PartialView("_ClosingStockRegister", new ClosingStockRepository().GetClosingStockData1(Ason, stkid, itmcatid, itmid, OrganizationId,PartNo,itmGroup, itmSubGroup));
        }

        public ActionResult DrillDownIndex(int itemId)
        {
            ClosingStockRepository repo = new ClosingStockRepository();
            ViewBag.itemId = itemId;
            //ViewBag.ItemName = ItemName;
            ViewBag.startdate = FYStartdate;
            return View();
        }

        public ActionResult DrillDown(DateTime? from, DateTime? to, int itemId)
        {
            ClosingStockRepository repo = new ClosingStockRepository();
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return View(repo.GetItemWiseDetails(from, to, itemId));
        }
        public ActionResult ClosingStockWithAvgRate()
        {
            InitDropdown();
            FillWarehouse();
            return View();
        }
        public ActionResult ClosingStockWithAvgRateRegister(DateTime? Ason, int stkid = 0, string itmid = "", string PartNo = "")
        {
            Ason = Ason ?? DateTime.Today;
            return PartialView("_ClosingStockWithAvgRateRegister", new ClosingStockRepository().GetClosingStockWithAvgRate(Ason, stkid, itmid, OrganizationId, PartNo));
        }
        public void FillWarehouse()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.StockpointDropdown();
            ViewBag.WarehouseList = new SelectList(result, "Id", "Name");
        }
        public void FillItem(int Id)
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCatDropdown(Id);
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }
        public void FillItemCategory()
                {
                    DropdownRepository repo = new DropdownRepository();
                    var result = repo.ItemCategoryDropdown();
                    ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
                }
        public ActionResult Item(int Code)
         {
             FillItem(Code);
             return PartialView("_ItemDropDown");
         }

        public ActionResult Print(DateTime? date, string Spname = "", int Spid = 0, int ItmCatid = 0, string ItmCatname = "", string Itmid = "", string PartNo="")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ClosingStockReport.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("Stkpoint");
            ds.Tables["Head"].Columns.Add("ItemCat");
            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("PartNum");

            //-------DT

            ds.Tables["Items"].Columns.Add("Item");
            ds.Tables["Items"].Columns.Add("RefNo");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("ClosingStk");
            ds.Tables["Items"].Columns.Add("Unit");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["Stkpoint"] = Spname;
            dr["ItemCat"] = ItmCatname;
            dr["Item"] = Itmid;
            dr["PartNum"] = PartNo;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            ClosingStockRepository repo1 = new ClosingStockRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetClosingStockDataDTPrint(stockPointId: Spid, itemCategoryId: ItmCatid, itemId: Itmid, OrganizationId: OrganizationId,partno:PartNo);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new OpeningStockReport
                {
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    Quantity = item.Quantity,
                    UnitName = item.UnitName,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Item"] = SupplyOrderRegItem.ItemName;
                dri["RefNo"] = SupplyOrderRegItem.ItemName;
                dri["PartNo"] = SupplyOrderRegItem.PartNo;
                dri["ClosingStk"] = SupplyOrderRegItem.Quantity;
                dri["Unit"] = SupplyOrderRegItem.UnitName;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ClosingStockReport.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("ClosingStockReport.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void FillGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemGroupDropdown();
            ViewBag.ItemGroup = new SelectList(result, "Id", "Name");
        }
        public void FillSubGroup()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemSubgroupDropdown();
            ViewBag.ItemSubgroup = new SelectList(result, "Id", "Name");
        }
    }
}