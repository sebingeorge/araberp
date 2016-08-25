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

            dashboard.DashboardMonthlySalesOrders = repo.GetSalesOrderDetails(OrganizationId);
            dashboard.DashboardTotalSalesQuotations = repo.GetSalesQuotationDetails(OrganizationId);
            dashboard.DashboardAcceptedSalesQuotations = repo.GetAccesptedSalesQuotationDetails(OrganizationId);
            dashboard.DashboardAcceptedProjectSalesQuotations = repo.GetAccesptedProjectSalesQuotationDetails(OrganizationId);
            dashboard.DashboardAcceptedTransportationSalesQuotations = repo.GetAccesptedTransportationSalesQuotationDetails(OrganizationId);
            dashboard.DashboardPurchase = repo.GetPurchaseDetails(OrganizationId);
            dashboard.DashboardSales = repo.GetSalesDetails(OrganizationId);
            dashboard.DashboardFGAllocated = repo.GetFGAllocated(OrganizationId);
            dashboard.DashboardSaleOrderAllocated = repo.GetFGAllocatedSaleOrder(OrganizationId);
            dashboard.DashboardJobCardCompletedDaily = repo.GetJobCardCompletedDaily(OrganizationId);

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
                var slist = rep.GetSaleOrdersPendingWorkshopRequest(OrganizationId);
                view.NoOfWorkShopRequests = slist.Count;
            }

            IEnumerable<ERPAlerts> Alerts;
            if (Session["alertpermissions"] == null)
            {
                UserRepository repo = new UserRepository();
                Alerts = repo.GetAlerts(UserID);
                Session["alertpermissions"] = Alerts;
            }
            else
            {
                Alerts = (IEnumerable<ERPAlerts>)Session["alertpermissions"];
                Session["alertpermissions"] = Alerts;
            }

            AlertPermission alertpermission = new AlertPermission();
            foreach (var item in Alerts)
            {
                if (item.HasPermission == 1)
                {
                    switch (item.AlertId)
                    {
                        case 1:
                            alertpermission.ProjectQuotApproval = true;
                            break;
                        case 2:
                            alertpermission.TransportQuotApproval = true;
                            break;
                        case 3:
                            alertpermission.PendingSupplyOrdForGrn = true;
                            break;
                        case 4:
                            alertpermission.DirectPurchaseReqDorApproval = true;
                            break;
                        case 5:
                            alertpermission.PendingSOForWorkshopReq = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            ViewBag.AlertPermissions = alertpermission;
            return PartialView("_QuickView", view);
        }
    }
}