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
    public class TargetVsAchievedController : BaseController
    {
        // GET: TargetVsAchieved
        public ActionResult Index()
        {
            FillMonth();
            return View();
        }

        public void FillMonth()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.MonthDropdown(OrganizationId);
            ViewBag.MonthList = new SelectList(result, "Id", "Name");
        }
        public ActionResult TargetVsAchievedRegister(int Id=0)
        {

            return PartialView("_TargetVsAchievedRegister", new SalesRegisterRepository().GetTargetVsAchieved(OrganizationId, Id,FYStartdate,FYEnddate));
        }

        public ActionResult Print( string name,int id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "TargetVsAchieved.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD
            ds.Tables["Head"].Columns.Add("Month");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("MonthName");
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("Target");
            ds.Tables["Items"].Columns.Add("Achieved");
            ds.Tables["Items"].Columns.Add("Varience");
            ds.Tables["Items"].Columns.Add("Varperc");
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["Month"] = name;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SalesRegisterRepository repo1 = new SalesRegisterRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetTargetVsAchievedDTPrint(id, OrganizationId, FYStartdate, FYEnddate);

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SalesRegister
                {
                    MonthName = item.MonthName,
                    WorkDescr = item.WorkDescr,
                    Target = item.Target,
                    Achieved = item.Achieved,
                    Varience = item.Varience,
                    Varperc=item.Varperc

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["MonthName"] = SupplyOrderRegItem.MonthName;
                dri["Target"] = SupplyOrderRegItem.Target;
                dri["Achieved"] = SupplyOrderRegItem.Achieved;
                dri["Varience"] = SupplyOrderRegItem.Varience;
                dri["Varperc"] = SupplyOrderRegItem.Varperc;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "TargetVsAchieved.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("TargetVsAchieved.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}