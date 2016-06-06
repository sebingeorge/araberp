using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class JobCardQCController : Controller
    {
        JobCardQCRepository JobCardQCRepo = new JobCardQCRepository();
        JobCardQCParamRepository JobCardQCParamRepo = new JobCardQCParamRepository();
        // GET: JobCardQC
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            FillJobCard();
            FillEmployee();
            JobCardQC objJCQC = new JobCardQC();
            objJCQC.JobCardQCParams = JobCardQCParamRepo.GetJobCardQCParamList();


            return View(objJCQC);
        }
        public void FillJobCard()
        {

            var List = JobCardQCRepo.FillJobCard();
            ViewBag.FillJobCardList = new SelectList(List, "Id", "Name");
        }
        public void FillEmployee()
        {
            var List = JobCardQCRepo.FillEmployee();
            ViewBag.FillEmployeeList = new SelectList(List, "Id", "Name");
        }
        public ActionResult Save(JobCardQC jc)
        {
            jc.OrganizationId = 1;
            jc.CreatedDate = System.DateTime.Now;
            jc.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new JobCardQCRepository().InsertJobCardQC(jc);
            return RedirectToAction("Create");
        }
    }
}