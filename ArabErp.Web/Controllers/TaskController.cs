using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class TaskController : Controller
    {
        // GET: Task
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult create()
        {
            return View();
        }

        public ActionResult Save(Task objTask)
        {
            var repo = new TaskRepository();
            new TaskRepository().InsertTask(objTask);
            return View("Create");
        }

        public ActionResult FillTaskList()
        {
            var repo = new TaskRepository();
            var List = repo.FillTaskList();
            return PartialView("TaskListView", List);
        }
    }
}