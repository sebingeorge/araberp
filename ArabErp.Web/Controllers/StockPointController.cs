using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class StockPointController : BaseController
    {
        // GET: StockPoint
        public ActionResult Index()
        {
            return View();
            
        }
        public ActionResult Create()
        {
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Stockpoint).Name);
            return View(new Stockpoint { StockPointRefNo = "EMPC/" + internalid });
        }
           [HttpPost]
        public ActionResult Create(Stockpoint model)
        {
            var repo = new StockpointRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "StockPoint", "StockPointName", model.StockPointName, null, null);
            if (!isexists)
            {
                var result = new StockpointRepository().InsertStockpoint(model);
                if (result.StockPointId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["RefNo"] = result.StockPointRefNo;
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
        //    var repo = new StockpointRepository();
        //    var result = new StockpointRepository().InsertStockpoint(model);
        //    if (result.StockPointId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["RefNo"] = result.StockPointRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {

        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["RefNo"] = null;
        //        return View("Create", model);
        //    }
        //}
           public ActionResult Edit(int Id)
           {

               Stockpoint model = new StockpointRepository().GetStockpoint(Id);
               return View("Create", model);
           }
           [HttpPost]
           public ActionResult Edit(Stockpoint model)
           {
               model.OrganizationId = OrganizationId;
               model.CreatedDate = System.DateTime.Now;
               model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

               var repo = new StockpointRepository();
               bool isexists = repo.IsFieldExists(repo.ConnectionString(), "StockPoint", "StockPointName", model.StockPointName, "StockPointId", model.StockPointId);
               if (!isexists)
               {
                   var result = new StockpointRepository().UpdateStockpoint(model);
                   if (result.StockPointId > 0)
                   {

                       TempData["Success"] = "Updated Successfully!";
                       TempData["RefNo"] = result.StockPointRefNo;
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

           //    var result = new StockpointRepository().UpdateStockpoint(model);
           //    if (result.StockPointId > 0)
           //    {
           //        TempData["Success"] = "Updated Successfully!";
           //        TempData["RefNo"] = result.StockPointRefNo;
           //        return RedirectToAction("Create");
           //    }
           //    else
           //    {

           //        TempData["error"] = "Oops!!..Something Went Wrong!!";
           //        TempData["RefNo"] = null;

           //        return View("Create", model);
           //    }

           //}
           public ActionResult Delete(int Id)
           {

               Stockpoint model = new StockpointRepository().GetStockpoint(Id);
               return View("Create", model);
           }
           [HttpPost]
           public ActionResult Delete(Stockpoint model)
           {

               var result = new StockpointRepository().DeleteStockpoint(model);


               if (result.StockPointId > 0)
               {
                   TempData["Success"] = "Deleted Successfully!";
                   TempData["RefNo"] = model.StockPointRefNo;
                   return RedirectToAction("Create");
               }
               else
               {
                   TempData["error"] = "Oops!!..Something Went Wrong!!";
                   TempData["RefNo"] = null;
                   return View("Create", model);
               }

           }
        public ActionResult FillStockpointList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo=new StockpointRepository ();
            var List=repo.FillStockpointList();
            return PartialView("StockpointListView",List);
        }

    }
}