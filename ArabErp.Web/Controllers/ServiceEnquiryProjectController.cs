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
                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View(model);
            }
        }
        public ActionResult EnquiryList(int? isProjectBased)
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiryList(OrganizationId, isProjectBased??0));
        }
        public ActionResult PendingEnquiries(int? isProjectBased)
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiries(OrganizationId, isProjectBased??0));
        }
        public ActionResult ServiceOrder(int id = 0)//ServiceEnquiryId is received here
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid request. Please try again.";
                    RedirectToAction("Index", "Home");
                }

                FillDropDowns();
               
                ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
                model.SaleOrderRefNo = DatabaseCommonRepository.GetNextDocNo(42, OrganizationId);
                model.SaleOrderDate = DateTime.Today;
                model.isProjectBased = 1;
                model.isService = 1;
                model.IsConfirmed = 1;
                model.Items = new List<SaleOrderItem>();
                model.Items.Add(new SaleOrderItem());
                return View("ServiceEnquiryProject", model);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Requested data could not be found. Please try again.";
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PendingEnquiries", new {isProjectBased =1});
        }

        [HttpPost]
        public ActionResult ServiceOrder(ServiceEnquiry model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString();
                string ref_no = new SaleOrderRepository().InsertServiceOrder(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                return RedirectToAction("PendingEnquiries", new { isProjectBased = 1 });
            }
            catch (Exception)
            {
                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View("ServiceEnquiryProject", model);
            }
        }
        public ActionResult ServiceOrderList(int isProjectBased)
        {

            return View("_ServiceOrderList", new SaleOrderRepository().GetPendingServiceOrderList(OrganizationId, isProjectBased));
           
        }
        [HttpGet]
        public ActionResult EditEnquiry(int id)//ServiceEnquiryId is received here
        {
            try
            {
                if (id != 0)
                {

                    FillCustomer();
                   
                    ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
                    model.IsConfirmed = 0;
                    return View(model);
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
        public ActionResult EditEnquiry(ServiceEnquiry model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                new SaleOrderRepository().UpdateServiceEnquiry(model);
                TempData["success"] = "Updated Successfully (" + model.ServiceEnquiryRefNo + ")";
                TempData["ServiceEnquiryRefNo"] = model.ServiceEnquiryRefNo;
                return RedirectToAction("EnquiryList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            FillCustomer();
          
            return View(model);
        }
        public ActionResult ServiceOrderEdit(int id = 0)//ServiceEnquiryId is received here
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid request. Please try again.";
                    RedirectToAction("Index", "Home");
                }
                FillDropDowns();
                ServiceEnquiry model = new SaleOrderRepository().GetServiceOrderDetails(id, OrganizationId);
                model.Used = new SaleOrderRepository().Count(id);
                model.SaleOrderDate = DateTime.Today;
                model.isProjectBased = 0;
                model.isService = 1;
                model.IsConfirmed = 1;
                model.Items = new List<SaleOrderItem>();
                var repo = new SaleOrderRepository();
                model.Items = repo.GetSaleOrderItm(id);
                model.Items.Add(new SaleOrderItem());
                return View("ServiceEnquiryProject", model);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Requested data could not be found. Please try again.";
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PendingEnquiries", new { isProjectBased = 0 });
        }

        [HttpPost]
        public ActionResult ServiceOrderEdit(SaleOrder model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString();
                new SaleOrderRepository().UpdateServiceOrder(model);
                TempData["success"] = "Updated Successfully (" + model.SaleOrderRefNo + ")";
                TempData["ServiceEnquiryRefNo"] = model.SaleOrderRefNo;
                return RedirectToAction("ServiceOrderList");
            }
            catch (Exception)
            {

                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View("_ServiceOrderList", model);
            }
        }
        void FillDropDowns()
        {
            FillCustomer();
           
        }
        public void FillCustomer()
        {

            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
      
    }
}