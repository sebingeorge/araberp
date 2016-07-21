using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobCardDailyActivityController : BaseController
    {
        // GET: JobCardDailyActivity
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PendingJobcardTasks()
        {
            return View((new JobCardDailyActivityRepository()).PendingJobcardTasks());
        }
        public ActionResult Create(int Id)
        {
            JobCardRepository jcRepo = new JobCardRepository();
            EmployeeRepository emRepo = new EmployeeRepository();
            JobCard jc = jcRepo.GetDetailsById(Id, null);
            FillTaks(jc.WorkDescriptionId);
            JobCardDailyActivity model = new JobCardDailyActivity();
            model.CreatedDate = DateTime.Now;
            model.JobCardDailyActivityDate = DateTime.Now;
            model.JobCardDailyActivityTask = new List<JobCardDailyActivityTask>();
            model.JobCardDailyActivityTask.Add(new JobCardDailyActivityTask() { TaskStartDate = DateTime.Now, TaskEndDate = DateTime.Now});
            Employee emp = emRepo.GetEmployee(jc.EmployeeId);
            model.EmployeeId = jc.EmployeeId;
            model.EmployeeName = emp.EmployeeName;
            model.JobCardNo = jc.JobCardNo;
            model.JobCardId = Id;
            ViewBag.isTxnPending = true;
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(JobCardDailyActivity model)
        {
            if(ModelState.IsValid)
            {
                JobCardDailyActivityRepository repo = new JobCardDailyActivityRepository();
                model.CreatedDate = DateTime.Now;
                var id = repo.InsertJobCardDailyActivity(model);
                //return RedirectToAction("PendingJobcardTasks");
                JobCard jc = new JobCardRepository().GetDetailsById(model.JobCardId, null);
                FillTaks(jc.WorkDescriptionId);
                ViewBag.isTxnPending = false;
                model.JobCardDailyActivityId = id;
                return View(model);
            }
            else
            {
                return View("Create", new { Id = model.JobCardId });
            }
        }
        public void FillTaks(int workId)
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetWorkVsTask(workId);
            ViewBag.TaskList = new SelectList(result, "JobCardTaskMasterId", "JobCardTaskName");
        }

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase file, int? id, int? index)
        {
            string fileName = UploadImage(file);
            new JobCardDailyActivityRepository().UpdateImageName(fileName, id, index);
            

            //RegistrationViewModel reg = _registrationManager.GetChildDetails(Child_Id);

            //_registrationManager.UpdateChildImage(Child_Id, fileName, image_type);

            //if (image_type == "child")
            //{
            //    if (reg != null && reg.childImagepath != null && reg.childImagepath != "")
            //    {
            //        string oldpath = Path.Combine(Server.MapPath("~/App_Images/"), reg.childImagepath);
            //        System.IO.File.Delete(oldpath);
            //    }
            //}
            //if (image_type == "parent")
            //{
            //    if (reg != null && reg.parentImagepath != null && reg.parentImagepath != "")
            //    {
            //        string oldpath = Path.Combine(Server.MapPath("~/Content/img/"), reg.parentImagepath);
            //        System.IO.File.Delete(oldpath);
            //    }
            //}
            //return Json(fileName, JsonRequestBehavior.AllowGet);
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        private string UploadImage(HttpPostedFileBase file)
        {
            string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string qualifiedName = Server.MapPath("~/App_Images/") + uniqueName;
            file.SaveAs(qualifiedName);
            return uniqueName;
        }
    }
}