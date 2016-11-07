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
    public class ProductWiseSalesRegisterController : BaseController
    {
        // GET: ProductWiseSalesRegister
        public ActionResult Index()
        {
            FillCustomer();
        
            return View();
        }
     
        public ActionResult ProductWiseSalesRegister(int Id = 0)
        {

            return PartialView("_ProductWiseSalesRegister", new SalesRegisterRepository().GetProductWiseSalesRegister(OrganizationId, Id, FYStartdate, FYEnddate));
        }
        public void FillCustomer()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.SICustomerDropdown(OrganizationId);
            ViewBag.CustomerList = new SelectList(result, "Id", "Name");
        }
        public ActionResult Print(string name="",int id=0)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProductWiseSalesRegister.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("Customer");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("Workdes");
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
           
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["Customer"] = name;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetProductWiseSalesRegisterDTPrint(id, OrganizationId,FYStartdate,FYEnddate);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    WorkDescr = item.WorkDescr,
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
                    Dece = item.Dece
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Workdes"] = SupplyOrderRegItem.WorkDescr;
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

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProductWiseSalesRegister.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("ProductWiseSalesRegister.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}