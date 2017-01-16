using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;
namespace ArabErp.Web.Controllers
{
    public class ServiceEnquiryProjectController : BaseController
    {
        // GET: ServiceEnquiryProject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ServiceEnquiryProject()
        {
            FillDropDowns();
            return View(new ServiceEnquiry
            {
                ServiceEnquiryRefNo = DatabaseCommonRepository.GetNextDocNo(41, OrganizationId),
                ServiceEnquiryDate = DateTime.Today,
                isProjectBased = 0,
                isService = 1,
                IsConfirmed = 0
            });
        }
        [HttpPost]
        public ActionResult ServiceEnquiryProject(ServiceEnquiry model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString(); ;
                model.CreatedDate = System.DateTime.Now;
                model.IsConfirmed = 0;
                model.isProjectBased = 1;
                string ref_no = new SaleOrderRepository().InsertServiceEnquiry(model);
                if (ref_no.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                    return RedirectToAction("ServiceEnquiryProject");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                FillCustomer();
                FillCurrency();
                FillServiceWorkDescription();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View(model);
            }
        }
        void FillDropDowns()
        {
            FillCustomer();
            FillCurrency();
            FillServiceWorkDescription();
        }
        public void FillCustomer()
        {

            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
        public void FillCurrency()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }
        private void FillServiceWorkDescription()
        {
            ViewBag.workDescList = new SelectList(
                new DropdownRepository().FillWorkDescForAfterSales(), "Id", "Name");
        }
    }
}