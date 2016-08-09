using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class WorkDescriptionController : BaseController
    {
        // GET: WorkDescription
        public ActionResult Index()
        {
            FillVehicle();
            FillBox();
            FillFreezerUnit();
            FillItem();
            FillJobCardTaskMaster();
            return View();
        }
        public ActionResult FillWorkDescriptionList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new WorkDescriptionRepository();
            var List = repo.FillWorkDescriptionList();
            return PartialView("WorkDescriptionGrid", List);
        }
        public ActionResult CreateWorkDescription()
        {

            FillVehicle();
            FillBox();
            FillFreezerUnit();
            FillItem();
            FillJobCardTaskMaster();
           
            WorkDescription workdescription = new WorkDescription();
            workdescription.WorkDescriptionRefNo ="WD/" + DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(WorkDescription).Name);

            workdescription.WorkVsItems.Add(new WorkVsItem());
            workdescription.WorkVsTasks.Add(new WorkVsTask());
            return View(workdescription);
        }

        public ActionResult CreateProjectWorkDescription()
        {
        
            FillItem();
            FillJobCardTaskMaster();
            WorkDescription workdescription = new WorkDescription();

            workdescription.isProjectBased = true;
            workdescription.WorkVsItems.Add(new WorkVsItem());
            workdescription.WorkVsTasks.Add(new WorkVsTask());
            return View("CreateWorkDescription",workdescription);
        }
        public ActionResult EditWorkDescription(int Id)
        {
            FillVehicle();
            FillBox();
            FillFreezerUnit();
            FillItem();
            FillJobCardTaskMaster();
            WorkDescription model = new WorkDescriptionRepository().GetWorkDescription(Id);
            return View("CreateWorkDescription", model);
        }
        [HttpPost]
        public ActionResult EditWorkDescription(WorkDescription model)
        {
            //model.OrganizationId = OrganizationId;
            //model.CreatedDate = System.DateTime.Now;
            //model.CreatedBy = UserID.ToString();
            //var result = new WorkDescriptionRepository().UpdateWorkDescription(model);
            //if (result.VehicleModelId > 0)
            //{
            //    TempData["Success"] = "Updated Successfully!";
            //    TempData["RefNo"] = result.VehicleModelRefNo;
            //    return RedirectToAction("Create");
            //}
            //else
            //{

            //    TempData["error"] = "Oops!!..Something Went Wrong!!";
            //    TempData["RefNo"] = null;

            //    return View("Create", model);
            //}
            return View("Create", model);
        }
        public void FillItem()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItem();
            ViewBag.ItemList = new SelectList(List, "Id", "Name");
        }

        public void FillJobCardTaskMaster()
        {
            JobCardTaskMasterRepository Repo = new JobCardTaskMasterRepository();
            var List = Repo.FillJobCardTaskMaster();
            ViewBag.JobCardTaskMasterList = new SelectList(List, "Id", "Name");
        }
        public void FillVehicle()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillVehicle();
            ViewBag.vehiclelist = new SelectList(list, "Id", "Name");
        }
        public void FillBox()
        {
            var repo = new BoxRepository();
            var list = repo.FillBox();
            ViewBag.boxlist = new SelectList(list, "BoxId", "BoxName");
        }
        public void FillFreezerUnit()
        {
            var repo = new FreezerUnitRepository();
            var list = repo.FillFreezerUnit();
            ViewBag.FreezerUnitlist = new SelectList(list, "FreezerUnitId", "FreezerUnitName");
        }
        [HttpPost]
        public ActionResult CreateWorkDescription(WorkDescription model)
        {


            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var result = new WorkDescriptionRepository().InsertWorkDescription(model);

           


            if (result.WorkDescriptionId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["RefNo"] = result.WorkDescriptionRefNo;
                return RedirectToAction("CreateWorkDescription");
            }
            else
            {
                FillVehicle();
                FillBox();
                FillFreezerUnit();
                FillItem();
                FillJobCardTaskMaster();
                WorkDescription workdescription = new WorkDescription();
                workdescription.WorkVsItems.Add(new WorkVsItem());
                workdescription.WorkVsTasks.Add(new WorkVsTask());

                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("CreateWorkDescription", model);
            }
        }

        [HttpPost]
        public ActionResult CreateProjectWorkDescription(WorkDescription model)
        {


            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var result = new WorkDescriptionRepository().InsertProjectWorkDescription(model);




            if (result.WorkDescriptionId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["RefNo"] = result.WorkDescriptionRefNo;
                return RedirectToAction("CreateProjectWorkDescription");
            }
            else
            {
                FillVehicle();
                FillBox();
                FillFreezerUnit();
                FillItem();
                FillJobCardTaskMaster();
                WorkDescription workdescription = new WorkDescription();
                workdescription.WorkVsItems.Add(new WorkVsItem());
                workdescription.WorkVsTasks.Add(new WorkVsTask());

                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("CreateProjectWorkDescription", model);
            }
        }

        public JsonResult GetUnit(int itemId)
        {
            return Json(new ItemRepository().GetPartNoUnit(itemId), JsonRequestBehavior.AllowGet);
        }

    }
}