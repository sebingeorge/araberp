using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CommissionAgentController : Controller
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
        public ActionResult Save(CommissionAgent objCommissionAgent)
        {
            var repo = new CommissionAgentRepository();
            new CommissionAgentRepository().InsertCommissionAgent(objCommissionAgent);
            return View("Create");
        }
   
        public ActionResult FillCommissionAgentList()
        {
            var repo=new CommissionAgentRepository();
            var List=repo.FillCommissionAgentList();

            return PartialView("CommissionAgentListView", List);
        }
    }
}