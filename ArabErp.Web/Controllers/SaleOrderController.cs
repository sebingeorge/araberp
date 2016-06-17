using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;

namespace ArabErp.Web.Controllers
{
    public class SaleOrderController : Controller
    {
        // GET: SaleOrder
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Create()
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();
            FillWrkDesc();
            FillVehicle();
            FillUnit();
            FillEmployee();
            SaleOrder saleOrder = new SaleOrder();
            saleOrder.Items = new List<SaleOrderItem>();
            saleOrder.Items.Add(new SaleOrderItem());
            return View(saleOrder);
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
        public void FillWrkDesc()
        {
            var repo = new SaleOrderItemRepository();
            var list = repo.FillWorkDesc();
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
            var repo = new SaleOrderRepository();
            var list = repo.FillEmployee();
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
        [HttpPost]
        public ActionResult Save(SaleOrder model)
        {

            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            var Result= new SaleOrderRepository().InsertSaleOrder(model);
            FillWrkDesc();
            FillUnit();
            FillCustomer();
            FillVehicle();
            FillCurrency();
            FillCommissionAgent();
            FillEmployee();
            if (Result.SaleOrderId > 0)
            {
                TempData["Success"] = "Added Successfully!";
                TempData["SaleOrderRefNo"] = Result.SaleOrderRefNo;
                return RedirectToAction("Create");
            }
            else 
            {
                TempData["error"] = "Oops!!..Something Went Wrong!!";
                TempData["SaleOrderRefNo"] = null ;
                SaleOrder saleOrder = new SaleOrder();
                saleOrder.SaleOrderDate = System.DateTime.Today;
                saleOrder.Items = new List<SaleOrderItem>();
                saleOrder.Items.Add(new SaleOrderItem());
                return View("Create", saleOrder);
            }
           
        }
        public JsonResult GetCustomerDetailsByKey(int cusKey)
        {
            var res = (new SaleOrderRepository()).GetCurrencyIdByCustKey(cusKey);
            string address = (new SaleOrderRepository()).GetCusomerAddressByKey(cusKey);
            return Json(new { Success = true, CurrencyName = res.Name,CurrencyId=res.Id, Address = address }, JsonRequestBehavior.AllowGet);
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
           
            FillUnit();
            FillEmployee();
                FillWrkDesc();
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
            var rep = new SaleOrderRepository();
            var slist = rep.GetSaleOrdersPendingWorkshopRequest();
            var pager = new Pager(slist.Count(), page);

            var viewModel = new PagedSaleOrderViewModel
            {
                SaleOrders = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };
            return View(viewModel);
        }
        public ActionResult Hold(int? SaleOrderId)
        {
            FillCustomer();
            FillCurrency();
            FillCommissionAgent();

            FillUnit();
            FillEmployee();
            FillWrkDesc();
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

            return View("Approval", model);
        }
        public ActionResult UpdateHoldStatus(int? Id, string hreason)
        {

            new SaleOrderRepository().UpdateSOHold(Id ?? 0, hreason);
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

            FillUnit();
            FillEmployee();
            FillWrkDesc();
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

            return View("Approval", model);
        }
        public ActionResult UpdateReleaseStatus(int? Id)
        {

            new SaleOrderRepository().UpdateSORelease(Id ?? 0);
            return RedirectToAction("PendingSaleOrderRelease");
        }
    }
}