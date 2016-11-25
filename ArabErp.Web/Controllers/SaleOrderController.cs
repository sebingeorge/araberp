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
    public class SaleOrderController : BaseController
    {
        // GET: SaleOrder
        public ActionResult Index(int type, int service = 0)
        {
            FillSORefNo(type);
            FillSOCustomer(type);
            ViewBag.isProjectBased = type;
            ViewBag.isService = service;
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int ProjectBased, int id = 0, int cusid = 0, int service = 0)
        {

            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            ViewBag.ProjectBased = ProjectBased;
            return PartialView("_PreviousList", new SaleOrderRepository().GetPreviousList(ProjectBased, id, cusid, OrganizationId, from, to, service));
        }

        public ActionResult Create(int? SalesQuotationId)
        {

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(3, OrganizationId);
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDesc();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillQuotationNo(0);


            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occurred. Please try again.|" + ex.Message;
            }

            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrderFrmQuotation(SalesQuotationId ?? 0);
            model.Items = repo.GetSaleOrderItemFrmQuotation(SalesQuotationId ?? 0);

            for (int i = 0; i < model.Items.Count; i++)
            {
                while (model.Items[i].Quantity > 1)
                {
                    model.Items.Insert(i + 1, model.Items[i]);
                    model.Items[i].Quantity -= model.Items[i + 1].Quantity = 1;
                }
            }

            model.Materials = repo.GetSaleOrderMaterialFrmQuotation(SalesQuotationId ?? 0);

            model.SaleOrderRefNo = internalId;
            model.SaleOrderDate = DateTime.Now;
            model.EDateArrival = DateTime.Now;
            //model.EDateDelivery = DateTime.Now;
            return View(model);
        }
        public ActionResult CreateProject(int? SalesQuotationId)
        {

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(4, OrganizationId);
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillWrkDescProject();
                FillVehicle();
                FillUnit();
                FillEmployee();
                FillQuotationNo(1);


            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occurred. Please try again.|" + ex.Message;
            }
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrderFrmQuotation(SalesQuotationId ?? 0);

            model.Items = repo.GetSaleOrderItemFrmQuotation(SalesQuotationId ?? 0);

            for (int i = 0; i < model.Items.Count; i++)
            {
                while (model.Items[i].Quantity > 1)
                {
                    model.Items.Insert(i + 1, model.Items[i]);
                    model.Items[i].Quantity -= model.Items[i + 1].Quantity = 1;
                }
            }
            model.Materials = repo.GetSaleOrderMaterialFrmQuotation(SalesQuotationId ?? 0);


            model.SaleOrderRefNo = internalId;
            model.SaleOrderDate = DateTime.Now;
            model.EDateArrival = DateTime.Now;
            model.EDateDelivery = DateTime.Now;
            return View("Create", model);
        }
        public ActionResult DisplaySOList()
        {
            FillCurrency();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            SaleOrder saleOrder = new SaleOrder();

            saleOrder.Items = new List<SaleOrderItem>();
            var item = new SaleOrderItem();

            saleOrder.Items.Add(item);
            return PartialView("_DisplaySOList", saleOrder);
        }

        public ActionResult RefreshDisplaySOList(int? quoId, int isProjectBased, int currid)
        {
            FillCurrency();
            if (isProjectBased == 0)
            {
                FillWrkDesc();
            }
            else
            {
                FillWrkDescProject();
            }

            FillVehicle();
            FillUnit();
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.CurrencyId = currid;
            saleOrder.Items = new List<SaleOrderItem>();

            SalesQuotationRepository quoRepo = new SalesQuotationRepository();
            List<SalesQuotationItem> quoItems = quoRepo.GetSalesQuotationItems(quoId ?? 0);
            if (quoItems != null)
            {
                foreach (var items in quoItems)
                {
                    saleOrder.Items.Add(new SaleOrderItem()
                    {
                        WorkDescriptionId = items.WorkDescriptionId,
                        VehicleModelName = items.VehicleModelName,
                        UnitId = items.UnitId,
                        SlNo = 1,
                        Quantity = items.Quantity,
                        Rate = items.Rate,
                        Discount = items.Discount,
                        Amount = items.Amount
                    });
                }
            }
            else
            {
                var item = new SaleOrderItem();
                saleOrder.Items.Add(item);
            }
            saleOrder.isProjectBased = isProjectBased;
            return PartialView("_DisplaySOList", saleOrder);
        }
        public void FillSORefNo(int type)
        {
            ViewBag.sorefnolist = new SelectList(new DropdownRepository().FillSORefNo(OrganizationId, type), "Id", "Name");

        }
        public void FillCustomer()
        {

            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDesc()
        {
            var repo = new DropdownRepository();
            var list = repo.FillWorkDesc();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDescProject()
        {
            var repo = new DropdownRepository();
            var list = repo.FillWorkDescForProject();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }


        public void FillSOCustomer(int type)
        {
            ViewBag.customerlist = new SelectList(new DropdownRepository().FillSOCustomer(OrganizationId, type), "Id", "Name");

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
        public void FillQuotationNo(int isProjectBased)
        {
            var repo = new DropdownRepository();
            var list = repo.QuotationNoDropdown(isProjectBased);
            ViewBag.QuotationNolist = new SelectList(list, "Id", "Name");
        }
        public void FillQuotationNoInSo(int isProjectBased)
        {
            var repo = new DropdownRepository();
            var list = repo.QuotationInSaleOrderDropdown(isProjectBased);
            ViewBag.QuotationNolist = new SelectList(list, "Id", "Name");
        }

        [HttpPost]
        public ActionResult Create(SaleOrder model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.OrganizationId = OrganizationId;
                    model.CreatedDate = System.DateTime.Now;
                    model.CreatedBy = UserID.ToString();

                    string id = new SaleOrderRepository().InsertSaleOrder(model);
                    if (id.Split('|')[0] != "0")
                    {
                        TempData["success"] = "Saved successfully. Sale Order Reference No. is " + id.Split('|')[1];
                        TempData["error"] = "";
                        return RedirectToAction("PendingSalesQutoforSaleOrder", new { ProjectBased = 0 });
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                }
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
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
            FillWrkDesc();
            FillUnit();
            FillCustomer();
            FillVehicle();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();

            return View("Create", model);
        }
        [HttpPost]
        public ActionResult CreateProject(SaleOrder model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    model.OrganizationId = OrganizationId;
                    model.CreatedDate = System.DateTime.Now;
                    model.CreatedBy = UserID.ToString();
                    string id = new SaleOrderRepository().InsertSaleOrder(model);
                    if (id.Split('|')[0] != "0")
                    {
                        TempData["success"] = "Saved successfully. Sale Order Reference No. is " + id.Split('|')[1];
                        TempData["error"] = "";
                        //if (model.isProjectBased == 0)
                        //{
                        //    return RedirectToAction("PendingSalesQutoforSaleOrder", new { ProjectBased = 0 });
                        //}
                        //else
                        //{
                        return RedirectToAction("PendingSalesQutoforSaleOrder", new { ProjectBased = 1 });
                        //}

                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                else
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                }
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
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
            FillWrkDescProject();
            FillUnit();
            FillCustomer();
            FillVehicle();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();
            return View("Create", model);

        }
        [HttpGet]
        public JsonResult GetCustomerDetailsByKey(int cusKey)
        {
            var res = (new SaleOrderRepository()).GetCurrencyIdByCustKey(cusKey);
            string address = (new SaleOrderRepository()).GetCusomerAddressByKey(cusKey);
            string ContactPerson = (new SaleOrderRepository()).GetContactPerson(cusKey);
            string Telephone = new SaleOrderRepository().GetCustomerTelephone(cusKey);
            return Json(new
            {
                Success = true,
                CurrencyName = res.Name,
                CurrencyId = res.Id,
                Address = address,
                ContactPerson = ContactPerson,
                Telephone = Telephone
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetQuationDetailsByKey(int quoKey)
        {

            string address = (new SaleOrderRepository()).GetCustomerAddressQuotKey(quoKey);
            var details = (new SaleOrderRepository()).GetSODetailsByQuotKey(quoKey);
            return Json(new { Success = true, CustomerId = details.CustomerId, Address = address, CurrencyId = details.CurrencyId, PaymentTerms = details.PaymentTerms, SpecialRemarks = details.SpecialRemarks }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleModel(int WorkDescriptionId)
        {
            SaleOrderItem List = new SaleOrderRepository().GetVehicleModel(WorkDescriptionId);
            var Result = new { VehicleId = List.VehicleModelId, VehicleName = List.VehicleModelName };
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PendingSaleOrderApproval(int ProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrderPending(ProjectBased, OrganizationId);
            return View(pendingSO);
        }
        public ActionResult PendingSaleOrderApprovalWR()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval(OrganizationId);
            var result = from a in pendingSO where (a.IsPaymentApprovedForWorkshopRequest == false || a.IsPaymentApprovedForWorkshopRequest == null) select a;
            ViewBag.ApproveType = "WORKSHOP_REQUEST";
            return View("PendingSaleOrderApprovalTransaction", result);
        }
        public ActionResult PendingSaleOrderApprovalJC()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval(OrganizationId);
            var result = from a in pendingSO where (a.IsPaymentApprovedForJobOrder == false || a.IsPaymentApprovedForJobOrder == null) select a;
            ViewBag.ApproveType = "JOB_CARD";
            return View("PendingSaleOrderApprovalTransaction", result);
        }
        public ActionResult PendingSaleOrderApprovalDEL()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval(OrganizationId);
            var result = from a in pendingSO where ((a.IsPaymentApprovedForDelivery == false || a.IsPaymentApprovedForDelivery == null) && a.JodCardCompleteStatus == 1) select a;
            ViewBag.ApproveType = "DELIVERY_CHALLAN";
            return View("PendingSaleOrderApprovalTransaction", result);
        }
        public ActionResult ApprovalForTrn(int? SaleOrderId, int? SaleOrderItemId, string AppType)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillUnit();
            FillEmployee();

            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
            var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                if (item.SaleOrderItemId == SaleOrderItemId)
                {
                    var soitem = new SaleOrderItem
                    {
                        SaleOrderItemId = item.SaleOrderItemId,
                        WorkDescriptionId = item.WorkDescriptionId,
                        VehicleModelId = item.VehicleModelId,
                        Quantity = item.Quantity,
                        UnitId = item.UnitId,
                        Rate = item.Rate,
                        Amount = item.Amount,
                        Discount = item.Discount
                    };
                    model.Items.Add(soitem);

                    if (AppType == "WORKSHOP_REQUEST")
                    {
                        soitem.IsPaymentApprovedForWorkshopRequest = true;
                    }
                    else if (AppType == "JOB_CARD")
                    {
                        soitem.IsPaymentApprovedForJobOrder = true;
                    }
                    else if (AppType == "DELIVERY_CHALLAN")
                    {
                        soitem.IsPaymentApprovedForDelivery = true;
                    }
                }

            }
            ViewBag.AppType = AppType;
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult UpdateApprovalForTrn(SaleOrder model)
        {
            var repo = new SaleOrderRepository();
            var item = model.Items[0];

            var view = "PendingSaleOrderApprovalDEL";
            if (item.IsPaymentApprovedForDelivery == true)
            {
                item.PaymentApprovedForDeliveryCreatedBy = UserName;
                item.PaymentApprovedForDeliveryCreatedDate = DateTime.Now;
                repo.UpdateSaleOrderItemStatus(item, "DELIVERY_CHALLAN");
            }
            else if (item.IsPaymentApprovedForJobOrder == true)
            {
                item.PaymentApprovedForJobOrderCreatedBy = UserName;
                item.PaymentApprovedForJobOrderCreatedDate = DateTime.Now;
                repo.UpdateSaleOrderItemStatus(item, "JOB_CARD");
                view = "PendingSaleOrderApprovalJC";
            }
            else if (item.IsPaymentApprovedForWorkshopRequest == true)
            {
                item.PaymentApprovedForWorkshopRequestCreatedBy = UserName;
                item.PaymentApprovedForWorkshopRequestCreatedDate = DateTime.Now;
                repo.UpdateSaleOrderItemStatus(item, "WORKSHOP_REQUEST");
                view = "PendingSaleOrderApprovalWR";
            }
            return RedirectToAction(view);
        }
        public ActionResult Approval(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillUnit();
            FillEmployee();

            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
            model.Items = repo.GetSaleOrderItem(SaleOrderId ?? 0);

            FillWrkDesc();
            return View("Approval", model);
        }
        [HttpPost]
        public ActionResult UpdateApprovalStatus(int? SaleOrderId)
        {
            SaleOrder so = (new SaleOrderRepository()).GetSaleOrder(SaleOrderId ?? 0);
            new SaleOrderRepository().UpdateSOApproval(SaleOrderId ?? 0);
            return RedirectToAction("PendingSaleOrderApproval", new { ProjectBased = so.isProjectBased });
        }
        public ActionResult PendingSaleOrderHold(int? page, int? isProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForHold(isProjectBased ?? 0, OrganizationId);
            return View(pendingSO);
        }

        public ActionResult Hold(int? Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillQuotationNoInSo(0);
            FillUnit();
            FillEmployee();

            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(Id ?? 0);
            model.SaleOrderHoldDate = DateTime.Now;
            var SOList = repo.GetSaleOrderItem(Id ?? 0);
            model.Items = new List<SaleOrderItem>();

            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem
                {
                    VehicleModelName = item.VehicleModelName,
                    WorkDescriptionId = item.WorkDescriptionId,
                    WorkDescr = item.WorkDescr,
                    VehicleModelId = item.VehicleModelId,
                    Quantity = item.Quantity,
                    UnitId = item.UnitId,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Discount = item.Discount
                };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult UpdateHoldStatus(int? Id, string hreason, string HoldDate)
        {

            new SaleOrderRepository().UpdateSOHold(Id ?? 0, hreason, HoldDate);
            return RedirectToAction("PendingSaleOrderHold");
        }
        public ActionResult PendingSaleOrderRelease(int? isProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrderHolded(isProjectBased ?? 0);
            return View(pendingSO);
        }
        public ActionResult Release(int? Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillQuotationNoInSo(0);
            FillUnit();
            FillEmployee();
            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(Id ?? 0);
            model.SaleOrderReleaseDate = DateTime.Now;
            var SOList = repo.GetSaleOrderItem(Id ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem
                {
                    WorkDescr = item.WorkDescr,
                    VehicleModelName = item.VehicleModelName,
                    WorkDescriptionId = item.WorkDescriptionId,
                    VehicleModelId = item.VehicleModelId,
                    Quantity = item.Quantity,
                    UnitId = item.UnitId,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Discount = item.Discount
                };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult Cancel(int? Id)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillQuotationNoInSo(0);
            FillUnit();
            FillEmployee();
            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(Id ?? 0);
            model.SaleOrderReleaseDate = DateTime.Now;
            var SOList = repo.GetSaleOrderItem(Id ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult UpdateCancelStatus(int? Id)
        {

            new SaleOrderRepository().UpdateSOCancel(Id ?? 0);
            return RedirectToAction("PendingSaleOrderHold");
        }
        public ActionResult UpdateReleaseStatus(int? Id, string ReleaseDate)
        {

            new SaleOrderRepository().UpdateSORelease(Id ?? 0, ReleaseDate);
            return RedirectToAction("PendingSaleOrderHold");
        }
        public ActionResult Closing(int? isProjectBased)
        {
            SaleOrderRepository repo = new SaleOrderRepository();
            return View(repo.GetSaleOrdersForClosing(isProjectBased ?? 0));
        }
        public ActionResult Close(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillQuotationNoInSo(0);
            //FillPaymentTerms();
            FillUnit();
            FillEmployee();
            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
            var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, WorkDescr = item.WorkDescr, VehicleModelId = item.VehicleModelId, VehicleModelName = item.VehicleModelName, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View(model);
        }
        [HttpPost]
        public ActionResult Close(SaleOrder model)
        {
            SaleOrderRepository repo = new SaleOrderRepository();
            repo.CloseSaleOrder(model.SaleOrderId);
            return RedirectToAction("Closing");
        }
        public ActionResult PendingSalesQutoforSaleOrder(int ProjectBased)
        {
            var repo = new SalesQuotationRepository();

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationForSO(ProjectBased, OrganizationId);
            ViewBag.ProjectBased = ProjectBased;
            //ViewBag.AfterSales = isAfterSales;

            return View(salesquotations);
        }

        public ActionResult IncentiveAmountReport()
        {
            FillCommissionAgent();
            return View("IncentiveAmount");
        }

        public ActionResult IncentiveAmountList(DateTime? FromDate, DateTime? ToDate, int CommissionAgentId = 0)
        {
            return PartialView("IncentiveAmountGrid", new SaleOrderRepository().GetSaleOrderIncentiveAmountList(FromDate, ToDate, CommissionAgentId));
        }

        public ActionResult Edit(int type, int id = 0)
        {
            if (id != 0)
            {
                FillCustomer();
                FillCurrency();
                FillCommissionAgent();
                FillUnit();
                FillEmployee();

                FillVehicle();
                var repo = new SaleOrderRepository();
                SaleOrder model = repo.GetSaleOrder(id);
                model.Items = repo.GetSaleOrderItem(id);
                model.Materials = repo.GetSaleOrderMaterial(id);
                FillWrkDesc();
                return View("Edit", model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult Edit(SaleOrder model)
        {
            if (new SaleOrderRepository().isUsed(model.SaleOrderId) > 0)
            {
                TempData["error"] = "Cannot edit. Sale Order already used.";
                return RedirectToAction("Edit", new { id = model.SaleOrderId });
            }
            if (ModelState.IsValid)
            {

                if (new SaleOrderRepository().UpdateSaleOrder(model) > 0)
                {
                    TempData["success"] = "Updated Successfully";
                    return RedirectToAction("Index", new { type = model.isProjectBased });
                }


                else
                {
                    TempData["error"] = "Could not update due to some error. Please try again.";
                    return RedirectToAction("Edit", new { id = model.SaleOrderId });
                }

            }
            else
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);

            }

            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillUnit();
            FillEmployee();
            FillVehicle();

            TempData["success"] = "";
            return View(model);

        }

        public ActionResult Delete(int id, string isProjectBased, string isAfterSales)
        {

            try
            {
                string ref_no = new SaleOrderRepository().DeleteSaleOrder(id, Convert.ToInt32(isAfterSales));
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index", new { type = Convert.ToInt32(isProjectBased) });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }


        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SaleOrder.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD

            ds.Tables["Head"].Columns.Add("SaleOrderNoDate");
            ds.Tables["Head"].Columns.Add("QuotationNoDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CustomerAddress");
            ds.Tables["Head"].Columns.Add("CustomerOrderRef");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");

            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("DeliveryTerms");
            ds.Tables["Head"].Columns.Add("CommissionAgentName");
            ds.Tables["Head"].Columns.Add("CommissionAmount");
            ds.Tables["Head"].Columns.Add("SalesExecutiveId");
            ds.Tables["Head"].Columns.Add("EDateArrival");
            ds.Tables["Head"].Columns.Add("EDateDelivery");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("CurrencyName");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("WorkDescr");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("Rate");
            ds.Tables["Items"].Columns.Add("Discount");
            ds.Tables["Items"].Columns.Add("Amount");


            SaleOrderRepository repo = new SaleOrderRepository();
            var Head = repo.GetSaleOrderHD(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["SaleOrderNoDate"] = Head.SaleOrderNoDate;
            dr["QuotationNoDate"] = Head.QuotationNoDate;
            dr["CustomerName"] = Head.CustomerName;
            dr["CustomerAddress"] = Head.CustomerAddress;
            dr["CustomerOrderRef"] = Head.CustomerOrderRef;
            dr["SpecialRemarks"] = Head.SpecialRemarks;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["DeliveryTerms"] = Head.DeliveryTerms;
            dr["CommissionAgentName"] = Head.CommissionAgentName;
            dr["CommissionAmount"] = Head.CommissionAmount;
            dr["SalesExecutiveId"] = Head.SalesExecutiveName;
            dr["EDateArrival"] = Head.EDateArrival.ToString("dd-MMM-yyyy");
            dr["EDateDelivery"] = Head.EDateDelivery.ToString("dd-MMM-yyyy");
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["CurrencyName"] = Head.CurrencyName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            SaleOrderRepository repo1 = new SaleOrderRepository();
            var Items = repo1.GetSaleOrderItemDT(Id, OrganizationId);
            foreach (var item in Items)
            {
                var pritem = new SaleOrderItem
                {
                    WorkDescr = item.WorkDescr,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Discount = item.Discount,
                    Amount = item.Amount,

                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["WorkDescr"] = pritem.WorkDescr;
                dri["Quantity"] = pritem.Quantity;
                dri["Rate"] = pritem.Rate;
                dri["Discount"] = pritem.Discount;
                dri["Amount"] = pritem.Amount;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SaleOrder.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SaleOrder{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
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
                FillVehicle();
                FillCustomer();
                FillServiceWorkDescription();
                ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
                model.SaleOrderRefNo = DatabaseCommonRepository.GetNextDocNo(35, OrganizationId);
                model.SaleOrderDate = DateTime.Today;
                model.isProjectBased = 0;
                model.isService = 1;
                model.IsConfirmed = 1;
                model.Items = new List<SaleOrderItem>();
                model.Items.Add(new SaleOrderItem());
                return View("ServiceEnquiry", model);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Requested data could not be found. Please try again.";
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PendingEnquiries");
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
                return RedirectToAction("PendingEnquiries");
            }
            catch (Exception)
            {
                FillVehicle();
                FillCustomer();
                FillServiceWorkDescription();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View("ServiceEnquiry", model);
            }
        }

        private void FillServiceWorkDescription()
        {
            ViewBag.workDescList = new SelectList(
                new DropdownRepository().FillWorkDescForAfterSales(), "Id", "Name");
        }

        public ActionResult ServiceEnquiry()
        {
            FillCustomer();
            FillCurrency();
            FillServiceWorkDescription();
            return View(new ServiceEnquiry
            {
                ServiceEnquiryRefNo = DatabaseCommonRepository.GetNextDocNo(33, OrganizationId),
                ServiceEnquiryDate = DateTime.Today,
                isProjectBased = 0,
                isService = 1,
                IsConfirmed = 0
            });
        }

        [HttpPost]
        public ActionResult ServiceEnquiry(ServiceEnquiry model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString(); ;
                model.CreatedDate = System.DateTime.Now;
                model.IsConfirmed = 0;
                string ref_no = new SaleOrderRepository().InsertServiceEnquiry(model);
                if (ref_no.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                    return RedirectToAction("ServiceEnquiry");
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

        public ActionResult PendingEnquiries()
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiries(OrganizationId));
        }


        public ActionResult PrintJob(int id)//ServiceEnquiryId is received here
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "JobRepairOrder.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("ServiceEnquiryRefNo");
            ds.Tables["Head"].Columns.Add("ServiceEnquiryDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CDoorNo");
            ds.Tables["Head"].Columns.Add("CStreet");
            ds.Tables["Head"].Columns.Add("CPhone");
            ds.Tables["Head"].Columns.Add("CZip");
            ds.Tables["Head"].Columns.Add("CState");
            ds.Tables["Head"].Columns.Add("CContactPerson");
            ds.Tables["Head"].Columns.Add("VehicleMake");
            ds.Tables["Head"].Columns.Add("VehicleRegNo");
            ds.Tables["Head"].Columns.Add("VehicleChassisNo");
            ds.Tables["Head"].Columns.Add("VehicleKm");
            ds.Tables["Head"].Columns.Add("BoxMake");
            ds.Tables["Head"].Columns.Add("BoxNo");
            ds.Tables["Head"].Columns.Add("BoxSize");
            ds.Tables["Head"].Columns.Add("FreezerMake");
            ds.Tables["Head"].Columns.Add("FreezerModel");
            ds.Tables["Head"].Columns.Add("FreezerSerialNo");
            ds.Tables["Head"].Columns.Add("FreezerHours");
            ds.Tables["Head"].Columns.Add("TailLiftMake");
            ds.Tables["Head"].Columns.Add("TailLiftModel");
            ds.Tables["Head"].Columns.Add("TailLiftSerialNo");
            ds.Tables["Head"].Columns.Add("Complaints");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Description");
            ds.Tables["Items"].Columns.Add("TotalHours");


            SaleOrderRepository repo = new SaleOrderRepository();
            ServiceEnquiry se=new ServiceEnquiry();
            var Head = repo.GetJobPrintHD(id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["ServiceEnquiryRefNo"] = Head.ServiceEnquiryRefNo;
            dr["ServiceEnquiryDate"] = Head.ServiceEnquiryDate.ToString("dd-MMM-yyyy");
            dr["CustomerName"] = Head.CustomerName;
            dr["CDoorNo"] = Head.CDoorNo;
            dr["CStreet"] = Head.CStreet;
            dr["CPhone"] = Head.CPhone;
            dr["CZip"] = Head.CZip;
            dr["CState"] = Head.CState;
            dr["CContactPerson"] = Head.CContactPerson;
            dr["VehicleMake"] = Head.VehicleMake;
            dr["VehicleRegNo"] = Head.CPhone;
            dr["VehicleChassisNo"] = Head.ContactPerson;
            dr["VehicleKm"] = Head.VehicleKm;
            dr["BoxMake"] = Head.BoxMake;
            dr["BoxNo"] = Head.BoxNo;
            //dr["TaskDate"] = Head.TaskDate;
            dr["BoxSize"] = Head.BoxSize;
            dr["FreezerMake"] = Head.FreezerMake;
            dr["FreezerModel"] = Head.FreezerModel;
            dr["FreezerSerialNo"] = Head.FreezerSerialNo;
            dr["FreezerHours"] = Head.FreezerHours;
            dr["TailLiftMake"] = Head.TailLiftMake;
            dr["TailLiftModel"] = Head.TailLiftModel;
            dr["TailLiftSerialNo"] = Head.TailLiftSerialNo;
            dr["Complaints"] = Head.Complaints;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            //JobCardTaskRepository repo1 = new JobCardTaskRepository();
            //var Items = repo1.GetJobCardDT(Id);
            //foreach (var item in Items)
            //{
            //    var JCItem = new JobCardTask
            //    {
            //        TaskDate = item.TaskDate,
            //        Employee = item.Employee,
            //        JobCardTaskName = item.JobCardTaskName,
            //        //StartTime = item.StartTime,
            //        //EndTime = item.EndTime,
            //        Hours = item.Hours
            //    };

            //    DataRow dri = ds.Tables["Items"].NewRow();
            //    dri["TaskDate"] = JCItem.TaskDate.ToString("dd-MMM-yyyy");
            //    dri["Employee"] = JCItem.Employee;
            //    dri["Description"] = JCItem.JobCardTaskName;
            //    //dri["StartTime"] = JCItem.StartTime;
            //    //dri["EndTime"] = JCItem.EndTime;
            //    dri["Hours"] = JCItem.Hours;
            //    ds.Tables["Items"].Rows.Add(dri);
            //}

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "JobRepairOrder.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("JobRepairOrder{0}.pdf", id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult EnquiryList()
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiryList(OrganizationId));
        }

        public ActionResult EditEnquiry(int id)//ServiceEnquiryId is received here
        {
            FillCustomer();
            FillCurrency();
            FillServiceWorkDescription();
            ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
            model.IsConfirmed = 0;
            return View("ServiceEnquiry", model);
        }
    }
}