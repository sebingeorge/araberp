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


namespace ArabErp.Web.Controllers
{
    public class PendingTasksForCompletionController : BaseController
    {
        // GET: PendingTasksForCompletion
        public ActionResult Index()
        {
            IEnumerable list = new PendingTasksForCompletionRepository().GetPendingTasks(OrganizationId: OrganizationId);
            return View(list);
        }

        public ActionResult PendingTasks(string saleorder = "", string jobcard = "", string jobcarddate = "",
                                         string engineer = "", string task = "", string technician = "")
        {
            var list = new PendingTasksForCompletionRepository().GetPendingTasks(
                OrganizationId: OrganizationId,
                saleorder: saleorder,
                jobcard: jobcard,
                jobcarddate: jobcarddate,
                technician: technician,
                task: task,
                engineer: engineer);
            return PartialView("_PendingTasksGrid", list);
        }

        //public ActionResult Print(string saleordername = "", string jobcardName = "", string jobcarddate = "", string engineername = "", string taskname = "", string technicianname = "")
        //{

        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PendingTasksForCompletion.rpt"));

        //    DataSet ds = new DataSet();
        //    ds.Tables.Add("Head");
        //    ds.Tables.Add("Items");

        //    //-------HEAD

        //    ds.Tables["Head"].Columns.Add("Saleorder");
        //    ds.Tables["Head"].Columns.Add("Jobcard");
        //    ds.Tables["Head"].Columns.Add("Jobcarddate");
        //    ds.Tables["Head"].Columns.Add("Engineer");
        //    ds.Tables["Head"].Columns.Add("Task");
        //    ds.Tables["Head"].Columns.Add("Technician");
        //    ds.Tables["Head"].Columns.Add("OrganizationName");
        //    ds.Tables["Head"].Columns.Add("Image1");

        //    //-------DT

        //    ds.Tables["Items"].Columns.Add("SaleOrderRefdate");
        //    ds.Tables["Items"].Columns.Add("JobCardRefNodate");
        //    ds.Tables["Items"].Columns.Add("Engineer");
        //    ds.Tables["Items"].Columns.Add("Task");
        //    ds.Tables["Items"].Columns.Add("EstHours");
        //    ds.Tables["Items"].Columns.Add("Technician");
        //    ds.Tables["Items"].Columns.Add("Requireddate");



        //    OrganizationRepository repo = new OrganizationRepository();
        //    var Head = repo.GetOrganization(OrganizationId);

        //    DataRow dr = ds.Tables["Head"].NewRow();

        //    dr["Saleorder"] = saleordername;
        //    dr["Jobcard"] = jobcardName;
        //    dr["Jobcarddate"] = jobcarddate;
        //    dr["Engineer"] = engineername;
        //    dr["Task"] = taskname;
        //    dr["Technician"] = technicianname;
        //    dr["OrganizationName"] = Head.OrganizationName;
        //    dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
        //    ds.Tables["Head"].Rows.Add(dr);


        //    PendingTasksForCompletionRepository repo1 = new PendingTasksForCompletionRepository();
        //    //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
        //    var Items = repo1.GetPendingTasksDTPrint(OrganizationId, saleordername, jobcardName, jobcarddate, engineername, taskname, technicianname);

        //    foreach (var item in Items)
        //    {
        //        var SupplyOrderRegItem = new PendingTasksForCompletion
        //        {
        //            JobCardDate = item.JobCardDate,
        //            SaleOrderDate = item.SaleOrderDate,
        //            Engineer = item.Engineer,
        //            JobCardTaskName = item.JobCardTaskName,
        //            EstimatedHours = item.EstimatedHours,
        //            EmployeeName = item.EmployeeName,
        //            RequiredDate = item.RequiredDate,
                  

        //        };

        //        DataRow dri = ds.Tables["Items"].NewRow();
        //        dri["SaleOrderRefdate"] = SupplyOrderRegItem.ItemName;
        //        dri["JobCardRefNodate"] = SupplyOrderRegItem.PartNo;
        //        dri["Engineer"] = SupplyOrderRegItem.WRQTY;
        //        dri["Task"] = SupplyOrderRegItem.WRPndIssQty;
        //        dri["Technician"] = SupplyOrderRegItem.MinLevel;
        //        dri["TotalReqqty"] = SupplyOrderRegItem.TotalQty;
        //        dri["Currstk"] = SupplyOrderRegItem.CurrentStock;
        //        dri["IntransitQty"] = SupplyOrderRegItem.InTransitQty;
        //        dri["pndPRQty"] = SupplyOrderRegItem.PendingPRQty;
        //        dri["Short"] = SupplyOrderRegItem.ShortorExcess;
        //        dri["Uom"] = SupplyOrderRegItem.UnitName;

        //        ds.Tables["Items"].Rows.Add(dri);
        //    }

        //    ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PendingTasksForCompletion.xml"), XmlWriteMode.WriteSchema);

        //    rd.SetDataSource(ds);

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();


        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf", String.Format("PendingTasksForCompletion.pdf"));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }
}