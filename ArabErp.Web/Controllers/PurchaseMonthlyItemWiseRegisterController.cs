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
    public class PurchaseMonthlyItemWiseRegisterController : BaseController
    {
        // GET: PurchaseMonthlyItemWiseRegister
        public ActionResult Index()
        {
            FillSupplier();
            return View();
        }

        public ActionResult PurchaseMonthlyItemWiseRegister(int Id = 0)
        {

            return PartialView("_PurchaseMonthlyItemWiseRegister", new PurchaseBillRegisterRepository().GetPurchaseMonthlyItemWiseData(OrganizationId, Id, FYStartdate, FYEnddate));
        }

        public ActionResult PurchaseMonthlySupplierWise()
        {
            FillItem();
            return View();
        }

        public ActionResult PurchaseMonthlySupplierWiseRegister(int Id = 0)
        {

            return PartialView("_PurchaseMonthlySupplierWiseRegister", new PurchaseBillRegisterRepository().GetPurchaseMonthlySupplieriseData(OrganizationId,Id,FYStartdate,FYEnddate));
        }

        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PurchaseBillSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PBItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public ActionResult Print(int SupId = 0, string SupName = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PurchaseMonthlyItemwise.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
           
            ds.Tables["Head"].Columns.Add("SupplierHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("ItemDes");
            ds.Tables["Items"].Columns.Add("Jan");
            ds.Tables["Items"].Columns.Add("Feb");
            ds.Tables["Items"].Columns.Add("Mar");
            ds.Tables["Items"].Columns.Add("Apr");
            ds.Tables["Items"].Columns.Add("May");
            ds.Tables["Items"].Columns.Add("Jun");
            ds.Tables["Items"].Columns.Add("Jul");
            ds.Tables["Items"].Columns.Add("Aug");
            ds.Tables["Items"].Columns.Add("Sep");
            ds.Tables["Items"].Columns.Add("Oct");
            ds.Tables["Items"].Columns.Add("Nov");
            ds.Tables["Items"].Columns.Add("Dec");
            ds.Tables["Items"].Columns.Add("Total");
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization( OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
          
            dr["SupplierHead"] = SupName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            PurchaseBillRegisterRepository repo1 = new PurchaseBillRegisterRepository();
            var Items = repo1.GetPurchaseMonthlyItemWiseDataDTPrint(SupId: SupId, OrganizationId: OrganizationId, FYStartdate: FYStartdate, FYEnddate: FYEnddate);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new PurchaseBillRegister
                {
                    ItemName = item.ItemName,
                    Jan=item.Jan,
                    Feb=item.Feb,
                    Mar=item.Mar,
                    Apr=item.Apr,
                    May = item.May,
                    Jun = item.Jun,
                    Jul = item.Jul,
                    Aug = item.Aug,
                    Sep = item.Sep,
                    Oct = item.Oct,
                    Nov = item.Nov,
                    Dece = item.Dece,
                    Total = item.Total

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["ItemDes"] = SupplyOrderRegItem.ItemName;
                dri["Jan"] = SupplyOrderRegItem.Jan;
                dri["Feb"] = SupplyOrderRegItem.Feb;
                dri["Mar"] = SupplyOrderRegItem.Mar;
                dri["Apr"] = SupplyOrderRegItem.Apr;
                dri["May"] = SupplyOrderRegItem.May;
                dri["Jun"] = SupplyOrderRegItem.Jun;
                dri["Jul"] = SupplyOrderRegItem.Jul;
                dri["Aug"] = SupplyOrderRegItem.Aug;
                dri["Sep"] = SupplyOrderRegItem.Sep;
                dri["Oct"] = SupplyOrderRegItem.Oct;
                dri["Aug"] = SupplyOrderRegItem.Aug;
                dri["Sep"] = SupplyOrderRegItem.Sep;
                dri["Oct"] = SupplyOrderRegItem.Oct;
                dri["Nov"] = SupplyOrderRegItem.Nov;
                dri["Dece"] = SupplyOrderRegItem.Dece;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PurchaseMonthlyItemwise.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PurchaseMonthlyItemwise.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public ActionResult Print1(int itmid = 0, string itmName = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PurchaseMonthlySupplierwise.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD

            ds.Tables["Head"].Columns.Add("ItemHead");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("SupplierDes");
            ds.Tables["Items"].Columns.Add("Jan");
            ds.Tables["Items"].Columns.Add("Feb");
            ds.Tables["Items"].Columns.Add("Mar");
            ds.Tables["Items"].Columns.Add("Apr");
            ds.Tables["Items"].Columns.Add("May");
            ds.Tables["Items"].Columns.Add("Jun");
            ds.Tables["Items"].Columns.Add("Jul");
            ds.Tables["Items"].Columns.Add("Aug");
            ds.Tables["Items"].Columns.Add("Sep");
            ds.Tables["Items"].Columns.Add("Oct");
            ds.Tables["Items"].Columns.Add("Nov");
            ds.Tables["Items"].Columns.Add("Dec");
            ds.Tables["Items"].Columns.Add("Total");
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["ItemHead"] = itmName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            PurchaseBillRegisterRepository repo1 = new PurchaseBillRegisterRepository();
            var Items = repo1.GetPurchaseMonthlySupplierwiseDataDTPrint(OrganizationId: OrganizationId, itmid: itmid, FYStartdate: FYStartdate, FYEnddate: FYEnddate);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new PurchaseBillRegister
                {
                    SupplierName = item.SupplierName,
                    Jan = item.Jan,
                    Feb = item.Feb,
                    Mar = item.Mar,
                    Apr = item.Apr,
                    May = item.May,
                    Jun = item.Jun,
                    Jul = item.Jul,
                    Aug = item.Aug,
                    Sep = item.Sep,
                    Oct = item.Oct,
                    Nov = item.Nov,
                    Dece = item.Dece,
                    Total = item.Total

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SupplierDes"] = SupplyOrderRegItem.SupplierName;
                dri["Jan"] = SupplyOrderRegItem.Jan;
                dri["Feb"] = SupplyOrderRegItem.Feb;
                dri["Mar"] = SupplyOrderRegItem.Mar;
                dri["Apr"] = SupplyOrderRegItem.Apr;
                dri["May"] = SupplyOrderRegItem.May;
                dri["Jun"] = SupplyOrderRegItem.Jun;
                dri["Jul"] = SupplyOrderRegItem.Jul;
                dri["Aug"] = SupplyOrderRegItem.Aug;
                dri["Sep"] = SupplyOrderRegItem.Sep;
                dri["Oct"] = SupplyOrderRegItem.Oct;
                dri["Aug"] = SupplyOrderRegItem.Aug;
                dri["Sep"] = SupplyOrderRegItem.Sep;
                dri["Oct"] = SupplyOrderRegItem.Oct;
                dri["Nov"] = SupplyOrderRegItem.Nov;
                dri["Dec"] = SupplyOrderRegItem.Dece;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PurchaseMonthlySupplierwise.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PurchaseMonthlySupplierwise.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }

}