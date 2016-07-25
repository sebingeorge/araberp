using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class AdditionOrDeductionController : BaseController
    {
        // GET: AdditionOrDeduction
        public ActionResult Index()
        {
            return View();
            //AdditionOrDeductionRepository _repo = new AdditionOrDeductionRepository();
            //return View(_repo.GetAdditionDeduction());
        }

        public ActionResult FillAdditionDeductionList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new AdditionOrDeductionRepository();
            var List = rep.FillAdditionDeductionList();
            return PartialView("AdditionDeductionListView", List);
        }

        public void FillAdditionDeduction()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 1, Name = "Addition" });
            types.Add(new Dropdown { Id = 2, Name = "Deduction" });
            ViewBag.AdditionDeduction = new SelectList(types, "Id", "Name");
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            AdditionOrDeduction AdditionOrDeduction = new AdditionOrDeduction();
            AdditionOrDeduction.AddDedRefNo = new AdditionOrDeductionRepository().GetRefNo(AdditionOrDeduction);
            FillAdditionDeduction();
            return View(AdditionOrDeduction);

        }
      
        [HttpPost]
        public ActionResult Create(AdditionOrDeduction model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new AdditionOrDeductionRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "AdditionDeduction", "AddDedName", model.AddDedName,null,null);
            if (!isexists)
            {
                var result = new AdditionOrDeductionRepository().Insert(model);
                if (result.AddDedId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["AddDedRefNo"] = result.AddDedRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillAdditionDeduction();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["AddDedRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillAdditionDeduction();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["AddDedRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new AdditionOrDeductionRepository().Insert(model);

        //    if (result.AddDedId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["AddDedRefNo"] = result.AddDedRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["AddDedRefNo"] = null;
        //        return View("Create", model);
        //    }
        //}

        public ActionResult Edit(int Id)
        {
            FillAdditionDeduction();
            ViewBag.Title = "Edit";
            AdditionOrDeduction objAdditionOrDeduction = new AdditionOrDeductionRepository().GetAdditionOrDeduction(Id);
            return View("Create", objAdditionOrDeduction);
        }

        [HttpPost]
        public ActionResult Edit(AdditionOrDeduction model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


            var repo = new AdditionOrDeductionRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "AdditionDeduction", "AddDedName", model.AddDedName, "AddDedId", model.AddDedId);
            if (!isexists)
            {
                var result = new AdditionOrDeductionRepository().Update(model);
                if (result.AddDedId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["AddDedRefNo"] = result.AddDedRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillAdditionDeduction();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["AddDedRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillAdditionDeduction();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["AddDedRefNo"] = null;
                return View("Create", model);
            }

        }

        //    var result = new AdditionOrDeductionRepository().Update(model);

        //    if (result.AddDedId > 0)
        //    {
        //        TempData["Success"] = "Updated Successfully!";
        //        TempData["AddDedRefNo"] = result.AddDedRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["AddDedRefNo"] = null;
        //        return View("Edit", model);
        //    }

        //}

        public ActionResult Delete(int Id)
        {
            FillAdditionDeduction();
            ViewBag.Title = "Delete";
            AdditionOrDeduction objAdditionOrDeduction = new AdditionOrDeductionRepository().GetAdditionOrDeduction(Id);
            return View("Create", objAdditionOrDeduction);

        }

        [HttpPost]
        public ActionResult Delete(AdditionOrDeduction model)
        {
            int result = new AdditionOrDeductionRepository().Delete(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["AddDedRefNo"] = model.AddDedRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Addition/Deduction It Is Already In Use";
                    TempData["AddDedRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["AddDedRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

    }
}