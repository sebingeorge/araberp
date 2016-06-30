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
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Create()
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
                //FillPaymentTerms();
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
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.isProjectBased = 0;
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            saleOrder.SaleOrderRefNo = "SAL/" + internalId;
            saleOrder.SaleOrderDate = DateTime.Now;
            saleOrder.EDateArrival = DateTime.Now;
            saleOrder.EDateDelivery = DateTime.Now;
            return View(saleOrder);
        }
        public ActionResult CreateProject()
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
                //FillPaymentTerms();
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
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.isProjectBased = 1;
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            saleOrder.SaleOrderRefNo = "SAL/" + internalId;
            saleOrder.SaleOrderDate = DateTime.Now;
            saleOrder.EDateArrival = DateTime.Now;
            saleOrder.EDateDelivery = DateTime.Now;
            return View("Create",saleOrder);
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
        public ActionResult RefreshDisplaySOList(int? quoId, int isProjectBased)
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
        public void FillQuotationNo(int isProjectBased)
        {
            var repo = new DropdownRepository();
            var list = repo.QuotationNoDropdown(isProjectBased);
            ViewBag.QuotationNolist = new SelectList(list, "Id", "Name");
        }
        [HttpPost]
        public ActionResult Create(SaleOrder model)
        {
            try
            {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            string id = new SaleOrderRepository().InsertSaleOrder(model);
             if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Sale Order Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("Create");
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
            //FillPaymentTerms();
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
                model.OrganizationId = 1;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                string id = new SaleOrderRepository().InsertSaleOrder(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Sale Order Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    if(model.isProjectBased == 0)
                    {
                        return RedirectToAction("Create");
                    }
                    else
                    {
                        return RedirectToAction("CreateProject");
                    }
                    
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
            //FillPaymentTerms();
            FillVehicle();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();

            return RedirectToAction("CreateProject");
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
            var cusname = (new SaleOrderRepository()).GetCustomerIdByQuotKey(quoKey);
            string address = (new SaleOrderRepository()).GetCustomerAddressQuotKey(quoKey);
            var details = (new SaleOrderRepository()).GetTermsByQuotKey(quoKey);
            return Json(new { Success = true, CustomerId = cusname.Id, Address = address, PaymentTerms = details.PaymentTerms, SpecialRemarks =details.SpecialRemarks}, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleModel(int WorkDescriptionId)
        {
            SaleOrderItem List = new SaleOrderRepository().GetVehicleModel(WorkDescriptionId);
            var Result=new {VehicleId=List.VehicleModelId ,VehicleName=List.VehicleModelName};
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PendingSaleOrderApproval()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrderPending();
            return View(pendingSO);  
        }
        public ActionResult Approval(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
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
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate,Amount=item.Amount,Discount=item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval",model);
        }
        [HttpPost]
        public ActionResult UpdateApprovalStatus(int? SaleOrderId)
        {

            new SaleOrderRepository().UpdateSOApproval(SaleOrderId ?? 0);
            return RedirectToAction("PendingSaleOrderApproval");
        }
        public ActionResult PendingSaleOrderHold(int? page)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForHold();
            return View(pendingSO);
        }
        public ActionResult Hold(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            //FillPaymentTerms();
            FillUnit();
            FillEmployee();
            
            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
            model.SaleOrderHoldDate = DateTime.Now;
            var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
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
        public ActionResult PendingSaleOrderRelease()
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrderHolded();
            return View(pendingSO);
        }
        public ActionResult Release(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            //FillPaymentTerms();
            FillUnit();
            FillEmployee();            
            FillVehicle();
            var repo = new SaleOrderRepository();
            SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
            model.SaleOrderReleaseDate = DateTime.Now;
            var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
            model.Items = new List<SaleOrderItem>();
            foreach (var item in SOList)
            {
                var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
                model.Items.Add(soitem);

            }
            FillWrkDesc();
            return View("Approval", model);
        }
        public ActionResult UpdateReleaseStatus(int? Id, string ReleaseDate)
        {

            new SaleOrderRepository().UpdateSORelease(Id ?? 0, ReleaseDate);
            return RedirectToAction("PendingSaleOrderRelease");
        }
        public ActionResult Closing()
        {
            SaleOrderRepository repo = new SaleOrderRepository();
            return View(repo.GetSaleOrdersForClosing());
        }
        public ActionResult Close(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
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
    }
}