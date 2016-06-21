using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobOrderCompletionController : Controller
    {
        // GET: JobOrderCompletion
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(int Id)
        {
            JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
            var jobcard = repo.GetJobCardCompletion(Id);
            FillEmployee();
            FillTaks(jobcard.WorkDescriptionId ?? 0);

            jobcard.JobCardDate = DateTime.Now;
            return View(jobcard);
        }
        public ActionResult PendingJobOrderCompletion()
        {
            JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
            var result = repo.GetPendingJobOrder();
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
            JobOrderCompletionRepository repo = new JobOrderCompletionRepository();
            repo.UpdateJobCardCompletion(model);
            return RedirectToAction("PendingJobOrderCompletion");
        }
    }
}