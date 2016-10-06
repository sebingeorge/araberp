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
    public class JobCardController : BaseController
    {
        JobCardRepository repo;
        public JobCardController()
        {
            repo = new JobCardRepository();
        }
        // GET: JobCard
        public ActionResult Index(int isProjectBased = 0)
        {
            try
            {
                FillJCNo(isProjectBased);
                FillCustomerinJC(isProjectBased);
                ViewBag.ProjectBased = isProjectBased;
                return View();
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        public ActionResult Create(int? Id, int? isProjectBased)
        {
            try
            {
                FillBay();
                FillEmployee();

                JobCardRepository repo = new JobCardRepository();
                SaleOrderRepository soRepo = new SaleOrderRepository();
                isProjectBased = soRepo.IsProjectOrVehicle(Id ?? 0);
                JobCard model = repo.GetJobCardDetails(Id ?? 0, isProjectBased ?? 0);
                model.JobCardNo = DatabaseCommonRepository.GetNextDocNo(16, OrganizationId);
                model.isProjectBased = isProjectBased ?? 0;
                model.JobCardTasks = new List<JobCardTask>();
                model.JobCardTasks.Add(new JobCardTask() { TaskDate = DateTime.Now });
                model.JobCardDate = DateTime.Now;
                model.RequiredDate = DateTime.Now;
               
                FillTaks(model.WorkDescriptionId);
                //FillFreezerUnit();
                //FillBox();
                //FillVehicleRegNo();
             
                return View(model);
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        public ActionResult PendingJobCard(int? isProjectBased)
        {
            try
            {
                IEnumerable<PendingSO> pendingSo = repo.GetPendingSO(isProjectBased ?? 0, OrganizationId);
                return View(pendingSo);
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        [HttpPost]
        public ActionResult Create(JobCard model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                var data = new JobCardRepository().SaveJobCard(model);
                if (data.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + data;
                    return RedirectToAction("PendingJobCard", new { isProjectBased = model.isProjectBased });
                }
                TempData["error"] = "Some error occured while saving. Please try again.";
                return View(model);
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }

        public void FillBay()
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetBayList();
            ViewBag.BayList = new SelectList(result, "BayId", "BayName");
        }
        public void FillBay1(int JobCardId)
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetBayList1(JobCardId);
            ViewBag.BayList = new SelectList(result, "BayId", "BayName");
        }
        public void FillEmployee()
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetEmployeeList();
            ViewBag.EmployeeList = new SelectList(result, "EmployeeId", "EmployeeName");
        }
        public void FillTaks(int workId)
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetWorkVsTask(workId);
            ViewBag.TaskList = new SelectList(result, "JobCardTaskMasterId", "JobCardTaskName");
        }
        public void FillFreezerUnit()
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetFreezerUnits();
            ViewBag.FreezerUnits = new SelectList(result, "FreezerUnitId", "FreezerUnitName");
        }
        public void FillBox()
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetBoxes();
            ViewBag.Boxes = new SelectList(result, "BoxId", "BoxName");
        }
        //public void FillVehicleRegNo(int saleOrderItemId)
        //{
        //    ViewBag.inpassList = new SelectList(new DropdownRepository().VehicleInPassDropdown(), "Id", "Name");
        //}

        public void FillJCNo(int isProjectBased)
        {
            ViewBag.JCNoList = new SelectList(new DropdownRepository().JCNODropdown(OrganizationId, isProjectBased), "Id", "Name");
        }
        public void FillCustomerinJC(int isProjectBased)
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().JCCustomerDropdown(OrganizationId, isProjectBased), "Id", "Name");
        }
        public ActionResult PreviousList(int ProjectBased, DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            try
            {
                from = from ?? DateTime.Today.AddMonths(-1);
                to = to ?? DateTime.Today;
                return PartialView("_PreviousList", new JobCardRepository().GetAllJobCards(ProjectBased, id, cusid, OrganizationId, from, to));
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }

        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "JobCard.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("JobCardNo");
            ds.Tables["Head"].Columns.Add("JobCardDate");
            ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Unit");
            ds.Tables["Head"].Columns.Add("Customer");
            ds.Tables["Head"].Columns.Add("Technician");
            //-------DT
            ds.Tables["Items"].Columns.Add("TaskDate");
            ds.Tables["Items"].Columns.Add("Employee");
            ds.Tables["Items"].Columns.Add("Description");
            ds.Tables["Items"].Columns.Add("StartTime");
            ds.Tables["Items"].Columns.Add("EndTime");

            JobCardRepository repo = new JobCardRepository();
            var Head = repo.GetJobCardHD(Id);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["JobCardNo"] = Head.JobCardNo;
            dr["JobCardDate"] = Head.JobCardDate.ToString("dd-MMM-yyyy");
            dr["SaleOrderRefNo"] = Head.RegistrationNo;
            dr["CustomerName"] = Head.CustomerName;
            dr["Phone"] = Head.Phone;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Customer"] = Head.Customer;
            dr["Unit"] = Head.FreezerUnitName;
            dr["Technician"] = Head.Technician;
            ds.Tables["Head"].Rows.Add(dr);


            JobCardTaskRepository repo1 = new JobCardTaskRepository();
            var Items = repo1.GetJobCardDT(Id);
            foreach (var item in Items)
            {
                var JCItem = new JobCardTask { TaskDate = item.TaskDate, Employee = item.Employee,
                                               Description = item.Description,StartTime = item.StartTime,EndTime =item.EndTime};

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["TaskDate"] = JCItem.TaskDate.ToString("dd-MMM-yyyy");
                dri["Employee"] = JCItem.Employee;
                dri["Description"] = JCItem.Description;
                dri["StartTime"] = JCItem.StartTime;
                dri["EndTime"] = JCItem.EndTime;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "JobCard.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("JobCard{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Edit(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            JobCard model = new JobCardRepository().GetJobCardDetails2(id, OrganizationId);
            FillBay1(model.JobCardId);
            FillEmployee();
            FillTaks(model.WorkDescriptionId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(JobCard model)
        {
            try
            {
                new JobCardRepository().UpdateJobCard(model);
                TempData["success"] = "Updated Successfully (" + model.JobCardNo + ")";
                return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            FillBay1(model.JobCardId);
            FillEmployee();
            FillTaks(model.WorkDescriptionId);
            return View(model);
        }

        public ActionResult Delete(int JobCardId = 0, int isProjectBased = 0)
        {
            try
            {
                if (JobCardId == 0) return RedirectToAction("Index", "Home");
                JobCard model = new JobCardRepository().GetJobCardDetails2(JobCardId, OrganizationId);
                string ref_no = new JobCardRepository().DeleteJobCard(JobCardId);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = JobCardId });
            }
        }
    }
}