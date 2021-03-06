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
    public class MaterialPlanningController : BaseController
    {
        // GET: MaterialPlanning
        public ActionResult Index()
        {
            FillItemRM();
            //FillPartNo();
            InitDropdown();
            FillBatch();
            MaterialPlanning mp = new MaterialPlanning();
            mp.ItemId = 0;
            return View("Index", mp);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.PartNoList = new SelectList(List, "Id", "Name");
           
        }
        public void FillBatch()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 0, Name = "Non-Batch" });
            types.Add(new Dropdown { Id = 1, Name = "Batch" });
            ViewBag.BatchList = new SelectList(types, "Id", "Name");
        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemFGDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }
        public void FillItemRM()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemRMDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public void FillPartNo(int Id)
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PartNoDropdown(Id);
            ViewBag.PartNoList = new SelectList(result, "Id", "Name");
        }

        public ActionResult Planning(string partNo,int itmid = 0)
        {
            return PartialView("_Planning", new MaterialPlanningRepository().GetMaterialPlanning(itmid,partNo));
        }
        public ActionResult MaterialPlanningFG()
        {
            FillItem();
            FillBatch();
            return View();
        }
        public ActionResult PlanningFG(int itmid = 0)
        {
            return PartialView("_PlanningFG", new MaterialPlanningRepository().GetMaterialPlanningFG(itmid));
        }
        public ActionResult InTransitDetails(int id)
        {
            return PartialView("_InTransitDetails", new MaterialPlanningRepository().GetInTransitDetails(id));
        }
        public ActionResult SODetailsDetails(int id)
        {
            return PartialView("_SODetailsDetails", new MaterialPlanningRepository().GetSaleOrderDetails(id));
        }
        public ActionResult ReservedItemDetails(int id)
        {
            return PartialView("_ReservedItemDetails", new MaterialPlanningRepository().GetReservedItemDetails(id));
        }
        public ActionResult Item()
        {
            FillItemRM();
            return PartialView("_ItemDropDown");
        }
        public ActionResult Print(string Itmname = "", int Itmid = 0, string batchname = "", string batch = "all")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MaterialPlanning.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD

            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("Batch");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("WRQty");
            ds.Tables["Items"].Columns.Add("PndIssQty");
            ds.Tables["Items"].Columns.Add("MinStk");
            ds.Tables["Items"].Columns.Add("TotalReqqty");
            ds.Tables["Items"].Columns.Add("Currstk");
            ds.Tables["Items"].Columns.Add("IntransitQty");
            ds.Tables["Items"].Columns.Add("pndPRQty");
            ds.Tables["Items"].Columns.Add("Short");
            ds.Tables["Items"].Columns.Add("Uom");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["Item"] = Itmname;
            dr["Batch"] = batchname;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            MaterialPlanningRepository repo1 = new MaterialPlanningRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetMaterialPlanningDTPrint(Itmid, batch);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new MaterialPlanning
                {
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    WRQTY = item.WRQTY,
                    WRPndIssQty = item.WRPndIssQty,
                    MinLevel = item.MinLevel,
                    TotalQty = item.TotalQty,
                    CurrentStock = item.CurrentStock,
                    InTransitQty = item.InTransitQty,
                    PendingPRQty = item.PendingPRQty,
                    ShortorExcess = item.ShortorExcess,
                    UnitName = item.UnitName,

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Material"] = SupplyOrderRegItem.ItemName;
                dri["PartNo"] = SupplyOrderRegItem.PartNo;
                dri["WRQty"] = SupplyOrderRegItem.WRQTY;
                dri["PndIssQty"] = SupplyOrderRegItem.WRPndIssQty;
                dri["MinStk"] = SupplyOrderRegItem.MinLevel;
                dri["TotalReqqty"] = SupplyOrderRegItem.TotalQty;
                dri["Currstk"] = SupplyOrderRegItem.CurrentStock;
                dri["IntransitQty"] = SupplyOrderRegItem.InTransitQty;
                dri["pndPRQty"] = SupplyOrderRegItem.PendingPRQty;
                dri["Short"] = SupplyOrderRegItem.ShortorExcess;
                dri["Uom"] = SupplyOrderRegItem.UnitName;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "MaterialPlanning.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult FGPrint(string Itmname = "",int Itmid = 0)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MaterialPlanningFG.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD

            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("Batch");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("WRQty");
            ds.Tables["Items"].Columns.Add("PndIssQty");
            ds.Tables["Items"].Columns.Add("MinStk");
            ds.Tables["Items"].Columns.Add("TotalReqqty");
            ds.Tables["Items"].Columns.Add("Currstk");
            ds.Tables["Items"].Columns.Add("IntransitQty");
            ds.Tables["Items"].Columns.Add("pndlpoQty");
            ds.Tables["Items"].Columns.Add("Short");
            ds.Tables["Items"].Columns.Add("Uom");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["Item"] = Itmname;
           
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            MaterialPlanningRepository repo1 = new MaterialPlanningRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetMaterialPlanningFGPrint(Itmid);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new MaterialPlanning
                {
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    WRQTY = item.WRQTY,
                    WRPndIssQty = item.WRPndIssQty,
                    MinLevel = item.MinLevel,
                    TotalQty = item.TotalQty,
                    CurrentStock = item.CurrentStock,
                    InTransitQty = item.InTransitQty,
                    PendingPRQty = item.PendingPRQty,
                    ShortorExcess = item.ShortorExcess,
                    UnitName = item.UnitName,
                    
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Material"] = SupplyOrderRegItem.ItemName;
                dri["PartNo"] = SupplyOrderRegItem.PartNo;
                dri["WRQty"] = SupplyOrderRegItem.PENWRQTY;
                dri["PndIssQty"] = SupplyOrderRegItem.WRPndIssQty;
                dri["MinStk"] = SupplyOrderRegItem.MinLevel;
                dri["TotalReqqty"] = SupplyOrderRegItem.TotalQty;
                dri["Currstk"] = SupplyOrderRegItem.CurrentStock;
                dri["IntransitQty"] = SupplyOrderRegItem.InTransitQty;
                dri["pndlpoQty"] = SupplyOrderRegItem.PendingPRQty;
                dri["Short"] = SupplyOrderRegItem.ShortorExcess;
                dri["Uom"] = SupplyOrderRegItem.UnitName;
               
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "MaterialPlanningFG.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}