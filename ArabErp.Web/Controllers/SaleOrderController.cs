using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;

namespace ArabErp.Web.Controllers
{
    public class SaleOrderController : BaseController
    {
        // GET: SaleOrder
        public ActionResult Index(int type)
        {
            FillSORefNo(type);
            FillSOCustomer(type);
            ViewBag.isProjectBased = type;
            return View();
           
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int ProjectBased, int id = 0, int cusid = 0)
        {

            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            ViewBag.ProjectBased = ProjectBased;
            return PartialView("_PreviousList", new SaleOrderRepository().GetPreviousList(ProjectBased, id, cusid, OrganizationId, from, to));
        }

        public ActionResult Create(int? SalesQuotationId)
        {
           
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(SaleOrder).Name);
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
            //SaleOrder saleOrder = new SaleOrder();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrderFrmQuotation(SalesQuotationId ?? 0);

            var SOList = repo.GetSaleOrderItemFrmQuotation(SalesQuotationId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelName = item.VehicleModelName,VehicleModelId=item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            model.isProjectBased = 0;
            //saleOrder.Items = new List<SaleOrderItem>();
            //saleOrder.Items.Add(new SaleOrderItem());
            model.SaleOrderRefNo = "SAL/" + internalId;
            model.SaleOrderDate = DateTime.Now;
            model.EDateArrival = DateTime.Now;
            model.EDateDelivery = DateTime.Now;
            return View(model);
        }
        public ActionResult CreateProject(int? SalesQuotationId)
        {

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(SaleOrder).Name);
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

            var SOList = repo.GetSaleOrderItemFrmQuotation(SalesQuotationId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelName = item.VehicleModelName, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            //SaleOrder saleOrder = new SaleOrder();
            model.isProjectBased = 1;
            //model.Items = new List<SaleOrderItem>();
            //model.Items.Add(new SaleOrderItem());
            model.SaleOrderRefNo = "SAL/" + internalId;
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
            if(quoItems != null)
            {
                foreach(var items in quoItems)
                {
                    saleOrder.Items.Add(new SaleOrderItem()
                    {
                        WorkDescriptionId = items.WorkDescriptionId,
                        VehicleModelName=items.VehicleModelName,
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
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDesc();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillWrkDescProject()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDescForProject();
            ViewBag.workdesclist = new SelectList(list, "Id", "Name");
        }
        public void FillSOCustomer(int type)
        {
            ViewBag.customerlist = new SelectList(new DropdownRepository().FillSOCustomer(OrganizationId, type), "Id", "Name");

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
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
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

            return View(model);
        }
        [HttpPost]
        public ActionResult CreateProject(SaleOrder model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
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
            return View(model);
          
        }
        [HttpGet]
        public JsonResult GetCustomerDetailsByKey(int cusKey)
        {
            var res = (new SaleOrderRepository()).GetCurrencyIdByCustKey(cusKey);
            string address = (new SaleOrderRepository()).GetCusomerAddressByKey(cusKey);
            return Json(new { Success = true, CurrencyName = res.Name,CurrencyId=res.Id, Address = address }, JsonRequestBehavior.AllowGet);
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
            var Result=new {VehicleId=List.VehicleModelId ,VehicleName=List.VehicleModelName};
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PendingSaleOrderApproval(int ProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrderPending(ProjectBased);
            return View(pendingSO);  
        }
        public ActionResult PendingSaleOrderApprovalWR()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval();
            var result = from a in pendingSO where (a.IsPaymentApprovedForWorkshopRequest == false || a.IsPaymentApprovedForWorkshopRequest == null) select a;
            ViewBag.ApproveType = "WORKSHOP_REQUEST";
            return View("PendingSaleOrderApprovalTransaction", result);
        }
        public ActionResult PendingSaleOrderApprovalJC()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval();
            var result = from a in pendingSO where (a.IsPaymentApprovedForJobOrder == false || a.IsPaymentApprovedForJobOrder == null) select a;
            ViewBag.ApproveType = "JOB_CARD";
            return View("PendingSaleOrderApprovalTransaction", result);
        }
        public ActionResult PendingSaleOrderApprovalDEL()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSaleOrderForTransactionApproval> pendingSO = repo.GetSaleOrderPendingForTrnApproval();
            var result = from a in pendingSO where (a.IsPaymentApprovedForDelivery == false || a.IsPaymentApprovedForDelivery == null) select a;
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
                if(item.SaleOrderItemId == SaleOrderItemId)
                {
                    var soitem = new SaleOrderItem { 
                        SaleOrderItemId = item.SaleOrderItemId,
                        WorkDescriptionId = item.WorkDescriptionId, 
                        VehicleModelId = item.VehicleModelId, 
                        Quantity = item.Quantity, 
                        UnitId = item.UnitId, 
                        Rate = item.Rate, 
                        Amount = item.Amount, 
                        Discount = item.Discount };
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
            if(item.IsPaymentApprovedForDelivery == true)
            {
                item.PaymentApprovedForDeliveryCreatedBy = UserName;
                item.PaymentApprovedForDeliveryCreatedDate = DateTime.Now;
                repo.UpdateSaleOrderItemStatus(item, "DELIVERY_CHALLAN");
            }
            else if(item.IsPaymentApprovedForJobOrder == true)
            {
                item.PaymentApprovedForJobOrderCreatedBy = UserName;
                item.PaymentApprovedForJobOrderCreatedDate = DateTime.Now;
                repo.UpdateSaleOrderItemStatus(item, "JOB_CARD");
                view = "PendingSaleOrderApprovalJC";
            }
            else if(item.IsPaymentApprovedForWorkshopRequest == true)
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
            var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate,Amount=item.Amount,Discount=item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval",model);
        }
        [HttpPost]
        public ActionResult UpdateApprovalStatus(int? SaleOrderId)
        {
            SaleOrder so = (new SaleOrderRepository()).GetSaleOrder(SaleOrderId ?? 0);
            new SaleOrderRepository().UpdateSOApproval(SaleOrderId ?? 0);
            return RedirectToAction("PendingSaleOrderApproval", new { ProjectBased = so.isProjectBased});
        }
        public ActionResult PendingSaleOrderHold(int? page, int? isProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForHold(isProjectBased ?? 0);
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
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult UpdateHoldStatus(int? Id, string hreason, string  HoldDate)
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
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
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
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
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

            List<SalesQuotation> salesquotations = repo.GetSalesQuotationForSO(ProjectBased);
            ViewBag.ProjectBased = ProjectBased;
            return View(salesquotations);
        }

        public ActionResult IncentiveAmountReport()
        {
            FillCommissionAgent();
            return View("IncentiveAmount");
         }

        public ActionResult IncentiveAmountList(DateTime? FromDate, DateTime? ToDate, int CommissionAgentId=0)
        {
            return PartialView("IncentiveAmountGrid", new SaleOrderRepository().GetSaleOrderIncentiveAmountList(FromDate, ToDate, CommissionAgentId));
        }
      

    }
}