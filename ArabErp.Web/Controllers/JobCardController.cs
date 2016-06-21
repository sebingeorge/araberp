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
    public class JobCardController : Controller
    {
        JobCardRepository repo;
        public JobCardController()
        {
            repo = new JobCardRepository();
        }
        // GET: JobCard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(int? Id)
        {
            JobCardRepository repo = new JobCardRepository();
            JobCard model = repo.GetJobCardDetails(Id ?? 0);
            model.JobCardTasks = new List<JobCardTask>();
            model.JobCardTasks.Add(new JobCardTask());
            model.JobCardDate = DateTime.Now;
            FillBay();
            FillEmployee();
            FillTaks(model.WorkDescriptionId);
            FillFreezerUnit();
            FillBox();
            FillVehicleRegNo();
            return View(model);
        }
        public ActionResult PendingJobCard()
        {
            IEnumerable<PendingSO> pendingSo = repo.GetPendingSO();
            return View(pendingSo);
        }

        public ActionResult Save(JobCard jc)
        {
            var data = new JobCardRepository().SaveJobCard(jc);
            return RedirectToAction("PendingJobCard");
            // return View();
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
        public void FillVehicleRegNo()
        {
            ViewBag.inpassList = new SelectList(new DropdownRepository().VehicleInPassDropdown(), "Id", "Name");
        }
    }
}