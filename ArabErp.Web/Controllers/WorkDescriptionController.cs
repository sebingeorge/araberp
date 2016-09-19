using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

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
        public ActionResult FillWorkDescriptionList(int? page, string vehiclemodel = "", string freezerunit = "", string box = "")
        {
            //int itemsPerPage = 2;
            int pageNumber = page ?? 1;
            var repo = new WorkDescriptionRepository();
            var List = repo.FillWorkDescriptionList(vehiclemodel.Trim(), freezerunit.Trim(), box.Trim());
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
            workdescription.isNewInstallation = true;
            workdescription.WorkVsItems.Add(new WorkVsItem());
            workdescription.WorkVsTasks.Add(new WorkVsTask());
            return View(workdescription);
        }

        public ActionResult FillProjectWorkDescriptionList(int? page)
        {
            //int itemsPerPage = 2;
            int pageNumber = page ?? 1;
            var repo = new WorkDescriptionRepository();
            var List = repo.FillProjectWorkDescriptionList();
            return View("ProjectWorkDescriptionList", List);
        }

        public ActionResult CreateProjectWorkDescription()
        {
        
            FillItem();
            FillJobCardTaskMaster();
            WorkDescription workdescription = new WorkDescription();
            workdescription.WorkDescriptionRefNo = "WD/" + DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(WorkDescription).Name);
            workdescription.isNewInstallation = true;
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
            return View("Edit", model);
        }


        [HttpPost]
        public ActionResult EditWorkDescription(WorkDescription model)
        {

            FillVehicle();
            FillBox();
            FillFreezerUnit();
            FillItem();
            FillJobCardTaskMaster();

            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new WorkDescriptionRepository();

            var result1 = new WorkDescriptionRepository().CHECK(model.WorkDescriptionId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["WorkDescriptionRefNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result3 = new WorkDescriptionRepository().DeleteWorkDescriptionTask(model.WorkDescriptionId);
                    var result2 = new WorkDescriptionRepository().DeleteWorkDescriptionItem(model.WorkDescriptionId);
                    var result4 = new WorkDescriptionRepository().DeleteWorkDescriptionHD(model.WorkDescriptionId, UserID.ToString());
                    //string id = new WorkDescriptionRepository().InsertWorkDescription(model);
                    var result = new WorkDescriptionRepository().InsertWorkDescription(model);
                    if (result.WorkDescriptionId > 0)
                    {
                        TempData["success"] = "Updated successfully!";
                        TempData["WorkDescriptionRefNo"] = result.WorkDescriptionRefNo;
                        return RedirectToAction("Index");
                        //return View("Edit", model);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (SqlException sx)
                {
                    TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
                }
                catch (NullReferenceException nx)
                {
                    TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
                }
                return RedirectToAction("Index");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new WorkDescriptionRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["WorkDescriptionRefNo"] = null;
                return RedirectToAction("EditWorkDescription", new { id = Id });
                //return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result3 = new WorkDescriptionRepository().DeleteWorkDescriptionTask(Id);
                var result2 = new WorkDescriptionRepository().DeleteWorkDescriptionItem(Id);
                var result4 = new WorkDescriptionRepository().DeleteWorkDescriptionHD(Id, UserID.ToString());

                if (Id > 0)
                {
                    TempData["success"] = "Deleted successfully!";
                    TempData["WorkDescriptionRefNo"] = Id;
                    return RedirectToAction("Index");

                    //TempData["Success"] = "Deleted Successfully!";
                    ////return RedirectToAction("PreviousList");
                    //return RedirectToAction("CreateWorkDescription");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["WorkDescriptionRefNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }


        //[HttpPost]
        //public ActionResult EditWorkDescription(WorkDescription model)
        //{
          
        //    return View("Edit", model);
        //}
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
            var repo = new DropdownRepository();
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
                TempData["WorkDescriptionRefNo"] = result.WorkDescriptionRefNo;
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
                TempData["WorkDescriptionRefNo"] = result.WorkDescriptionRefNo;
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