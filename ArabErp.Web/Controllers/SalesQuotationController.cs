using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

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
        public ActionResult AfterSalesIndex()
        {
            return View("Index", (new SalesQuotationRepository()).GetSalesQuotaationList(2));
        }
        public ActionResult Create()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(1, OrganizationId);

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            FillRateSettings();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = 0;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = internalid; 
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
            salesquotation.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            salesquotation.SalesQuotationItems[0].Quantity = 1;
            //salesquotation.SalesQuotationItems[0].UnitId = 25;
            ViewBag.SubmitAction = "Save";
            return View(salesquotation);
        }

        [HttpPost]
        public ActionResult Create(SalesQuotation model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);

                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                FillRateSettings();

                return View(model);
            }
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            SalesQuotation result = new SalesQuotationRepository().InsertSalesQuotation(model);
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
                return View("Create", model);
            }
        }
        public ActionResult CreateProject()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(2, OrganizationId);

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDescForProject();
            FillVehicle();
            FillUnit();
            ItemDropdown();
            FillEmployee();
            FillQuerySheet();
            FillSalesQuotationRejectReason();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = 1;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = internalid;
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
            salesquotation.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            salesquotation.Materials = new List<SalesQuotationMaterial>();
            salesquotation.Materials.Add(new SalesQuotationMaterial());
            salesquotation.SalesQuotationItems[0].Quantity = 1;
            //salesquotation.SalesQuotationItems[0].UnitId = 25;
            ViewBag.SubmitAction = "Save";
            return View("Create",salesquotation);
        }
        [HttpPost]
        public ActionResult CreateProject(SalesQuotation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            
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
                FillQuerySheet();
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
        public ActionResult CreateAfterSales()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(28, OrganizationId);

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDescAfterSales();
            FillVehicle();
            FillUnit();
            FillEmployee();
            ItemDropdown();
            FillSalesQuotationRejectReason();
            FillRateSettings();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = 2;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = internalid;
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
            salesquotation.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());

            salesquotation.Materials = new List<SalesQuotationMaterial>();
            salesquotation.Materials.Add(new SalesQuotationMaterial());
          
            salesquotation.SalesQuotationItems[0].Quantity = 1;
            //salesquotation.SalesQuotationItems[0].UnitId = 25;
            ViewBag.SubmitAction = "Save";
            return View("Create", salesquotation);
        }
        [HttpPost]
        public ActionResult CreateAfterSales(SalesQuotation model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);

                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDescAfterSales();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                FillRateSettings();

                return View(model);
            }
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            SalesQuotation result = new SalesQuotationRepository().InsertSalesQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["QuotationRefNo"] = result.QuotationRefNo;
                return RedirectToAction("CreateAfterSales");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDescAfterSales();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillSalesQuotationRejectReason();
                return View("CreateAfterSales", model);
            }
        }
        [HttpGet]
        public ActionResult Approve(int SalesQuotationId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
          
            FillVehicle();
            FillQuerySheetInQuot();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            FillRateSettings();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(SalesQuotationId);
            if (salesquotation.isProjectBased == 2)
            {
                FillWrkDescAfterSales();
                ItemDropdown();
            }
            else if (salesquotation.isProjectBased == 1)
            {
                FillWrkDescAfterSales();
                ItemDropdown();
            }
            else
            {
                FillWrkDesc();
            }
            salesquotation.CustomerAddress= sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(SalesQuotationId);
            salesquotation.Materials = repo.GetSalesQuotationMaterials(SalesQuotationId);
            ViewBag.SubmitAction = "Approve";
            return View("Create",salesquotation);
        }

        public ActionResult Approve(SalesQuotation model)
        {

            var repo = new SalesQuotationRepository();
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            repo.ApproveSalesQuotation(model);
            if (model.isProjectBased == 0)
            {
                TempData["Success"] = "Approved Successfully!";
                TempData["QuotationRefNo"] = model.QuotationRefNo;
                return RedirectToAction("ListSalesQuotations", new { ProjectBased = 0 });
            }
            else if (model.isProjectBased == 1)
            {
                TempData["Success"] = "Approved Successfully!";
                TempData["QuotationRefNo"] = model.QuotationRefNo;
                return RedirectToAction("ListSalesQuotations", new { ProjectBased = 1 });
            }
            else if (model.isProjectBased == 2)
            {
                TempData["Success"] = "Approved Successfully!";
                TempData["QuotationRefNo"] = model.QuotationRefNo;
                return RedirectToAction("ListSalesQuotations", new { ProjectBased = 2 });
            }
            else
            {
                return View();
            }

        }
        [HttpGet]
        public ActionResult Revise(int Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillVehicle();
            FillQuerySheet();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
            if (salesquotation.isProjectBased == 2)
            {
                FillWrkDescAfterSales();
                ItemDropdown();
            }
            else
            {
                FillWrkDesc();
            }
            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.ParentId = salesquotation.SalesQuotationId;
            salesquotation.IsQuotationApproved = false;
            if (salesquotation.GrantParentId == null || salesquotation.GrantParentId == 0)
            {
                salesquotation.GrantParentId = salesquotation.ParentId;
            }

            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(Id);
            salesquotation.Materials = repo.GetSalesQuotationMaterials(Id);
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
                FillQuerySheet();
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
        public void FillQuotationNo(int type)
        {
            ViewBag.QuotationNoList = new SelectList(new DropdownRepository().FillQuotationNo(OrganizationId, type), "Id", "Name");
        }
        public void FillSQCustomer(int type)
        {
            ViewBag.customerlist = new SelectList(new DropdownRepository().FillSQCustomer(OrganizationId, type), "Id", "Name");
        }
        public void FillWrkDesc()
        {
            var repo = new DropdownRepository();
            var list = repo.FillWorkDesc();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDescForProject()
        {
            var repo = new DropdownRepository();
            var list = repo.FillWorkDescForProject();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDescAfterSales()
        {
            var repo = new DropdownRepository();
            var list = repo.FillWorkDescForAfterSales();
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
            var repo = new DropdownRepository();
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
            var repo = new DropdownRepository();
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
        public void FillQuerySheet()
        {
            var repo = new DropdownRepository();
            var list = repo.QuerySheetNoDropdown();
            ViewBag.QuerySheetNolist = new SelectList(list, "Id", "Name");
        }
        public void FillQuerySheetInQuot()
        {
            var repo = new DropdownRepository();
            var list = repo.QuerySheetNoInQuotationDropdown();
            ViewBag.QuerySheetNolist = new SelectList(list, "Id", "Name");
        }
       
       
        public ActionResult StatusUpdate(int Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillQuerySheet();
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

        public JsonResult GetRate(int workDescriptionId, string date, int type)
        {
            decimal data = new RateSettingsRepository().GetRate(workDescriptionId, date, type);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSpecialRate(int workDescriptionId, int customerId)
        {
            decimal data = new RateSettingsRepository().GetSpecialRate(workDescriptionId, customerId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void FillRateSettings()
        {
            ViewBag.rateSettings = new SelectList(new RateSettingsController().RateSettingsDropdown(), "Value", "Text");
        }

        public ActionResult Cancel(int Id)
        {
            SalesQuotationRepository repo = new SalesQuotationRepository();
            int CancelStatus = repo.GetUserApprovalCancelStatus(UserID);
            int result = new SalesQuotationRepository().Cancel(Id, CancelStatus);

            if (result == 1)
            {
                TempData["success"] = "Cancelled Successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                if (result == 0)
                {
                    TempData["error"] = "Sorry!! You don't have Permission to Delete this Quotation";
                    TempData["QuotationRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["QuotationRefNo"] = null;
                }
                return RedirectToAction("Index");
            }

        }

        public ActionResult PreviousList(int type)
        {
            FillQuotationNo(type);
            FillSQCustomer(type);
            ViewBag.isProjectBased = type;
            return View();
        }

        public ActionResult SalesQuotationsList(DateTime? from, DateTime? to, int ProjectBased, int id = 0, int cusid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            ViewBag.ProjectBased = ProjectBased;
            return PartialView("_SalesQuotationsList", new SalesQuotationRepository().GetPreviousList(ProjectBased, id, cusid, OrganizationId, from, to));
        }

        public ActionResult Edit(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            ItemDropdown();
            FillVehicle();
            FillQuerySheetInQuot();
            FillUnit();
            FillEmployee();
            FillSalesQuotationRejectReason();
            FillRateSettings();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(id);
            if (salesquotation.isProjectBased == 2)
            {
                FillWrkDescAfterSales();

            }
            else if (salesquotation.isProjectBased == 1)
            {
                FillWrkDescForProject();
            }
            else if (salesquotation.isProjectBased == 0)
            {
                FillWrkDesc();
            }

            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(id);
            salesquotation.Materials = repo.GetSalesQuotationMaterials(id);
            return View("Edit", salesquotation);
        }
        [HttpPost]
        public ActionResult Edit(SalesQuotation model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDescForProject();
            FillVehicle();
            FillUnit();
            FillEmployee();
            FillQuerySheet();
            FillSalesQuotationRejectReason();

            var repo = new SalesQuotationRepository();
            try
            {
                new SalesQuotationRepository().UpdateSalesQuotation(model);
                TempData["success"] = "Updated Successfully (" + model.QuotationRefNo + ")";
                return RedirectToAction("PreviousList", new { type = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return View("PreviousList", new { type = model.isProjectBased });
        }

        //public ActionResult DeleteSQ(int Id)
        //{
        //    ViewBag.Title = "Delete";

        //    var result1 = new SalesQuotationRepository().CHECK(Id);
        //    if (result1 > 0)
        //    {
        //        TempData["error"] = "Sorry!!..Already Approved!";
        //        TempData["ExpenseNo"] = null;
        //        return RedirectToAction("Edit", new { id = Id });
        //    }

        //    else
        //    {
        //        var result2 = new SalesQuotationRepository().DeleteSQDT(Id);
        //        var result3 = new SalesQuotationRepository().DeleteSQHD(Id);

        //        if (Id > 0)
        //        {

        //            TempData["Success"] = "Deleted Successfully!";
        //            return RedirectToAction("Index");
        //            return View("Create", model);
        //        }

        //        else
        //        {

        //            TempData["error"] = "Oops!!..Something Went Wrong!!";
        //            TempData["ExpenseNo"] = null;
        //            return RedirectToAction("Edit", new { id = Id });
        //        }

        //    }

        //}

       
        private void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public JsonResult GetItemUnit(int itemId)
        {
            return Json(new StockReturnItemRepository().GetItemUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemPartNo(int itemId)
        {
            return Json(new WorkShopRequestRepository().GetItemPartNo(itemId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteSQ( int id = 0,int isProjectBased=0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new SalesQuotationRepository().DeleteSalesQuotation(id,isProjectBased);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("PreviousList", new { type = isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
            return RedirectToAction("Edit", new { id = id });
            }
        }
      
    }
}