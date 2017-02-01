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
        
        public ActionResult PendingJobcardTasks(int type = 0)
       {
            return View();
        }
        public ActionResult PendingJobcards(int type = 0, string RegNo = "", string jcno = "",string customer="")
        {
            return PartialView((new JobCardDailyActivityRepository()).PendingJobcardTasks(type, OrganizationId, RegNo, jcno,customer));
        }
        public ActionResult Create(int Id)
        {
            JobCardRepository jcRepo = new JobCardRepository();
            EmployeeRepository emRepo = new EmployeeRepository();
            JobCard jc = jcRepo.GetDetailsById(Id, null);
            //FillTaks(jc.WorkDescriptionId);
            JobCardDailyActivity model = new JobCardDailyActivity();
             if (jc.isProjectBased==1)
            {
                model.JobCardDailyActivityRefNo = DatabaseCommonRepository.GetNextDocNo(38, OrganizationId);
                //FillTasks();
                FillTasks(jc.isProjectBased);
                FillEmployees();

            }
            else
            {
                model.JobCardDailyActivityRefNo = DatabaseCommonRepository.GetNextDocNo(27, OrganizationId);
            }
            model.CreatedDate = DateTime.Now;
            model.JobCardDailyActivityDate = DateTime.Now;
            model.isProjectBased = jc.isProjectBased;
            model.CustomerName = jc.CustomerName;
            if (model.isProjectBased == 0)
                model.JobCardDailyActivityTask = new JobCardDailyActivityRepository().GetJobCardTasksForDailyActivity(Id, OrganizationId);
            else
            {
                model.JobCardDailyActivityTask = new List<JobCardDailyActivityTask>();
                model.JobCardDailyActivityTask.Add(new JobCardDailyActivityTask
                {
                    TaskStartDate = model.JobCardDailyActivityDate,
                    TaskEndDate = model.JobCardDailyActivityDate
                });
            }

            //if (model.JobCardDailyActivityTask.Count > 0)
            //{
            //    foreach (var item in model.JobCardDailyActivityTask)
            //    {
            //        item.StartTime = "00:00"; item.EndTime = "00.00";
            //    }
            //}
            //model.JobCardDailyActivityTask = new List<JobCardDailyActivityTask>();
            //model.JobCardDailyActivityTask.Add(new JobCardDailyActivityTask() { TaskStartDate = DateTime.Now, TaskEndDate = DateTime.Now});
            Employee emp = emRepo.GetEmployee(jc.EmployeeId);
            model.EmployeeId = jc.EmployeeId;
            model.EmployeeName = emp.EmployeeName;
            model.JobCardNo = jc.JobCardNo;
            model.JobCardId = Id;
            ViewBag.isTxnPending = true;
            return View(model);
        }

        private void FillEmployees()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }

        private void FillTasks()
        {
            ViewBag.taskList = new SelectList(new DropdownRepository().TaskDropdown(OrganizationId), "Id", "Name");
        }
        private void FillTasks(int isProjectBased)
        {
            ViewBag.taskList = new SelectList(new DropdownRepository().TaskDropdown1(isProjectBased), "Id", "Name");
        }
        [HttpPost]
        public ActionResult Create(JobCardDailyActivity model)
        {
            try
            {
                model.CreatedBy = UserID.ToString();
                model.OrganizationId = OrganizationId;
                model.CreatedDate = DateTime.Now;
                //if (ModelState.IsValid)
                //{
                    JobCardDailyActivityRepository repo = new JobCardDailyActivityRepository();
                    model.CreatedDate = DateTime.Now;
                    int id = 0;
                    if (model.isProjectBased == 0)
                        id = repo.InsertJobCardDailyActivity(model);
                    else
                        id = repo.InsertProjectDailyActivity(model);
                    if (id == 0)
                    {
                        TempData["error"] = "Some error occured while saving. Please try again.";
                        return View(model);
                    }
                    TempData["success"] = "Saved Successfully.";
                    TempData["previousAction"] = "Create";
                    return RedirectToAction("Details", new { id = id, type = model.isProjectBased });
                //}
                //else
                //{
                //    return View("Create", new { Id = model.JobCardId });
                //}
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                return View(model);
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
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        private string UploadImage(HttpPostedFileBase file)
        {
            string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string qualifiedName = Server.MapPath("~/App_Images/") + uniqueName;
            file.SaveAs(qualifiedName);
            return uniqueName;
        }

        public ActionResult Details(int id=0, int type = 0)//JobCardDailyActivityId
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request";
                return RedirectToAction("Index", "Home");
            }
            try
            {
                var model = new JobCardDailyActivityRepository().GetJobCardDailyActivity(id);
                if (model == null)
                {
                    TempData["error"] = "Could not find the requested data. Please try again.";
                }
                return View(model);
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PreviousList");
        }

        [HttpPost]
        public ActionResult Details(JobCardDailyActivity model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            try
            {
                new JobCardDailyActivityRepository().UpdateJobCardDailyActivity(model);
                TempData["success"] = "Updated Successfully (" + model.JobCardDailyActivityRefNo + ")";
                return RedirectToAction("PreviousList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }

        public ActionResult Delete(int Id)
        {
            try
            {
                if (Id == 0) return RedirectToAction("PreviousList");
                JobCardDailyActivity model = new JobCardDailyActivityRepository().DeleteJobCardDailyActivity(Id);
                if (model != null)
                {
                    string filepath;
                    try
                    {
                        filepath = Server.MapPath("~/App_Images/" + model.Image1);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch { }

                    try
                    {
                        filepath = Server.MapPath("~/App_Images/" + model.Image2);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch { }

                    try
                    {
                        filepath = Server.MapPath("~/App_Images/" + model.Image3);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch { }

                    try
                    {
                        filepath = Server.MapPath("~/App_Images/" + model.Image4);
                        if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    }
                    catch { }

                    TempData["success"] = "Deleted Successfully (" + model.JobCardDailyActivityRefNo + ")";
                    return RedirectToAction("PreviousList");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Details", new { id = Id });
            }
        }

        public ActionResult PreviousList(int type = 0)
        {
            return View(new JobCardDailyActivityRepository().GetJobCardDailyActivitys(OrganizationId, type));
        }
      
    }
}