using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class TaskController : BaseController
    {
        // GET: Task
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult FillTaskList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new TaskRepository();
            var List = repo.FillTaskList();
            return PartialView("TaskListView", List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            JobCardTaskMaster JobCardTaskMaster = new JobCardTaskMaster();
            JobCardTaskMaster.JobCardTaskRefNo = new TaskRepository().GetRefNo(JobCardTaskMaster);
            return View(JobCardTaskMaster);
        }
        [HttpPost]
        public ActionResult Create(JobCardTaskMaster model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new TaskRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "JobCardTaskMaster", "JobCardTaskName", model.JobCardTaskName,null,null);
            if (!isexists)
            {
                var result = new TaskRepository().InsertTask(model);
                if (result.JobCardTaskMasterId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["JobCardTaskRefNo"] = result.JobCardTaskRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["JobCardTaskRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This  Name Alredy Exists!!";
                TempData["JobCardTaskRefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            JobCardTaskMaster objTask = new TaskRepository().GetTask(Id);
            return View("Create", objTask);
        }

        [HttpPost]
        public ActionResult Edit(JobCardTaskMaster model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new TaskRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "JobCardTaskMaster", "JobCardTaskName", model.JobCardTaskName, "JobCardTaskMasterId", model.JobCardTaskMasterId);
            if (!isexists)
            {
                var result = new TaskRepository().UpdateTask(model);
                if (result.JobCardTaskMasterId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["JobCardTaskRefNo"] = result.JobCardTaskRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["JobCardTaskRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This  Name Alredy Exists!!";
                TempData["JobCardTaskRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            JobCardTaskMaster objTask = new TaskRepository().GetTask(Id);
            return View("Create", objTask);

        }

        [HttpPost]
        public ActionResult Delete(JobCardTaskMaster model)
        {
            int result = new TaskRepository().DeleteTask(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["JobCardTaskRefNo"] = model.JobCardTaskRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Task. It Is Already In Use";
                    TempData["JobCardTaskRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["JobCardTaskRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }


    }
}