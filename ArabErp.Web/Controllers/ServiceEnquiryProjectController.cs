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
                ServiceEnquiryRefNo = DatabaseCommonRepository.GetNextDocNo(33, OrganizationId),
                ServiceEnquiryDate = DateTime.Today,
                isProjectBased = 0,
                isService = 1,
                IsConfirmed = 0
            });
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