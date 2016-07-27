using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class PurchaseRequestController : BaseController
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET: PurchaseRequest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PendingPurchaseRequest()
        {
            var repo = new PurchaseRequestRepository();
            IEnumerable<PendingWorkShopRequest> pendingWR = repo.GetWorkShopRequestPending();
            return View(pendingWR);
        }
        public ActionResult Create(int? WorkShopRequestId)
        {
            PurchaseRequestRepository repo = new PurchaseRequestRepository();
           
            PurchaseRequest model = repo.GetPurchaseRequestDetails(WorkShopRequestId ?? 0);

            var PRList = repo.GetPurchaseRequestItem(WorkShopRequestId ?? 0);
            model.items = new List<PurchaseRequestItem>();
            foreach (var item in PRList)
            {
                var pritem = new PurchaseRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, MinLevel = item.MinLevel, WRRequestQty = item.WRRequestQty, CurrentStock = item.CurrentStock, WRIssueQty = item.WRIssueQty, TotalQty = item.TotalQty,InTransitQty=item.InTransitQty,PendingPRQty=item.PendingPRQty,ShortorExcess=item .ShortorExcess  };
                model.items.Add(pritem);

            }
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(PurchaseRequest).Name);

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

            model.PurchaseRequestNo = "PUR/" + internalId;
            model.PurchaseRequestDate = System.DateTime.Today;
            model.RequiredDate = System.DateTime.Today;


            return View(model);
        }
        [HttpPost]
        public ActionResult Save(PurchaseRequest model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            string id = new PurchaseRequestRepository().InsertPurchaseRequest(model);
                   if (id.Split('|')[0] != "0")
                   {
                       TempData["success"] = "Saved successfully. Purchase Request Reference No. is " + id.Split('|')[1];
                       TempData["error"] = "";
                       return RedirectToAction("PendingPurchaseRequest");
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
            return RedirectToAction("PendingPurchaseRequest");
        }

        //public ActionResult Edit(int Id)
        //{
        //    var repo = new PurchaseRequestRepository();
        //    SaleOrder model = repo.GetSaleOrder(SaleOrderId ?? 0);
        //    var SOList = repo.GetSaleOrderItem(SaleOrderId ?? 0);
        //    model.Items = new List<SaleOrderItem>();
        //    foreach (var item in SOList)
        //    {
        //        var soitem = new SaleOrderItem { WorkDescriptionId = item.WorkDescriptionId, VehicleModelId = item.VehicleModelId, Quantity = item.Quantity, UnitId = item.UnitId, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount };
        //        model.Items.Add(soitem);

        //    }
        //     return View("Approval", model);
        //}

        [HttpPost]
        public ActionResult Edit(Supplier model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            var repo = new SupplierRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Supplier", "SupplierName", model.SupplierName, "SupplierId", model.SupplierId);
            if (!isexists)
            {
                var result = new SupplierRepository().UpdateSupplier(model);
                if (result.SupplierId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["SupplierRefNo"] = result.SupplierRefNo;
                    return RedirectToAction("Index");
                }

                else
                {
  
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SupplierRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
                TempData["SupplierRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult PreviousList()
        {
            return View(new PurchaseRequestRepository().GetPurchaseRequest());
        }
    }
}