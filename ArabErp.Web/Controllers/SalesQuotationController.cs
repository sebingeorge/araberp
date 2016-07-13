using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SalesQuotationController : BaseController
    {
        // GET: SalesQuotation
        public ActionResult Index()
        {
            return View((new SalesQuotationRepository()).GetSalesQuotaationList(0));
        }
        public ActionResult ProjectIndex()
        {
            return View("Index",(new SalesQuotationRepository()).GetSalesQuotaationList(1));
        }
        public ActionResult Create()
        {
            var internalid = DatabaseCommonRepository.GetNextReferenceNo(typeof(SalesQuotation).Name);

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = 0;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = "SQ/" + internalid; 
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            ViewBag.SubmitAction = "Save";
            return View(salesquotation);
        }
        public ActionResult CreateProject()
        {
            var internalid = DatabaseCommonRepository.GetNextReferenceNo(typeof(SalesQuotation).Name);

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDescForProject();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = 1;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = "SQ/" + internalid;
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            ViewBag.SubmitAction = "Save";
            return View("Create",salesquotation);
        }
        [HttpPost]
        public ActionResult CreateProject(SalesQuotation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            SalesQuotation result = new SalesQuotationRepository().InsertSalesQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["QuotationRefNo"] = result.QuotationRefNo;
                return RedirectToAction("CreateProject");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDescForProject();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                //SalesQuotation salesquotation = new SalesQuotation();
                //salesquotation.QuotationDate = System.DateTime.Today;
                //salesquotation.PredictedClosingDate = System.DateTime.Today;
                //salesquotation.QuotationValidToDate = System.DateTime.Today;
                //salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
                //SaleOrder saleOrder = new SaleOrder();
                //saleOrder.SaleOrderDate = System.DateTime.Today;
                //saleOrder.Items = new List<SaleOrderItem>();
                //saleOrder.Items.Add(new SaleOrderItem());
                return View("Create", model);
            }

        }
        [HttpGet]
        public ActionResult Approve(int SalesQuotationId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(SalesQuotationId);
            salesquotation.CustomerAddress= sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);


            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(SalesQuotationId);
            ViewBag.SubmitAction = "Approve";
            return View("Create",salesquotation);
        }

        [HttpGet]
        public ActionResult Revise(int Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.ParentId = salesquotation.SalesQuotationId;
            salesquotation.IsQuotationApproved = false;
            if (salesquotation.GrantParentId == null || salesquotation.GrantParentId == 0)
            {
                salesquotation.GrantParentId = salesquotation.ParentId;
            }

            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(Id);
            ViewBag.SubmitAction = "Revise";
            return View(salesquotation);
        }
        [HttpPost]
        public ActionResult Revise(SalesQuotation model)
        {
            int isProjectBased = model.isProjectBased;
            if(!ModelState.IsValid)
            {
                //To Debug Errors
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();
                //End
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                return View(model);
            }
            else
            {
                SalesQuotation result = new SalesQuotationRepository().ReviseSalesQuotation(model);
                if (isProjectBased == 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("ProjectIndex");
                }
                
            }
        }
        public ActionResult Approve(SalesQuotation model)
        {

            var repo = new SalesQuotationRepository();

            repo.ApproveSalesQuotation(model);
            if(model.isProjectBased == 0)
            {
                return RedirectToAction("ListSalesQuotations");
            }
            else
            {
                return RedirectToAction("ListSalesQuotationsProject");
            }           
            
        }

        public ActionResult ListSalesQuotations(int ProjectBased)
        {
            QuotationApprovalRepository appRepo = new QuotationApprovalRepository();
            QuotationApprovalAmountSettings amt = appRepo.GetUserApprovalAmountSettings(UserID);

            ViewBag.AmountFrom = amt.AmountFrom;
            ViewBag.AmountTo = amt.AmountTo;

            var repo = new SalesQuotationRepository();

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationApproveList(ProjectBased);
      
            return View(salesquotations);
        }

        public void FillWrkDesc()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDesc();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDescForProject()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDescForProject();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillCustomer()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
        public void FillVehicle()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillVehicle();
            ViewBag.vehiclelist = new SelectList(list, "Id", "Name");
        }
        public void FillCommissionAgent()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCommissionAgent();
            ViewBag.commissionagentlist = new SelectList(list, "Id", "Name");
        }
        public void FillEmployee()
        {
            var repo = new DropdownRepository();
            var list = repo.EmployeeDropdown();
            ViewBag.employeelist = new SelectList(list, "Id", "Name");
        }
        public void FillUnit()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillUnit();
            ViewBag.unitlist = new SelectList(list, "Id", "Name");
        }
        public void FillCurrency()
        {
            var repo = new SaleOrderRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }

        public void FillSalesQuotationRejectReason()
        {
            var repo = new SalesQuotationRepository();
            var list = repo.FillSalesQuotationRejectReason();
            ViewBag.SalesQuotationRejectReasonlist = new SelectList(list, "Id", "Name");
        }

        [HttpPost]
        public ActionResult Create(SalesQuotation model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            SalesQuotation result= new SalesQuotationRepository().InsertSalesQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["QuotationRefNo"] = result.QuotationRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                //SalesQuotation salesquotation = new SalesQuotation();
                //salesquotation.QuotationDate = System.DateTime.Today;
                //salesquotation.PredictedClosingDate = System.DateTime.Today;
                //salesquotation.QuotationValidToDate = System.DateTime.Today;
                //salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
                //SaleOrder saleOrder = new SaleOrder();
                //saleOrder.SaleOrderDate = System.DateTime.Today;
                //saleOrder.Items = new List<SaleOrderItem>();
                //saleOrder.Items.Add(new SaleOrderItem());
                return View("Create", model);
            }
          
        }
       
        public ActionResult StatusUpdate(int Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);


            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(Id);
            ViewBag.SubmitAction = "StatusUpdate";
            return View("StatusUpdate", salesquotation);
        }
        [HttpPost]
        public ActionResult StatusUpdate(SalesQuotation model)
          {
              int SalesQUotationId = model.SalesQuotationId;
              var repo = new SalesQuotationRepository();

              var result =repo.StatusUpdate(model);


              if (result.SalesQuotationId > 0)
              {
                  TempData["Success"] = "Status Successfully Updated!";
                  TempData["QuotationRefNo"] = result.QuotationRefNo;
                  if (model.isProjectBased == 0)
                  {
                      return RedirectToAction("Index");
                  }
                  else
                  {
                      return RedirectToAction("Index");
                  }
              }
              else
              {
                  int i = model.SalesQuotationId;
                  TempData["error"] = "Oops!!..Something Went Wrong!!";
                  TempData["SaleOrderRefNo"] = null;
                  return RedirectToAction("StatusUpdate", new { Id = SalesQUotationId });
                  
              }
             

          }
    }
}