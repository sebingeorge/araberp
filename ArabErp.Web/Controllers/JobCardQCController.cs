using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(JobCardQC).Name);
            FillEmployee();
            JobCardQC objJCQC = new JobCardQC();
            objJCQC.JobCardQCRefNo = "JQC/" + internalId;
            objJCQC.JobCardNo = No;
            objJCQC.JobCardId = Id;
            objJCQC.CurrentDate =  System.DateTime.Today;
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
                return View("Create",model);
            }
            //try
            //{
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                if (new JobCardQCRepository().InsertJobCardQC(model) > 0)
                {
                    TempData["Success"] = "Added Successfully!";
                    TempData["JobCardQCRefNo"] = model.JobCardQCRefNo;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["JobCardQCRefNo"] = null;
                    return View(new JobCardQC { JobCardQCId = model.JobCardQCId });
                }

            }
        //}
        //    //new JobCardQCRepository().InsertJobCardQC(jc);

            //return RedirectToAction("PendingJobCardQC");
            //}

            // catch (Exception ex)
            // {
            //     string ErrorMessage = ex.Message.ToString();
            //     if (ex.InnerException != null)
            //     {
            //         if (ex.InnerException.Message != null)
            //         {
            //             ErrorMessage = ErrorMessage + ex.InnerException.Message.ToString();
            //         }
            //     }
            //     ViewData["Error"] = ErrorMessage;
            //     return View("ShowError");
            // }
        //}
        public ActionResult PendingJobCardQC()
        {
            try
            {

            JobCardQCRepository repo = new JobCardQCRepository();
            var result = repo.GetPendingJobCardQC();
            return View("PendingJobCardQC",result);
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
    }
}