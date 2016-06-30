using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CommissionAgentController : BaseController
    {
        // GET: CommissionAgent
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(CommissionAgent model)
        {
            var repo = new CommissionAgentRepository();
            var result = new CommissionAgentRepository().InsertCommissionAgent(model);
            if (result.CommissionAgentId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["RefNo"] = result.CommissionAgentRefNo;
                return RedirectToAction("Create");
            }
            else
            {

                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }
        }
        public ActionResult Edit(int Id)
        {

            CommissionAgent model = new CommissionAgentRepository().GetCommissionAgent(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(CommissionAgent model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var result = new CommissionAgentRepository().UpdateCommissionAgent(model);
            if (result.CommissionAgentId > 0)
            {
                TempData["Success"] = "Updated Successfully!";
                TempData["RefNo"] = result.CommissionAgentRefNo;
                return RedirectToAction("Create");
            }
            else
            {

                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;

                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {

            CommissionAgent model = new CommissionAgentRepository().GetCommissionAgent(Id);
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Delete(CommissionAgent model)
        {

            var result = new CommissionAgentRepository().DeleteCommissionAgent(model);


            if (result.CommissionAgentId > 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["RefNo"] = model.CommissionAgentRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["RefNo"] = null;
                return View("Create", model);
            }

        }
        public ActionResult FillCommissionAgentList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo=new CommissionAgentRepository();
            var List=repo.FillCommissionAgentList();
            return PartialView("CommissionAgentListView", List);
        }
    }
}