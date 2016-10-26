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
    public class JobCardQCController : BaseController
    {
        JobCardQCRepository JobCardQCRepo = new JobCardQCRepository();
        JobCardQCParamRepository JobCardQCParamRepo = new JobCardQCParamRepository();
        // GET: JobCardQC
        public ActionResult Index()
        {
            FillJCQcNo();
            return View();
        }

        public ActionResult Create(int Id, string No, DateTime JcDate, string Customer, string VehicleModel)
        {
            try
            {
                string internalId = "";
                internalId = DatabaseCommonRepository.GetNextDocNo(17, OrganizationId);
                FillEmployee();
                JobCardQC objJCQC = new JobCardQC();
                objJCQC.JobCardQCRefNo = internalId;
                objJCQC.JobCardNo = No;
                objJCQC.JobCardId = Id;
                objJCQC.CurrentDate = System.DateTime.Today;
                objJCQC.JcDate = JcDate;
                objJCQC.Customer = Customer;
                objJCQC.VehicleModel = VehicleModel;
                objJCQC.JobCardQCParams = JobCardQCParamRepo.GetJobCardQCParamList();
                return View(objJCQC);
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        public void FillJobCard()
        {

            var List = JobCardQCRepo.FillJobCard();
            ViewBag.JobCardList = new SelectList(List, "Id", "Name");

        }
        public void FillEmployee()
        {
            var List = JobCardQCRepo.FillEmployee();
            ViewBag.EmployeeList = new SelectList(List, "Id", "Name");
        }
        public ActionResult Save(JobCardQC model)
        {

            if (!ModelState.IsValid)
            {
                FillEmployee();
                return View("Create", model);
            }
            //try
            //{
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            if (new JobCardQCRepository().InsertJobCardQC(model) > 0)
            {
                TempData["Success"] = "Added Successfully! Reference No. is " + model.JobCardQCRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                return View(new JobCardQC { JobCardQCId = model.JobCardQCId });
            }

        }

        public ActionResult PendingJobCardQC()
        {
            try
            {

                JobCardQCRepository repo = new JobCardQCRepository();
                var result = repo.GetPendingJobCardQC();
                return View("PendingJobCardQC", result);
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            try
            {
                from = from ?? DateTime.Today.AddMonths(-1);
                to = to ?? DateTime.Today;
                return PartialView("_PreviousList", new JobCardQCRepository().GetPreviousList(id, cusid, OrganizationId, from, to));
            }

            catch (Exception ex)
            {
                string ErrorMessage = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message != null)
                    {
                        ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
                    }
                }
                ViewData["Error"] = ErrorMessage;
                return View("ShowError");
            }
        }
        public void FillJCQcNo()
        {
            ViewBag.JCQcNoList = new SelectList(new DropdownRepository().JCQcNODropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    FillEmployee();
                    JobCardQC JobCardQC = new JobCardQC();
                    JobCardQC = new JobCardQCRepository().GetJobCardQC(id);
                    JobCardQC.JobCardQCParams = JobCardQCParamRepo.GetJobCardQCParamDt(id);

                    return View(JobCardQC);
                }
                else
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    TempData["success"] = "";
                }
            }
            catch (InvalidOperationException iox)
            {
                TempData["error"] = "Sorry, we could not find the requested item. Please try again.|" + iox.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please try again after sometime.|" + sx.Message;
            }
            catch (NullReferenceException nx)
            {
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }

            TempData["success"] = "";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(JobCardQC model)
        {
            FillEmployee();

            try
            {

                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;

                if (model.JobCardQCParams != null && model.JobCardQCParams.Count > 0)
                {
                    new JobCardQCParamRepository().DeleteJobCardQCParam(model.JobCardQCId);
                }

                new JobCardQCRepository().UpdateJobCardQC(model);

                TempData["success"] = "Updated Successfully (" + model.JobCardQCRefNo + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }

        public ActionResult Delete(int JobCardQCId = 0)
        {
            try
            {
                if (JobCardQCId == 0) return RedirectToAction("Index", "Home");
                string ref_no = new JobCardQCRepository().DeleteJobCardQC(JobCardQCId);

                TempData["Success"] = "Deleted Successfully! (" + ref_no + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = JobCardQCId });
            }
        }


    }
}