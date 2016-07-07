using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class VehicleModelController : BaseController
    {
        // GET: VehicleModel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(VehicleModel).Name);
            return View(new VehicleModel { VehicleModelRefNo = "VM/" + internalid });
        }
          [HttpPost]
        public ActionResult Create(VehicleModel model)
        {
            var repo = new VehicleModelRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "VehicleModel", "VehicleModelName", model.VehicleModelName, null, null);
            if (!isexists)
            {
                var result = new VehicleModelRepository().InsertVehicleModel(model);
                if (result.VehicleModelId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["RefNo"] = result.VehicleModelRefNo;
                    return RedirectToAction("Create");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This  Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

          public ActionResult Edit(int Id)
          {

              VehicleModel model = new VehicleModelRepository().GetVehicleModel(Id);
              return View("Create", model);
          }
          [HttpPost]
          public ActionResult Edit(VehicleModel model)
          {
              model.OrganizationId = 1;
              model.CreatedDate = System.DateTime.Now;
              model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

              var repo = new VehicleModelRepository();
              bool isexists = repo.IsFieldExists(repo.ConnectionString(), "VehicleModel", "VehicleModelName", model.VehicleModelName, "VehicleModelId", model.VehicleModelId);
              if (!isexists)
              {
                  var result = new VehicleModelRepository().UpdateVehicleModel(model);
                  if (result.VehicleModelId > 0)
                  {

                      TempData["Success"] = "Updated Successfully!";
                      TempData["RefNo"] = result.VehicleModelRefNo;
                      return RedirectToAction("Create");
                  }

                  else
                  {

                      TempData["error"] = "Oops!!..Something Went Wrong!!";
                      TempData["RefNo"] = null;
                      return View("Create", model);
                  }

              }
              else
              {

                  TempData["error"] = "This  Name Alredy Exists!!";
                  TempData["RefNo"] = null;
                  return View("Create", model);
              }

          }

        
          public ActionResult Delete(int Id)
          {

              VehicleModel model = new VehicleModelRepository().GetVehicleModel(Id);
              return View("Create", model);
          }
          [HttpPost]
          public ActionResult Delete(VehicleModel model)
          {

              var result = new VehicleModelRepository().DeleteVehicleModel(model);


              if (result.VehicleModelId > 0)
              {
                  TempData["Success"] = "Deleted Successfully!";
                  TempData["RefNo"] = model.VehicleModelRefNo;
                  return RedirectToAction("Create");
              }
              else
              {
                  TempData["error"] = "Oops!!..Something Went Wrong!!";
                  TempData["RefNo"] = null;
                  return View("Create", model);
              }

          }
        public ActionResult FillVehicleModelList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new VehicleModelRepository();
            var List = repo.FillVehicleModelList();
            return PartialView("VehicleModelListView",List);
        }

    }
}