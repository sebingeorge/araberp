using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class CostingParametersController : BaseController
    {
        // GET: CostingParameters
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            CostingParameters costing = new CostingParameters();
            costing.CostingRefNo = new CostingParametersRepository().GetRefNo(costing);
            return View(costing);
        }

        public ActionResult FillCostingParameterList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new CostingParametersRepository();
            var List = rep.FillCostingParameterList();
            return PartialView("CostingParametersListView", List);
        }
        [HttpPost]
        public ActionResult Create(CostingParameters model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CostingParametersRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "CostingParameters", "Description", model.Description, null, null);
            if (!isexists)
            {
                var result = new CostingParametersRepository().InsertCosting(model);
                if (result.CostingId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["CostingRefNo"] = result.CostingRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CostingRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This  Name Alredy Exists!!";
                TempData["CostingRefNo"] = null;
                return View("Create", model);
            }

        }
        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            CostingParameters objCosting = new CostingParametersRepository().GetCosting(Id);
            return View("Create", objCosting);
        }
         [HttpPost]
        public ActionResult Edit(CostingParameters model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new CostingParametersRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(),  "CostingParameters", "Description", model.Description, "CostingId", model.CostingId);
            if (!isexists)
            {
                var result = new CostingParametersRepository().UpdateCostingParameters(model);
                if (result.CostingId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["CostingRefNo"] = result.CostingRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CostingRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This  Name Alredy Exists!!";
                TempData["CostingRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            CostingParameters objCostingParameters = new CostingParametersRepository().GetCosting(Id);
            return View("Create", objCostingParameters);

        }

        [HttpPost]
        public ActionResult Delete(CostingParameters model)
        {
            int result = new CostingParametersRepository().DeleteCostingParameters(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["CostingRefNo"] = model.CostingRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete Costing parameter It Is Already In Use";
                    TempData["CostingRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["CostingRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}