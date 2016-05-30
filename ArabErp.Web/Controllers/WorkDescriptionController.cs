using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class WorkDescriptionController : Controller
    {
        // GET: WorkDescription
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CreateWorkDescription()
        {
           
            FillVehicle();
            FillBox();
            FillFreezerUnit();
            WorkDescription workdescription = new WorkDescription();


            workdescription.WorkVsItems.Add(new WorkVsItem());
            workdescription.WorkVsTasks.Add(new WorkVsTask());
            return View(workdescription);
        }
        public void FillVehicle()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillVehicle();
            ViewBag.vehiclelist = new SelectList(list, "Id", "Name");
        }
        public void FillBox()
        {
            var repo = new BoxRepository();
            var list = repo.FillBox();
            ViewBag.boxlist = new SelectList(list, "Id", "Name");
        }
        public void FillFreezerUnit()
        {
            var repo = new FreezerUnitRepository();
            var list = repo.FillFreezerUnit();
            ViewBag.FreezerUnitlist = new SelectList(list, "Id", "Name");
        }
        [HttpPost]
        public ActionResult Save(WorkDescription model)
        {


            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new WorkDescriptionRepository().InsertWorkDescription(model);

            FillVehicle();
            FillBox();
            FillFreezerUnit();
            WorkDescription workdescription = new WorkDescription();


            workdescription.WorkVsItems.Add(new WorkVsItem());
            workdescription.WorkVsTasks.Add(new WorkVsTask());
           

            return View("CreateWorkDescription", workdescription);
        }
        }
}