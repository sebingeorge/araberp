using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult Index(int isProjectBased)
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
                JobCardRepository repo = new JobCardRepository();
                SaleOrderRepository soRepo = new SaleOrderRepository();
                isProjectBased = soRepo.IsProjectOrVehicle(Id ?? 0);
                JobCard model = repo.GetJobCardDetails(Id ?? 0, isProjectBased ?? 0);
                model.JobCardNo = "JC/" + DatabaseCommonRepository.GetNextReferenceNo(typeof(JobCard).Name);
                model.isProjectBased = isProjectBased ?? 0;
                model.JobCardTasks = new List<JobCardTask>();
                model.JobCardTasks.Add(new JobCardTask() { TaskDate = DateTime.Now });
                model.JobCardDate = DateTime.Now;
                model.RequiredDate = DateTime.Now;
                FillBay();
                FillEmployee();
                FillTaks(model.WorkDescriptionId);
                FillFreezerUnit();
                FillBox();
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
    }
}