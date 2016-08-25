using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobOrderCompletionController : BaseController
    {
        // GET: JobOrderCompletion
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(int Id, int isProjectBased)
        {
            JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
            var jobcard = repo.GetJobCardCompletion(Id, isProjectBased);
            FillEmployee();
            FillTaks(jobcard.WorkDescriptionId ?? 0);

            jobcard.JobCardDate = DateTime.Now;
            jobcard.JobCardCompletedDate = DateTime.Now;
            jobcard.WarrentyPeriod = DateTime.Now;
            jobcard.isProjectBased = isProjectBased;
            //ViewBag.type = 1;
            return View(jobcard);
        }
        public ActionResult PendingJobOrderCompletion(int? isProjectBased)
        {
            JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
            var result = repo.GetPendingJobOrder(isProjectBased);
            return View(result);
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
        public ActionResult Save(JobCardCompletion model)
        {
            try
            {
                JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
                repo.UpdateJobCardCompletion(model, UserID.ToString());
                TempData["success"] = "Saved Successfully";
                return RedirectToAction("PendingJobOrderCompletion", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }
        }
    }
}