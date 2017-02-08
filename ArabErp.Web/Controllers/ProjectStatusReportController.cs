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
    public class ProjectStatusReportController : BaseController
    {
        // GET: ProjectStatusReport
        public ActionResult Index()
        {
            FillJobCard();
            return View();
        }

        public void FillJobCard()
        {
            ViewBag.JCList = new SelectList(new DropdownRepository().JobCardDropdown(OrganizationId), "Id", "Name");
        }

        public ActionResult ProjectStatusReport(int id = 0)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProjectStatusReport.rpt"));

            DataSet ds = new DataSet();

            ds.Tables.Add("OrganizationDtls");
            ds.Tables.Add("ProjectDtls");
            ds.Tables.Add("RoomDtls");
            ds.Tables.Add("DailyActivityDtls");
            ds.Tables.Add("DelieveryStatus");
            //-------Organization Dtls
            ds.Tables["OrganizationDtls"].Columns.Add("OrganizationName");
            ds.Tables["OrganizationDtls"].Columns.Add("Image1");
            //-------Project Dtls
            ds.Tables["ProjectDtls"].Columns.Add("JobCardNo");
            ds.Tables["ProjectDtls"].Columns.Add("JobCardDate");
            ds.Tables["ProjectDtls"].Columns.Add("Customer");
            ds.Tables["ProjectDtls"].Columns.Add("ProjectName");
            ds.Tables["ProjectDtls"].Columns.Add("ContactPerson");
            ds.Tables["ProjectDtls"].Columns.Add("ContactNumber");
            ds.Tables["ProjectDtls"].Columns.Add("InCharge");
            ds.Tables["ProjectDtls"].Columns.Add("CostingAmount");
            ds.Tables["ProjectDtls"].Columns.Add("QuerySheetRefNo");
            //-------Room Dtls
            ds.Tables["RoomDtls"].Columns.Add("RoomDetails");
            ds.Tables["RoomDtls"].Columns.Add("RoomSize");
            ds.Tables["RoomDtls"].Columns.Add("TempRequired");
            ds.Tables["RoomDtls"].Columns.Add("Door");
            ds.Tables["RoomDtls"].Columns.Add("Floor");
            //-------Daily Activity Dtls
            ds.Tables["DailyActivityDtls"].Columns.Add("JobCardDailyActivityDate");
            ds.Tables["DailyActivityDtls"].Columns.Add("JobCardTaskName");
            ds.Tables["DailyActivityDtls"].Columns.Add("EmployeeName");
            ds.Tables["DailyActivityDtls"].Columns.Add("StartTime");
            ds.Tables["DailyActivityDtls"].Columns.Add("EndTime");
            ds.Tables["DailyActivityDtls"].Columns.Add("EstHours");
            ds.Tables["DailyActivityDtls"].Columns.Add("OverTime");
            ds.Tables["DailyActivityDtls"].Columns.Add("ActualHours");
            //-------Material Delievery Status
            ds.Tables["DelieveryStatus"].Columns.Add("ItemName");
            ds.Tables["DelieveryStatus"].Columns.Add("IssuedQuantity");

            //----------------DATA SET-------------------
            //--------------------------------Organization Dtls
            OrganizationRepository repo = new OrganizationRepository();
            var OrganizationDtls = repo.GetOrganization(OrganizationId);
            
            DataRow dr = ds.Tables["OrganizationDtls"].NewRow();
            dr["OrganizationName"] = OrganizationDtls.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + OrganizationDtls.Image1;
            ds.Tables["OrganizationDtls"].Rows.Add(dr);

            //--------------------------------Project Dtls
            JobCardRepository repo1 = new JobCardRepository();
            var ProjectDtls = repo1.GetProjectDtls(id);

            DataRow dr1 = ds.Tables["ProjectDtls"].NewRow();
            dr1["JobCardNo"] = ProjectDtls.JobCardNo;
            dr1["JobCardDate"] = ProjectDtls.JobCardDate.ToString("dd-MMM-yyyy");
            dr1["Customer"] = ProjectDtls.Customer;
            dr1["ProjectName"] = ProjectDtls.ProjectName;
            dr1["ContactPerson"] = ProjectDtls.ContactPerson;
            dr1["ContactNumber"] = ProjectDtls.ContactNumber;
            dr1["InCharge"] = ProjectDtls.InCharge;
            dr1["CostingAmount"] = ProjectDtls.CostingAmount;
            dr1["QuerySheetRefNo"] = ProjectDtls.QuerySheetRefNo;
            ds.Tables["ProjectDtls"].Rows.Add(dr1);

            //--------------------------------Room Dtls
            JobCardRepository repo2 = new JobCardRepository();
            var RoomDtls = repo2.GetRoomDtls(id);

            foreach (var item in RoomDtls)
            {
                var RDtls = new ProjectStatusReport
                {
                    RoomDetails = item.RoomDetails,
                    RoomSize = item.RoomSize,
                    TempRequired = item.TempRequired,
                    Door = item.Door,
                    Floor = item.Floor,
                };

                DataRow dr2 = ds.Tables["RoomDtls"].NewRow();
                dr2["RoomDetails"] = RDtls.RoomDetails;
                dr2["RoomSize"] = RDtls.RoomSize;
                dr2["TempRequired"] = RDtls.TempRequired;
                dr2["Door"] = RDtls.Door;
                dr2["Floor"] = RDtls.Floor;
                ds.Tables["RoomDtls"].Rows.Add(dr2);
            }

            //--------------------------------Daily Activity Dtls
            JobCardRepository repo3 = new JobCardRepository();
            var DailyActivityDtls = repo3.GetDailyActivityDtls(id);

            foreach (var item in DailyActivityDtls)
            {
                var DADtls = new ProjectStatusReport
                {
                    JobCardDailyActivityDate = item.JobCardDailyActivityDate,
                    JobCardTaskName = item.JobCardTaskName,
                    EmployeeName = item.EmployeeName,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    EstHours=item.EstHours,
                    OverTime = item.OverTime,
                    ActualHours = item.ActualHours,
                };

                DataRow dr3 = ds.Tables["DailyActivityDtls"].NewRow();
                dr3["JobCardDailyActivityDate"] = DADtls.JobCardDailyActivityDate.ToString("dd-MMM-yyyy");
                dr3["JobCardTaskName"] = DADtls.JobCardTaskName;
                dr3["EmployeeName"] = DADtls.EmployeeName;
                dr3["StartTime"] = DADtls.StartTime;
                dr3["EndTime"] = DADtls.EndTime;
                dr3["EstHours"] = DADtls.EstHours;
                dr3["OverTime"] = DADtls.OverTime;
                dr3["ActualHours"] = DADtls.ActualHours;
                ds.Tables["DailyActivityDtls"].Rows.Add(dr3);
            }

            //--------------------------------Material Delievery Status
            JobCardRepository repo4 = new JobCardRepository();
            var DelieveryStatus = repo4.GetDelieveryStatusDtls(id);

            foreach (var item in DelieveryStatus)
            {
                var DSDtls = new ProjectStatusReport
                {
                    ItemName = item.ItemName,
                    IssuedQuantity = item.IssuedQuantity,
                };

                DataRow dr4 = ds.Tables["DelieveryStatus"].NewRow();
                dr4["ItemName"] = DSDtls.ItemName;
                dr4["IssuedQuantity"] = DSDtls.IssuedQuantity;
                ds.Tables["DelieveryStatus"].Rows.Add(dr4);
            }


            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProjectStatusReport.xml"), XmlWriteMode.WriteSchema);

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