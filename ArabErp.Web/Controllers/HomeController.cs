using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            Dashboard dashboard = new Dashboard();
            DashboardRepository repo = new DashboardRepository();
            dashboard.DashboardMonthlySalesQuotations = repo.GetSalesQuotationDetails(OrganizationId);
            return View(dashboard);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult LoadQuickView()
        {
            QuickView view = new QuickView();
            view.PendingDirectPurchaseRequests = true;
            view.PendingTransQuotations = true;
            view.PendingProjectQuotations = true;
            view.PendingSupplyOrders = true;
            view.PendingWorkshopRequests = true;

            if (view.PendingProjectQuotations)
            {
                SalesQuotationRepository repo = new SalesQuotationRepository();
                var res = repo.GetSalesQuotationApproveList(1);
                view.NoOfProjectQuotations = res.Count;
            }
            if (view.PendingTransQuotations)
            {
                SalesQuotationRepository repo = new SalesQuotationRepository();
                var res = repo.GetSalesQuotationApproveList(0);
                view.NoOfTransQuotations = res.Count;
            }
            if (view.PendingSupplyOrders)
            {
                var repo = new GRNRepository();
                var res = repo.GetGRNPendingList(0);
                view.NoOfSupplyOrders = res.Count();
            }
            if(view.PendingDirectPurchaseRequests)
            {
                var res = new DirectPurchaseRepository().GetUnApprovedRequests();
                view.NoOfPurchaseRequests = res.Count;
            }
            if(view.PendingWorkshopRequests)
            {
                var rep = new SaleOrderRepository();
                var slist = rep.GetSaleOrdersPendingWorkshopRequest();
                view.NoOfWorkShopRequests = slist.Count;
            }
            return PartialView("_QuickView", view);
        }
    }
}