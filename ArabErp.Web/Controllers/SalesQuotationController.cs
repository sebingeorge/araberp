using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;
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
            return View("Index", (new SalesQuotationRepository()).GetSalesQuotaationList(1));
        }
        public ActionResult AfterSalesIndex()
        {
            return View("Index", (new SalesQuotationRepository()).GetSalesQuotaationList(2));
        }
        public ActionResult Create()
        {
            var internalid = DatabaseCommonRepository.GetNextDocNo(1, OrganizationId);

            DropDowns();
            //FillWrkDesc();
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
            salesquotation.Materials = new List<SalesQuotationMaterial>();
            salesquotation.Materials.Add(new SalesQuotationMaterial());
            ViewBag.SubmitAction = "Save";
            return View("CreateTransportation", salesquotation);
        }

        [HttpPost]
        public ActionResult Create(SalesQuotation model)
        {
            //if (!ModelState.IsValid)
            //{
            //    var allErrors = ModelState.Values.SelectMany(v => v.Errors);


            DropDowns();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillSalesQuotationStatus();
            FillRateSettings();

            //return View(model);
            //}
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

                return View("CreateTransportation", model);
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
            salesquotation.isProjectBased = true;
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
        public ActionResult CreateProject(SalesQuotation model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            SalesQuotation result = new SalesQuotationRepository().InsertProjectQuotation(model);
            if (result.SalesQuotationId > 0)
            {
                TempData["Success"] = "Saved Successfully. Reference No. is " + model.QuotationRefNo;
                return RedirectToAction("CreateProject");
            }
            else
            {
                TempData["error"] = "Some error occurred. Please try again.";
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
            salesquotation.isWarranty = new SalesQuotationRepository().isUnderWarranty(id, "Transport");
            salesquotation.DeliveryChallanDetails = new DeliveryChallanRepository().GetDeliveryChallan(id);
            salesquotation.CustomerId = new CustomerRepository().GetCustomerFromWarranty(id, salesquotation.isProjectBased);

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

        public ActionResult CreateAfterSalesProject(int id = 0)//ProjectCompletionId is received here
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            var internalid = DatabaseCommonRepository.GetNextDocNo(28, OrganizationId);
            DropDowns();
            FillWrkDescAfterSales();
            FillVehicle();
            FillUnit();
            ItemDropdown();
            FillQuerySheet();

            SalesQuotation salesquotation = new SalesQuotation();
            salesquotation.ProjectCompletionId = id;
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
            salesquotation.isWarranty = new SalesQuotationRepository().isUnderWarranty(id, "Project");
            salesquotation.ProjectCompleionDetails = new ProjectCompletionRepository().GetProjectCompletion(id);
            salesquotation.CustomerId = new CustomerRepository().GetCustomerFromWarranty(id, salesquotation.isProjectBased);

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
            FillSQEmployee(ProjectBased, AfterSales);
            ViewBag.isProjectBased = ProjectBased;
            ViewBag.isAfterSales = AfterSales;
            return View();
        }

        public ActionResult SalesQuotationsList(DateTime? from, DateTime? to, int ProjectBased, int AfterSales, int id = 0, int cusid = 0, int Employee=0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            ViewBag.ProjectBased = ProjectBased;
            return PartialView("_SalesQuotationsList", new SalesQuotationRepository().GetPreviousList(ProjectBased, AfterSales, id, cusid, OrganizationId, from, to, Employee));
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
                if (salesquotation.isProjectBased)
                    salesquotation.ProjectCompleionDetails = new ProjectCompletionRepository().GetProjectCompletion(salesquotation.ProjectCompletionId);
                else
                    salesquotation.DeliveryChallanDetails = new DeliveryChallanRepository().GetDeliveryChallan(salesquotation.DeliveryChallanId);
            }

            salesquotation.CustomerAddress = sorepo.GetCusomerAddressByKey(salesquotation.CustomerId);
            salesquotation.SalesQuotationItems = repo.GetSalesQuotationItems(id);
            try
            {
                //each workdescription will have the same vehicle model id
                salesquotation.VehicleModelId = salesquotation.SalesQuotationItems[0].VehicleModelId;
            }
            catch { }
            salesquotation.Materials = repo.GetSalesQuotationMaterials(id);
            if (salesquotation.Materials == null || salesquotation.Materials.Count == 0)
            {
                salesquotation.Materials.Add(new SalesQuotationMaterial());
            }
            if (!salesquotation.isProjectBased)
            {
                return View("EditTransportation", salesquotation);
            }
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
                return RedirectToAction("PreviousList", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales = Convert.ToInt32(model.isAfterSales) });
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
            //salesquotation.SalesQuotationItems[0].UnitName = "Nos";
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
                TempData["Success"] = "Approved Successfully (" + model.QuotationRefNo + ")";
                return RedirectToAction("ListSalesQuotations", new { ProjectBased = Convert.ToInt32(model.isProjectBased), AfterSales = Convert.ToInt32(model.isAfterSales) });
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

        #region Revise Quotation
        public ActionResult Revise(int Id)
        {
            DropDowns();
            var repo = new SalesQuotationRepository();

            var sorepo = new SaleOrderRepository();


            SalesQuotation salesquotation = repo.GetSalesQuotation(Id);
            if (salesquotation.isAfterSales)
            {
                FillWrkDescAfterSales();
                ItemDropdown();
            }
            else if (salesquotation.isProjectBased)
            {
                FillWrkDescForProject();
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
            try
            {
                //each workdescription will have the same vehicle model id
                salesquotation.VehicleModelId = salesquotation.SalesQuotationItems[0].VehicleModelId;
            }
            catch { }
            salesquotation.Materials = repo.GetSalesQuotationMaterials(Id);
            if (salesquotation.Materials == null || salesquotation.Materials.Count == 0)
            {
                salesquotation.Materials.Add(new SalesQuotationMaterial());
            }
            ViewBag.SubmitAction = "Revise";
            return View(salesquotation);
        }
        [HttpPost]
        public ActionResult Revise(SalesQuotation model)
        {
            bool isProjectBased = model.isProjectBased;
            if (!ModelState.IsValid)
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
                FillSalesQuotationStatus();
                return View(model);
            }
            else
            {
                model.CreatedBy = UserID.ToString();
                model.CreatedDate = DateTime.Today;
                model.OrganizationId = OrganizationId;
                SalesQuotation result = new SalesQuotationRepository().ReviseSalesQuotation(model);
                TempData["success"] = "Quotation revised succcessfully. Reference No. is " + model.QuotationRefNo;
                if (!isProjectBased)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("ProjectIndex");
                }

            }
        }
        #endregion

        public ActionResult ListSalesQuotations(int ProjectBased, int AfterSales)
        {
            //QuotationApprovalRepository appRepo = new QuotationApprovalRepository();
            //QuotationApprovalAmountSettings amt = appRepo.GetUserApprovalAmountSettings(UserID);

            //ViewBag.AmountFrom = amt.AmountFrom;
            //ViewBag.AmountTo = amt.AmountTo;

            var repo = new SalesQuotationRepository();

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationApproveList(ProjectBased, AfterSales, OrganizationId);

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
        public ActionResult DeleteSQ(string isProjectBased, string isAfterSales, int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new SalesQuotationRepository().DeleteSalesQuotation(id, isAfterSales);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("PreviousList", new { ProjectBased = Convert.ToInt32(bool.Parse(isProjectBased)), AfterSales = Convert.ToInt32(bool.Parse(isAfterSales)) });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }
        public void FillQuotationNo(int ProjectBased, int AfterSales)
        {
            ViewBag.QuotationNoList = new SelectList(new DropdownRepository().FillQuotationNo(OrganizationId, ProjectBased, AfterSales), "Id", "Name");
        }
        public void FillSQCustomer(int ProjectBased, int AfterSales)
        {
            ViewBag.customerlist = new SelectList(new DropdownRepository().FillSQCustomer(OrganizationId, ProjectBased, AfterSales), "Id", "Name");
        }
        public void FillSQEmployee(int ProjectBased, int AfterSales)
        {
            ViewBag.Employeelist = new SelectList(new DropdownRepository().FillSQEmployee(OrganizationId, ProjectBased, AfterSales), "Id", "Name");
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
            var list = repo.QuerySheetNoDropdown(OrganizationId);
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
            FillFreezerUnit();
            FillBox();
            FillVehicle();
            ItemDropdown();
        }

        private void FillFreezerUnit()
        {
            ViewBag.freezerUnitList = new SelectList(new DropdownRepository().FillFreezerUnit(), "Id", "Name");
        }

        private void FillBox()
        {
            ViewBag.boxList = new SelectList(new DropdownRepository().FillBox(), "BoxId", "BoxName");
        }

        public ActionResult CommissionedProjects()
        {
            return View(new ProjectCompletionRepository().GetCommissionedProjects(OrganizationId));
        }

        public ActionResult CommissionedProjectsGrid(string project = "", string saleorder = "", string customer = "")
        {
            return PartialView("_CommissionedProjectsGrid", new ProjectCompletionRepository().GetCommissionedProjects(
                OrganizationId: OrganizationId,
                project: project,
                customer: customer,
                saleorder: saleorder));
        }

        public ActionResult DeliveryChallans()
        {
            return View(new DeliveryChallanRepository().GetDeliveryChallans(OrganizationId));
        }

        public ActionResult DeliveryChallansGrid(string challan = "", string saleorder = "", string customer = "")
        {
            return PartialView("_DeliveryChallansGrid", new DeliveryChallanRepository().GetDeliveryChallans(
                OrganizationId: OrganizationId,
                challan: challan,
                customer: customer,
                saleorder: saleorder));
        }

        public JsonResult GetItemSellingRate(int id)
        {
            return Json(new ItemSellingPriceRepository().GetItemSellingPrice(id, OrganizationId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQuerySheetCostingAmount(int id)
        {
            return Json(new QuerySheetRepository().GetCostingAmount(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult BusinessReport()
        {
            ViewBag.Year = FYStartdate.Year;
            return View();
        }


        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesQuotation.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("QuotationRefNo");
            ds.Tables["Head"].Columns.Add("QuotationDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CustomerAddress");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("SalesExecutive");
            ds.Tables["Head"].Columns.Add("PredictedClosingDate");
            ds.Tables["Head"].Columns.Add("QuotationValidToDate");
            ds.Tables["Head"].Columns.Add("ExpectedDeliveryDate");
            ds.Tables["Head"].Columns.Add("Remarks");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("SalesQuotationStatusName");
            ds.Tables["Head"].Columns.Add("QuotationStage");
            ds.Tables["Head"].Columns.Add("Competitors");
            ds.Tables["Head"].Columns.Add("DiscountRemarks");
            ds.Tables["Head"].Columns.Add("Discount");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("CurrencyName");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Designation");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("UserName");
            // ds.Tables["Head"].Columns.Add("EmpDesignation");
            ds.Tables["Head"].Columns.Add("Sign");


            //-------DT
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("UOM");
            ds.Tables["Items"].Columns.Add("Discount");
            ds.Tables["Items"].Columns.Add("Amount");
            ds.Tables["Items"].Columns.Add("TotalAmount");
            ds.Tables["Items"].Columns.Add("WorkDescription");


            SalesQuotationRepository repo = new SalesQuotationRepository();
            var Head = repo.GetSalesQuotationHD(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["QuotationRefNo"] = Head.QuotationRefNo;
            dr["QuotationDate"] = Head.QuotationDate.ToString("dd-MMM-yyyy");
            dr["CustomerName"] = Head.CustomerName;
            dr["CustomerAddress"] = Head.CustomerAddress;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["SalesExecutive"] = Head.SalesExecutiveName;
            dr["PredictedClosingDate"] = Head.PredictedClosingDate;
            dr["QuotationValidToDate"] = Head.QuotationValidToDate.ToString("dd-MMM-yyyy");
            dr["ExpectedDeliveryDate"] = Head.ExpectedDeliveryDate;
            dr["Remarks"] = Head.Remarks;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["SalesQuotationStatusName"] = Head.SalesQuotationStatusName;
            dr["QuotationStage"] = Head.QuotationStage;
            dr["Competitors"] = Head.Competitors;
            dr["DiscountRemarks"] = Head.DiscountRemarks;
            dr["Discount"] = Head.Discount;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["CurrencyName"] = Head.CurrencyName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Designation"] = Head.DesignationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["UserName"] = Head.EmpNmae;
            //dr["EmpDesignation"] = Head.EmpDesignation;
            dr["Sign"] = Server.MapPath("~/App_images/") + Head.ApprovedUsersig;

            ds.Tables["Head"].Rows.Add(dr);

            SalesQuotationRepository repo1 = new SalesQuotationRepository();
            var Items = repo1.GetSalesQuotationItemsPrint(Id);
            foreach (var item in Items)
            {
                var pritem = new SalesQuotationItem
                {
                    WorkDescr = item.WorkDescr,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    UnitName = item.UnitName,
                    Discount = item.Discount,
                    Amount = item.Amount,
                    WorkDescription = item.WorkDescription,
                    TotalAmount = item.TotalAmount

                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["WorkDescr"] = pritem.WorkDescr;
                dri["Quantity"] = pritem.Quantity;
                dri["Rate"] = pritem.Rate;
                dri["Discount"] = pritem.Discount;
                dri["Amount"] = pritem.Amount;
                dri["UOM"] = pritem.UnitName;
                dri["WorkDescription"] = pritem.WorkDescription;
                dri["TotalAmount"] = pritem.TotalAmount;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesQuotation.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/ms-word", String.Format("SalesQuotation{0}.doc", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult PrintBussinessReport(string MonthName, int? Month, string YearName, int? Year)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BussinessReport.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("Month");
            ds.Tables["Head"].Columns.Add("Year");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("QuotRef");
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Customer");
            ds.Tables["Items"].Columns.Add("Lead");
            ds.Tables["Items"].Columns.Add("Descr");
            ds.Tables["Items"].Columns.Add("Status");
            ds.Tables["Items"].Columns.Add("GrandTot");
            ds.Tables["Items"].Columns.Add("RevisionNo");
            ds.Tables["Items"].Columns.Add("Reason");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["Month"] = MonthName;
            dr["Year"] = YearName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            SalesQuotationRepository repo1 = new SalesQuotationRepository();
            var Items = repo1.GetSalesQuotaationListPrint(Month ?? DateTime.Today.Month, Year ?? DateTime.Today.Year);
            foreach (var item in Items)
            {
                var pritem = new SalesQuotationList

                {
                    QuotationRefNo = item.QuotationRefNo,
                    QuotationDate = item.QuotationDate,
                    CustomerName = item.CustomerName,
                    EmployeeName = item.EmployeeName,
                    GrandTotal = item.GrandTotal,
                    RevisionNo = item.RevisionNo,
                    RevisionReason = item.RevisionReason,
                    Status = item.Status,
                    Description = item.Description


                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["QuotRef"] = item.QuotationRefNo;
                dri["Date"] = item.QuotationDate.ToString("dd/MMM/yyyy");
                dri["Customer"] = item.CustomerName;
                dri["Lead"] = item.EmployeeName;
                dri["Descr"] = item.Description;
                dri["Status"] = item.Status;
                dri["GrandTot"] = item.GrandTotal;
                dri["RevisionNo"] = item.RevisionNo;
                dri["Reason"] = item.RevisionReason;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "BussinessReport.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("BussinessReport.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult GetRoomDetailsFromQuerySheet(int querySheetId)
        {
            FillUnitDoorUnit();
            SalesQuotation model = new SalesQuotationRepository().GetRoomDetailsFromQuerySheet(querySheetId);
            return PartialView("_ProjectRooms", model);
        }

        //public void UnitDropDown()
        //{
        //    ViewBag.UnitList = new SelectList(new DropdownRepository().FillFreezerUnit(), "Id", "Name");
        //}
        void FillUnitDoorUnit()
        {
            CondenserDropDown();
            EvaporatorDropDown();
            DoorDropDown();

        }
        void CondenserDropDown()
        {
            ViewBag.CondenserList = new SelectList(new DropdownRepository().FillCondenserUnit(), "Id", "Name");
        }
        void EvaporatorDropDown()
        {
            ViewBag.EvaporatorList = new SelectList(new DropdownRepository().FillEvaporatorUnit(), "Id", "Name");
        }
        void DoorDropDown()
        {
            ViewBag.DoorList = new SelectList(new DropdownRepository().FillDoor(), "Id", "Name");
        }
    }
}