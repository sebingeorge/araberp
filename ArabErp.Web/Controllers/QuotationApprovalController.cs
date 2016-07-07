using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class QuotationApprovalController : BaseController
    {
        // GET: QuotationApproval
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            QuotationApprovalRepository repo = new QuotationApprovalRepository();
            QuotationApprovalViewModel model = new QuotationApprovalViewModel();

            var amountSettings = repo.GetApprovalAmountSettings();
            var approvalSettings = repo.GetApprovalSettings();

            model.QuotationApprovalAmountSettings = new List<QuotationApprovalAmountSettings>();
            model.QuotationApprovalSettings = new List<QuotationApprovalSettings>();

            foreach(var item in amountSettings)
            {
                model.QuotationApprovalAmountSettings.Add(item);
            }
            foreach(var item in approvalSettings)
            {
                model.QuotationApprovalSettings.Add(item);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Create(QuotationApprovalViewModel model)
        {
            if(ModelState.IsValid)
            {
                QuotationApprovalRepository repo = new QuotationApprovalRepository();
                repo.UpdateSettings(model);
            }
            return View(model);
        }
    }
}