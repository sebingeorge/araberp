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
            return View();
        }
        public ActionResult Create(int Id, string No, string JcDate, string Customer, string VehicleModel)
        {
          //  FillJobCard();
            FillEmployee();
            JobCardQC objJCQC = new JobCardQC();
            objJCQC.JobCardNo = No;
            objJCQC.JobCardId = Id;

            objJCQC.JcDate = JcDate;
            objJCQC.Customer = Customer;
            objJCQC.VehicleModel = VehicleModel;


            objJCQC.JobCardQCParams = JobCardQCParamRepo.GetJobCardQCParamList();


            return View(objJCQC);
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
        public ActionResult Save(JobCardQC jc)
        {
            jc.OrganizationId = 1;
            jc.CreatedDate = System.DateTime.Now;
            jc.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new JobCardQCRepository().InsertJobCardQC(jc);
            return RedirectToAction("PendingJobCardQC");
        }
        public ActionResult PendingJobCardQC()
        {
            JobCardQCRepository repo = new JobCardQCRepository();
            var result = repo.GetPendingJobCardQC();
            return View("PendingJobCardQC",result);
        }
    }
}