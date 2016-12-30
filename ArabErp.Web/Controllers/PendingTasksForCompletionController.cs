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
            FillNeworService();
           // IEnumerable list = new PendingTasksForCompletionRepository().GetPendingTasks(OrganizationId: OrganizationId);
            return View();
        }

        public ActionResult PendingTasks(string saleorder = "", string jobcard = "", string jobcarddate = "",
                                         string engineer = "", string task = "", string technician = "", string InstallType = "all")
        {
            var list = new PendingTasksForCompletionRepository().GetPendingTasks(
                OrganizationId: OrganizationId,
                saleorder: saleorder,
                jobcard: jobcard,
                jobcarddate: jobcarddate,
                technician: technician,
                task: task,
                engineer: engineer, InstallType: InstallType);
             return PartialView("_PendingTasksGrid", list);
        }

        public ActionResult Print(string saleorder = "", string jobcard = "", string jobcarddate = "", string engineer = "", string task = "", string technician = "")
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PendingTasksForCompletion.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD

            ds.Tables["Head"].Columns.Add("Saleorder");
            ds.Tables["Head"].Columns.Add("Jobcard");
            ds.Tables["Head"].Columns.Add("Jobcarddate");
            ds.Tables["Head"].Columns.Add("Engineer");
            ds.Tables["Head"].Columns.Add("Task");
            ds.Tables["Head"].Columns.Add("Technician");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Items"].Columns.Add("JobCardRefNo");
            ds.Tables["Items"].Columns.Add("SaleOrderdate");
            ds.Tables["Items"].Columns.Add("JobCarddate");
            ds.Tables["Items"].Columns.Add("Engineer");
            ds.Tables["Items"].Columns.Add("Task");
            ds.Tables["Items"].Columns.Add("EstHours");
            ds.Tables["Items"].Columns.Add("Technician");
            ds.Tables["Items"].Columns.Add("Requireddate");



            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["Saleorder"] = saleorder;
            dr["Jobcard"] = jobcard;
            dr["Jobcarddate"] = jobcarddate;
            dr["Engineer"] = engineer;
            dr["Task"] = task;
            dr["Technician"] = technician;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            PendingTasksForCompletionRepository repo1 = new PendingTasksForCompletionRepository();
            //var Items = repo1.GetSOVarianceDataDTPrint(from, to, itmid, itmName, SupId, SupName);
            var Items = repo1.GetPendingTasksDTPrint(OrganizationId: OrganizationId,
                saleorder: saleorder,
                jobcard: jobcard,
                jobcarddate: jobcarddate,
                technician: technician,
                task: task,
                engineer: engineer);

            foreach (var item in Items)
            {
                var PendingItem = new PendingTasksForCompletion
                {
                    SaleOrderRefNo=item.SaleOrderRefNo,
                    SaleOrderDate = item.SaleOrderDate,
                    JobCardNo = item.JobCardNo,
                    JobCardDate = item.JobCardDate,
                    Engineer = item.Engineer,
                    TaskDate = item.TaskDate,
                    EstimatedHours = item.EstimatedHours,
                    EmployeeName=item.EmployeeName,
                    RequiredDate=item.RequiredDate                
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SaleOrderRefNo"] = PendingItem.SaleOrderRefNo;
                dri["SaleOrderdate"] = PendingItem.SaleOrderDate.ToString("dd/MMM/yyyy");
                dri["JobCardRefNo"] = PendingItem.JobCardNo;
                dri["JobCarddate"] = PendingItem.JobCardDate.ToString("dd/MMM/yyyy");
                dri["Engineer"] = PendingItem.Engineer;
                dri["Task"] = PendingItem.JobCardTaskName;
                dri["EstHours"] = PendingItem.EstimatedHours;
                dri["Technician"] = PendingItem.EmployeeName;
                dri["Requireddate"] = PendingItem.RequiredDate;


                //ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PendingTasksForCompletion.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PendingTasksForCompletion.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region Dropdowns
        public void FillNeworService()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 1, Name = "New Installation" });
            types.Add(new Dropdown { Id = 2, Name = "Service" });
            ViewBag.Type = new SelectList(types, "Id", "Name");
        }
        #endregion
    }
}