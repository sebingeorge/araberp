using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class EmployeeController : BaseController
    {
        EmployeeRepository rep = new EmployeeRepository();
        // GET: Employee
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillDesignationDropdown();
            FillCategoryDropdown();
            FillLocationDropdown();
            
            string internalid = DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Employee).Name);
            //model.EmployeeRefNo = "EMP/" + internalid;

            return View(new Employee { EmployeeRefNo = "EMP/" + internalid });
        }

        public ActionResult Edit(int EmployeeId)
        {
            FillDesignationDropdown();
            FillCategoryDropdown();
            FillLocationDropdown();
            Employee model = new EmployeeRepository().GetEmployee(EmployeeId);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(Employee model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];


            var repo = new EmployeeRepository();

            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Employee", "EmployeeName", model.EmployeeName, "EmployeeId", model.EmployeeId);
            if (!isexists)
            {
                var result = new EmployeeRepository().UpdateEmployee(model);
                if (result.EmployeeId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["RefNo"] = result.EmployeeRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillDesignationDropdown();
                    FillCategoryDropdown();
                    FillLocationDropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillDesignationDropdown();
                FillCategoryDropdown();
                FillLocationDropdown();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }
 

        public void FillDesignationDropdown()
        {
            var emp = rep.FillDesignationDropdown();
            ViewBag.EmployeeDesignations = new SelectList(emp ,"Id", "Name");
        }
        public void FillCategoryDropdown()
        {
            var emp = rep.FillCategoryDropdown();
            ViewBag.EmployeeCategory = new SelectList(emp, "Id", "Name");
        }
        public void FillLocationDropdown()
        {
            var emp = rep.FillLocationDropdown();
            ViewBag.EmployeeLocation = new SelectList(emp, "Id", "Name");
        }
        [HttpPost]
        public ActionResult Create(Employee model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var repo = new EmployeeRepository();

            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Employee", "EmployeeName", model.EmployeeName, null,null);
            if (!isexists)
            {
                var result = new EmployeeRepository().Insert(model);
                if (result.EmployeeId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["RefNo"] = result.EmployeeRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    FillDesignationDropdown();
                    FillCategoryDropdown();
                    FillLocationDropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["RefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                FillDesignationDropdown();
                FillCategoryDropdown();
                FillLocationDropdown();
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }

        //    if (result.EmployeeId > 0)
        //    {
        //        TempData["Success"] = "Added Successfully!";
        //        TempData["RefNo"] = result.EmployeeRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        FillDesignationDropdown();
        //        FillCategoryDropdown();
        //        FillLocationDropdown();
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["RefNo"] = null;
        //        return View("Create", model);
        //    }
          
        //}

        public ActionResult Delete(int EmployeeId)
        {

            Employee model = new EmployeeRepository().GetEmployee(EmployeeId);
            FillDesignationDropdown();
            FillCategoryDropdown();
            FillLocationDropdown();
        

            return View("Create",model);


        }
        [HttpPost]
        public ActionResult Delete(Employee model)
        {

            var result = new EmployeeRepository().DeleteEmployee(model);


            if (result.EmployeeId > 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["RefNo"] = model.EmployeeRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                FillDesignationDropdown();
                FillCategoryDropdown();
                FillLocationDropdown();
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult FillEmployeeList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new EmployeeRepository();
            var List = rep.FillEmployeeList();
            return PartialView("_EmployeeListView", List);
        }
    }
}