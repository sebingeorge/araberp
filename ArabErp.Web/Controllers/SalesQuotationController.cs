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

           
            DropDowns();
            FillWrkDesc();
            FillVehicle();
            FillRateSettings();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = false;
            salesquotation.QuotationDate = System.DateTime.Today;
            salesquotation.QuotationRefNo = internalid; 
            salesquotation.PredictedClosingDate = System.DateTime.Today;
            salesquotation.QuotationValidToDate = System.DateTime.Today;
            salesquotation.ExpectedDeliveryDate = System.DateTime.Today;
            salesquotation.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;

            salesquotation.SalesQuotationItems = new List<SalesQuotationItem>();
            salesquotation.SalesQuotationItems.Add(new SalesQuotationItem());
            salesquotation.SalesQuotationItems[0].Quantity = 1;
            salesquotation.SalesQuotationItems[0].UnitName = "Nos";
            ViewBag.SubmitAction = "Save";
            return View(salesquotation);
        }

        [HttpPost]
        public ActionResult Create(SalesQuotation model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);

            
                DropDowns();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillSalesQuotationStatus();
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
                DropDowns();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
              
                return View("Create", model);
            }
        }
        public ActionResult CreateProject()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(2, OrganizationId);

          
            DropDowns();
            FillWrkDescForProject();
            FillVehicle();
            FillUnit();
            ItemDropdown();
            FillQuerySheet();
        
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased=true;
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
            salesquotation.SalesQuotationItems[0].UnitName = "Nos";
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
              
                DropDowns();
                FillWrkDescForProject();
                FillVehicle();
                FillQuerySheet();
                FillUnit();
             
              
                return View("Create", model);
            }

        }
        public ActionResult CreateAfterSalesTrans(int id = 0)//DeliveryChallanId is received here
        {
            if (id == 0) return RedirectToAction("Index", "Home");

            var internalid = DatabaseCommonRepository.GetNextDocNo(28, OrganizationId);
            DropDowns();
            FillWrkDescAfterSales();
            FillVehicle();
            FillUnit();
            ItemDropdown();
            FillRateSettings();
            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.DeliveryChallanId = id;
            salesquotation.isProjectBased = false;
            salesquotation.isAfterSales = true;
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
            salesquotation.SalesQuotationItems[0].UnitName = "Nos";
            ViewBag.SubmitAction = "Save";
            return View("Create", salesquotation);
        }
        [HttpPost]
        public ActionResult CreateAfterSalesTrans(SalesQuotation model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);

              
                DropDowns();
                FillWrkDescAfterSales();
                FillVehicle();
                FillUnit();
              
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
                return RedirectToAction("CreateAfterSalesTrans");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;
               
                DropDowns();
                FillWrkDescAfterSales();
                FillVehicle();
                FillUnit();
            
                return View("CreateAfterSalesTrans", model);
            }
        }

        public ActionResult CreateAfterSalesProject()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(28, OrganizationId);


            DropDowns();
            FillWrkDescAfterSales();
            FillVehicle();
            FillUnit();
            ItemDropdown();
            FillQuerySheet();

            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.isProjectBased = true;
            salesquotation.isAfterSales = true;
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
            salesquotation.SalesQuotationItems[0].UnitName = "Nos";
            ViewBag.SubmitAction = "Save";
            return View("Create", salesquotation);
        }

        [HttpPost]
        public ActionResult CreateAfterSalesProject(SalesQuotation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            SalesQuotation result = new SalesQuotationRepository().InsertSalesQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["QuotationRefNo"] = result.QuotationRefNo;
                return RedirectToAction("CreateAfterSalesProject");
            }
            else
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null;

                DropDowns();
                FillWrkDescAfterSales();
                FillVehicle();
                FillQuerySheet();
                FillUnit();
                ItemDropdown();

                return View("Create", model);
            }

        }

        public ActionResult PreviousList(int ProjectBased, int AfterSales)
        {
            FillQuotationNo(ProjectBased, AfterSales);
            FillSQCustomer(ProjectBased, AfterSales);
            ViewBag.isProjectBased = ProjectBased;
            ViewBag.isAfterSales = AfterSales;
            return View();
        }

        public ActionResult SalesQuotationsList(DateTime? from, DateTime? to, int ProjectBased, int AfterSales, int id = 0, int cusid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            ViewBag.ProjectBased = ProjectBased;
            return PartialView("_SalesQuotationsList", new SalesQuotationRepository().GetPreviousList(ProjectBased, AfterSales, id, cusid, OrganizationId, from, to));
        }

        public ActionResult Edit(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");

            DropDowns();
            ItemDropdown();
            FillVehicle();
            FillQuerySheetInQuot();
            FillUnit();
            FillRateSettings();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(id);


            if (!salesquotation.isProjectBased && !salesquotation.isAfterSales)
            {
                FillWrkDesc();
            }
            else if (salesquotation.isProjectBased && !salesquotation.isAfterSales)
            {
                FillWrkDescForProject();
            }
          
            else if (salesquotation.isAfterSales)
            {
                FillWrkDescAfterSales();

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

           

            var repo = new SalesQuotationRepository();
            try
            {
                new SalesQuotationRepository().UpdateSalesQuotation(model);
                TempData["success"] = "Updated Successfully (" + model.QuotationRefNo + ")";
                return RedirectToAction("PreviousList", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales =  Convert.ToInt32(model.isAfterSales) });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("PreviousList", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales = Convert.ToInt32(model.isAfterSales) });
        }
  

        [HttpGet]
        public ActionResult Approve(int SalesQuotationId)
        {


            DropDowns();
            FillVehicle();
            FillQuerySheetInQuot();
            FillUnit();
            FillRateSettings();
            ItemDropdown();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(SalesQuotationId);
           
            if (!salesquotation.isProjectBased && !salesquotation.isAfterSales)
            {
                FillWrkDesc();
            }
            else if (salesquotation.isProjectBased && !salesquotation.isAfterSales)
            {
                FillWrkDescForProject();
            }

            else if (salesquotation.isAfterSales)
            {
                FillWrkDescAfterSales();

            }
            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(SalesQuotationId);
            salesquotation.Materials = repo.GetSalesQuotationMaterials(SalesQuotationId);
            salesquotation.SalesQuotationItems[0].UnitName = "Nos";
            ViewBag.SubmitAction = "Approve";
            return View("Create", salesquotation);
        }

        public ActionResult Approve(SalesQuotation model)
        {
            var repo = new SalesQuotationRepository();
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            //repo.ApproveSalesQuotation(model);
            //if (model.isProjectBased == 0)
            //{
            //    TempData["Success"] = "Approved Successfully!";
            //    TempData["QuotationRefNo"] = model.QuotationRefNo;
            //    return RedirectToAction("ListSalesQuotations", new { ProjectBased = 0 });
            //}


            try
            {
                new SalesQuotationRepository().ApproveSalesQuotation(model);
                TempData["Success"] = "Approved Successfully(" + model.QuotationRefNo + ")";
                return RedirectToAction("ListSalesQuotations", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales =  Convert.ToInt32(model.isAfterSales) });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("ListSalesQuotations", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales = Convert.ToInt32(model.isAfterSales) });
        }
            //else if (model.isProjectBased == 1)
            //{
            //    TempData["Success"] = "Approved Successfully!";
            //    TempData["QuotationRefNo"] = model.QuotationRefNo;
            //    return RedirectToAction("ListSalesQuotations", new { ProjectBased = 1 });
            //}
            //else if (model.isProjectBased == 2)
            //{
            //    TempData["Success"] = "Approved Successfully!";
            //    TempData["QuotationRefNo"] = model.QuotationRefNo;
            //    return RedirectToAction("ListSalesQuotations", new { ProjectBased = 2 });
            //}
            //else
            //{
            //    return View();
            //}

        //}
      
        //public ActionResult Revise(int Id)
        //{
        //    FillCustomer();
        //    FillCurrency();
        //    FillCommissionAgent();
        //    FillVehicle();
        //    FillQuerySheet();
        //    FillUnit();
        //    FillEmployee();
        //    FillSalesQuotationStatus();
        //    var repo = new SalesQuotationRepository();

        //    var sorepo = new SaleOrderRepository();


        //    SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
        //    if (salesquotation.isProjectBased == 2)
        //    {
        //        FillWrkDescAfterSales();
        //        ItemDropdown();
        //    }
        //    else if (salesquotation.isProjectBased == 1)
        //    {
        //        FillWrkDescForProject();
        //        ItemDropdown();
        //    }
        //    else
        //    {
        //        FillWrkDesc();
        //    }
        //    salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
        //    salesquotation.ParentId = salesquotation.SalesQuotationId;
        //    salesquotation.IsQuotationApproved = false;
        //    if (salesquotation.GrantParentId == null || salesquotation.GrantParentId == 0)
        //    {
        //        salesquotation.GrantParentId = salesquotation.ParentId;
        //    }

        //    salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(Id);
        //    salesquotation.Materials = repo.GetSalesQuotationMaterials(Id);
        //    ViewBag.SubmitAction = "Revise";
        //    return View(salesquotation);
        //}

        //[HttpPost]
        //public ActionResult Revise(SalesQuotation model)
        //{
        //    int isProjectBased = model.isProjectBased;
        //    if(!ModelState.IsValid)
        //    {
        //        //To Debug Errors
        //        var errors = ModelState
        //            .Where(x => x.Value.Errors.Count > 0)
        //            .Select(x => new { x.Key, x.Value.Errors })
        //            .ToArray();
        //        //End
        //        FillCustomer();
        //        FillCurrency();
        //        FillCommissionAgent();
        //        FillWrkDesc();
        //        FillQuerySheet();
        //        FillVehicle();
        //        FillUnit();
        //        FillEmployee();
        //        FillSalesQuotationStatus();
        //        return View(model);
        //    }
        //    else
        //    {
        //        SalesQuotation result = new SalesQuotationRepository().ReviseSalesQuotation(model);
        //        if (isProjectBased == 0)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            return RedirectToAction("ProjectIndex");
        //        }
                
        //    }
        //}
     

        public ActionResult ListSalesQuotations(int ProjectBased,int AfterSales)
        {
            QuotationApprovalRepository appRepo = new QuotationApprovalRepository();
            QuotationApprovalAmountSettings amt = appRepo.GetUserApprovalAmountSettings(UserID);

            ViewBag.AmountFrom = amt.AmountFrom;
            ViewBag.AmountTo = amt.AmountTo;

            var repo = new SalesQuotationRepository();

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationApproveList(ProjectBased, AfterSales);
      
            return View(salesquotations);
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
            FillSalesQuotationStatus();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);


            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(Id);
            ViewBag.SubmitAction = "StatusUpdate";
            return View("StatusUpdate", salesquotation);
        }

        //[HttpPost]
        //public ActionResult StatusUpdate(SalesQuotation model)
        //  {
        //      int SalesQUotationId = model.SalesQuotationId;
        //      var repo = new SalesQuotationRepository();

        //      var result =repo.StatusUpdate(model);


        //      if (result.SalesQuotationId > 0)
        //      {
        //          TempData["Success"] = "Status Successfully Updated!";
        //          TempData["QuotationRefNo"] = result.QuotationRefNo;
        //          if (model.isProjectBased == 0)
        //          {
        //              return RedirectToAction("Index");
        //          }
        //          else
        //          {
        //              return RedirectToAction("Index");
        //          }
        //      }
        //      else
        //      {
        //          int i = model.SalesQuotationId;
        //          TempData["error"] = "Oops!!..Something Went Wrong!!";
        //          TempData["SaleOrderRefNo"] = null;
        //          return RedirectToAction("StatusUpdate", new { Id = SalesQUotationId });
                  
        //      }
             

        //  }

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

 

    

        public ActionResult DeleteSQ(string isProjectBased,string isAfterSales,int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new SalesQuotationRepository().DeleteSalesQuotation(id, isAfterSales);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("PreviousList", new { ProjectBased = Convert.ToInt32(bool.Parse(isProjectBased)), AfterSales = Convert.ToInt32(bool.Parse(isAfterSales) )});
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
            return RedirectToAction("Edit", new { id = id });
            }
        }
        public void FillQuotationNo(int ProjectBased,int AfterSales)
        {
            ViewBag.QuotationNoList = new SelectList(new DropdownRepository().FillQuotationNo(OrganizationId, ProjectBased, AfterSales), "Id", "Name");
        }
        public void FillSQCustomer(int ProjectBased, int AfterSales)
        {
            ViewBag.customerlist = new SelectList(new DropdownRepository().FillSQCustomer(OrganizationId, ProjectBased, AfterSales), "Id", "Name");
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

        public void FillSalesQuotationStatus()
        {
            var repo = new SalesQuotationRepository();
            var list = repo.FillSalesQuotationStatus();
            ViewBag.SalesQuotationStatuslist = new SelectList(list, "Id", "Name");
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
        public void FillRateSettings()
        {
            ViewBag.rateSettings = new SelectList(new RateSettingsController().RateSettingsDropdown(), "Value", "Text");
        }


        public void DropDowns()
        {
            FillCustomer();
            FillCurrency();
            FillEmployee();
            FillCommissionAgent();
            FillSalesQuotationStatus();
        }
       
    }
}