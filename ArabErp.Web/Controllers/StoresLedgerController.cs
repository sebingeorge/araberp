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
    public class StoresLedgerController : BaseController
    {
        // GET: StoresLedger
        public ActionResult Index()
        {

            InitDropdown();
            FillWarehouse();
            FillItemCategory();
            FillItemGroup();
            FillItemSubGroup();
            ClosingStock cs = new ClosingStock();
            cs.itmCatId = 0;
            ViewBag.startdate = FYStartdate;
            return View("Index", cs);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemList = new SelectList(List, "Id", "Name");

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
        //public void FillItemGroup(int Id)
        //{
        //    ItemRepository Repo = new ItemRepository();
        //    var List = Repo.FillItemGroup(Id);
        //    ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
        //}
        //public void FillItemSubGroup(int Id)
        //{
        //    ItemRepository Repo = new ItemRepository();
        //    var List = Repo.FillItemSubGroup(Id);
        //    ViewBag.ItemSubGroupList = new SelectList(List, "Id", "Name");
        //}
        public ActionResult StoresLedger(DateTime? from, DateTime? to, int stkid = 0, int itmcatid = 0,int itmGrpId = 0,int itmSubGrpId = 0,string itmid = "", string PartNo = "")
        {
            from = from ?? FYStartdate;
            to = to ?? DateTime.Today;
            return PartialView("_StoresLedger", new StoresLedgerRepository().GetStoresLedgerData(from, to, stkid, itmcatid,itmGrpId,itmSubGrpId, itmid, OrganizationId, PartNo));
        }
        public ActionResult Item(int Code)
        {
            FillItem(Code);
            return PartialView("_ItemDropDown");
        }

        //public ActionResult ItemGroup(int Code)
        //{
        //    FillItemGroup(Code);
        //    return PartialView("_ItemGroupDropdown");
        //}
        //public ActionResult ItemSubGroup(int Code)
        //{
        //    FillItemSubGroup(Code);
        //    return PartialView("_ItemSubGroupDropdown");
        //}
        //public ActionResult ItemCategory()
        //{
        //    FillItemCategory();
        //    return PartialView("_ItemCategoryDropdown");
        //}
        public ActionResult Print(DateTime? from,DateTime?to, string Spname = "", int Spid = 0, int ItmCatid = 0, string ItmCatname = "", string Itmname = "", string Itmid ="",string PartNo="")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "StoresLedger.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("From");
            ds.Tables["Head"].Columns.Add("To");
            ds.Tables["Head"].Columns.Add("Stkpoint");
            ds.Tables["Head"].Columns.Add("ItemCat");
            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("PartNo");

            //-------DT

            ds.Tables["Items"].Columns.Add("Entrydate");
            ds.Tables["Items"].Columns.Add("Transno");
            ds.Tables["Items"].Columns.Add("TransType");
            ds.Tables["Items"].Columns.Add("QtyIn");
            ds.Tables["Items"].Columns.Add("Qtyout");
            ds.Tables["Items"].Columns.Add("PartNum");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["From"] = from.Value.ToShortDateString();
            dr["To"] = to.Value.ToShortDateString();
            dr["Stkpoint"] = Spname;
            dr["ItemCat"] = ItmCatname;
            dr["Item"] = Itmid;
            dr["PartNo"] = PartNo;
            
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            StoresLedgerRepository repo1 = new StoresLedgerRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetStoresLedgerDataDTPrint(from,to, Spid,ItmCatid,Itmid, OrganizationId,PartNo);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new ClosingStock
                {
                    stocktrnDate = item.stocktrnDate,
                    StockUserId = item.StockUserId,
                    StockType = item.StockType,
                    INQTY = item.INQTY,
                    OUTQTY = item.OUTQTY,
                    PartNo = item.PartNo,
                

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Entrydate"] = SupplyOrderRegItem.stocktrnDate.ToString("dd-MMM-yyyy");
                dri["Transno"] = SupplyOrderRegItem.StockUserId;
                dri["TransType"] = SupplyOrderRegItem.StockType;
                dri["QtyIn"] = SupplyOrderRegItem.INQTY;
                dri["Qtyout"] = SupplyOrderRegItem.OUTQTY;
                dri["PartNum"] = SupplyOrderRegItem.PartNo;
                //dri["totalOutQty"] = SupplyOrderRegItem.UnitName;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "StoresLedger.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("StoresLedger.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}