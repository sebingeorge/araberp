using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;

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
        public ActionResult Create(int? WorkShopRequestId)
        {
            PurchaseRequestRepository repo = new PurchaseRequestRepository();
           
            PurchaseRequest model = repo.GetPurchaseRequestDetails(WorkShopRequestId ?? 0);

            var PRList = repo.GetPurchaseRequestItem(WorkShopRequestId ?? 0);
            model.items = new List<PurchaseRequestItem>();
            foreach (var item in PRList)
            {
                var pritem = new PurchaseRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, MinLevel = item.MinLevel, WRRequestQty = item.WRRequestQty, CurrentStock = item.CurrentStock, WRIssueQty = item.WRIssueQty, TotalQty = item.TotalQty };
                model.items.Add(pritem);

            }
            return View(model);
        }
        public ActionResult PendingPurchaseRequest()
        {
            var repo = new PurchaseRequestRepository();
            IEnumerable<PendingWorkShopRequest> pendingWR = repo.GetWorkShopRequestPending();
            return View(pendingWR);      
         }
        [HttpPost]
        public ActionResult Save(PurchaseRequest model)
        {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            new PurchaseRequestRepository().InsertPurchaseRequest(model);
            return RedirectToAction("PendingPurchaseRequest");
        }
    }
}