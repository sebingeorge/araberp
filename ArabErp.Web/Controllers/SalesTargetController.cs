using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class SalesTargetController : BaseController
    {
        // GET: SalesTarget
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        
       {
            ViewBag.Title = "Create";
            SalesTarget Salestarget = new SalesTarget();
            Salestarget.SalesTargetRefNo = "ST/" + DatabaseCommonRepository.GetNextReferenceNo(typeof(SalesTarget).Name);// new SalesTargetRepository().GetRefNo(Salestarget);
            dropdown();
            return View(Salestarget);
        }



        public void dropdown()
        {
            var repo = new SalesTargetRepository();
            var List = repo.FillMonth();
            ViewBag.Month=new SelectList(List,"Id","Name");

            var desrepo = new SalesTargetRepository();
            var workDescription = desrepo.FillWorkDescription();
            ViewBag.workDes = new SelectList(workDescription, "Id", "Name");


            DropdownRepository repos = new DropdownRepository();
            var result = repos.OrganizationDropdown();
            ViewBag.OrganizationName = new SelectList(result, "Id", "Name");

          //  ViewBag.OrganizationName = new SelectList(new DropdownRepository().OrganizationDropdown(OrganizationId), "Id", "Name");
        }
        [HttpPost]
        public ActionResult Create(SalesTarget model)
        {
          
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            model.FyId = new FinancialYearRepository().getfinyear(OrganizationId);
            var repo = new SalesTargetRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "SalesTarget", "SalesTargetRefNo", model.SalesTargetRefNo, null, null);
            if (!isexists)
            {
                var result = new SalesTargetRepository().InsertSalesTarget(model);
                if (result.OrganizationId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["SalesTargetRefNo"] = result.SalesTargetRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    dropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SalesTargetRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                dropdown();
                TempData["error"] = "This Organization Name Alredy Exists!!";
                TempData["SalesTargetRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Edit(int Id)
        {
            dropdown();
            ViewBag.Title = "Edit";
            SalesTarget objSalesTarget = new SalesTargetRepository().GetSalesTarget(Id);
            return View("Create", objSalesTarget);
        }


        [HttpPost]
        public ActionResult Edit(SalesTarget model)
        {

            var repo = new SalesTargetRepository();
            model.CreatedBy = UserID.ToString();
            model.FyId = new FinancialYearRepository().getfinyear(OrganizationId);
            dropdown();
            model.CreatedDate = System.DateTime.Now;
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "SalesTarget", "SalesTargetRefNo", model.SalesTargetRefNo, "SalesTargetId", model.SalesTargetId);
            if (!isexists)
            {
                var result = new SalesTargetRepository().UpdateSalesTarget(model);

                if (result.OrganizationId > 0)
                {
                    TempData["Success"] = "Updated Successfully!";
                    TempData["SalesTargetRefNo"] = result.SalesTargetRefNo;
                    return RedirectToAction("Create");
                }
                else
                {
                    dropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SalesTargetRefNo"] = null;
                    return View("Edit", model);
                }
            }
            else
            {
                dropdown();
                TempData["error"] = "This Sales Target Ref No. Alredy Exists!!";
                TempData["SalesTargetRefNo"] = null;
                return View("Create", model);
            }

        }
        public ActionResult Delete(int Id)
        {
            dropdown();
         ViewBag.Title = "Delete";
         SalesTarget objSalesTarget = new SalesTargetRepository().GetSalesTarget(Id);
          return View("Create", objSalesTarget);
        }

         [HttpPost]
        public ActionResult Delete(SalesTarget model)
        {
            model.CreatedBy = UserID.ToString();
             int result= new SalesTargetRepository().DeleteSalesTarget(model);
             if(result==0)
             {
                 TempData["Success"]="Deleted Successfully";
                 TempData["SalesTargetRefNo"]=model.SalesTargetRefNo;
                 return RedirectToAction("Create");
             }
             else
             {
           if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Sales Target It Is Already In Use";
                    TempData["SalesTargetRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SalesTargetRefNo"] = null;
                }
                return RedirectToAction("Create");
             }
             
        }


        public ActionResult FillSalesTargetList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new SalesTargetRepository();
            var List = repo.FillSalesTargetList();
            return PartialView("SalesTargetListView", List);
        }
    }
}