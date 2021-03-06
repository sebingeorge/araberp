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
    public class OpeningStockReportController : BaseController
    {
        // GET: OpeningStockReport
        public ActionResult Index()
        {
            InitDropdown();
            FillWarehouse();
            FillItemCategory();
            FillGroup();
            FillSubGroup();
            OpeningStockReport os = new OpeningStockReport();
            os.itmCatId = 0;
            return View("Index", os);
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
        public void FillItemCategory()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCategoryDropdown();
            ViewBag.ItemCatList = new SelectList(result, "Id", "Name");
        }
        public void FillItem(int Id)
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemCatDropdown(Id);
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
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

        public ActionResult OpeningStockRegister(int stkid = 0, int itmcatid = 0, string itmid = "", int itmGroup = 0, int itmSubGroup = 0, string PartNo="")
        {

            return PartialView("_OpeningStockRegister", new OpeningStockRepository().GetClosingStockData(stkid, itmcatid, itmid, itmGroup, itmSubGroup, OrganizationId, PartNo));
        }
    
        public ActionResult Item(int Code)
        {
            FillItem(Code);
            return PartialView("_ItemDropDown");
        }
        public ActionResult Print(string Spname = "", int Spid = 0, int ItmCatid = 0, string ItmCatname = "", string Itmid = "", int ItmGroup = 0, string ItmGroupname = "", int ItmSubGroup = 0, string ItmSubGroupname = "", string PartNo="")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "OpeningStockReport.rpt"));

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
            ds.Tables["Head"].Columns.Add("Group");
            ds.Tables["Head"].Columns.Add("SubGroup");
            ds.Tables["Head"].Columns.Add("ItemCategory");

            //-------DT

            ds.Tables["Items"].Columns.Add("Item");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("OpeningStk");
            ds.Tables["Items"].Columns.Add("GroupG");
            ds.Tables["Items"].Columns.Add("SubGroupG");
            ds.Tables["Items"].Columns.Add("Unit");
      


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["Stkpoint"] = Spname;
            dr["ItemCat"] = ItmCatname;
            dr["Item"] = Itmid;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["PartNum"] = PartNo;
            dr["Group"] = ItmGroupname;
            dr["SubGroup"] = ItmSubGroupname;
            dr["ItemCategory"] = ItmCatname;
            ds.Tables["Head"].Rows.Add(dr);


            OpeningStockRepository repo1 = new OpeningStockRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetClosingStockDataDTPrint(stockPointId: Spid, itemCategoryId: ItmCatid, itemId: Itmid, itmGroup: ItmGroup, itmSubgroup: ItmSubGroup, OrganizationId: OrganizationId, partno:PartNo);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new OpeningStockReport
                {
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    Quantity = item.Quantity,
                    UnitName = item.UnitName,
                    ItemGroupName = item.ItemGroupName,
                    ItemSubGroupName = item.ItemSubGroupName,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Item"] = SupplyOrderRegItem.ItemName;
                dri["PartNo"] = SupplyOrderRegItem.PartNo;
                dri["OpeningStk"] = SupplyOrderRegItem.Quantity;
                dri["Unit"] = SupplyOrderRegItem.UnitName;
                dri["GroupG"] = SupplyOrderRegItem.ItemGroupName;
                dri["SubGroupG"] = SupplyOrderRegItem.ItemSubGroupName;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "OpeningStockReport.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("OpeningStockReport.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}