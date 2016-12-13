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
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class GINRegisterController : BaseController
    {
        // GET: GRNRegister
        public ActionResult Index()
        {
            FillItemsinWR();
            return View();
        }
        public void FillItemsinWR()
        {
            ViewBag.ItmList = new SelectList(new DropdownRepository().WRItemDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult GINRegisterReport( string itmid="",string PartNo="")
        {

            return PartialView("_GINRegister", new SalesRegisterRepository().GetGINRegisterData(itmid, OrganizationId, PartNo));
        }
        public ActionResult Print(string Itmid ="" ,string PartNo="")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "GINRegister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

             //-------HEAD
     
            ds.Tables["Head"].Columns.Add("Item");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("PartNo");

            //-------DT

            ds.Tables["Items"].Columns.Add("MaterialCode");
            ds.Tables["Items"].Columns.Add("Material");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("Stock");
            ds.Tables["Items"].Columns.Add("Unit");
            ds.Tables["Items"].Columns.Add("WR.No");
            ds.Tables["Items"].Columns.Add("RequiredQty");
            ds.Tables["Items"].Columns.Add("IssuedQty");
            ds.Tables["Items"].Columns.Add("BalanceQty");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
           
            dr["Item"] = Itmid;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["PartNo"] = PartNo;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetGINRegisterDataDetailsPrint(id: Itmid, OrganizationId: OrganizationId, partno: PartNo);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new GINRegister
                {
                    ItemRefNo = item.ItemRefNo,
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    STOCK = item.STOCK,
                    UnitName = item.UnitName,
                     WorkShopRequestRefNo = item.WorkShopRequestRefNo,
                    Quantity = item.Quantity,
                    ISSQTY = item.ISSQTY,
                    BALQTY = item.BALQTY,
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["MaterialCode"] = SupplyOrderRegItem.ItemRefNo;
                dri["Material"] = SupplyOrderRegItem.ItemName;
                dri["PartNo"] = SupplyOrderRegItem.PartNo;
                dri["Stock"] = SupplyOrderRegItem.STOCK;
                dri["Unit"] = SupplyOrderRegItem.UnitName;
                dri["WR.No"] = SupplyOrderRegItem.WorkShopRequestRefNo;
                dri["RequiredQty"] = SupplyOrderRegItem.Quantity;
                dri["IssuedQty"] = SupplyOrderRegItem.ISSQTY;
                dri["BalanceQty"] = SupplyOrderRegItem.BALQTY;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "GINRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("GINRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}