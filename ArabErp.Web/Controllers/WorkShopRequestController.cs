using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class WorkShopRequestController : BaseController
    {
        // GET: WorkShopRequest
        public ActionResult Index()
        {
            FillWRNo();
            FillCustomerinWR();
            return View();
        }
        [HttpGet]
        public ActionResult Create(int? SaleOrderId)
        {
            WorkShopRequestRepository repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetSaleOrderForWorkshopRequest(SaleOrderId ?? 0);
            model.WorkDescription = repo.GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(SaleOrderId ?? 0).WorkDescription;
            var WSList = repo.GetWorkShopRequestData(SaleOrderId ?? 0);
            model.Items = new List<WorkShopRequestItem>();
            foreach (var item in WSList)
            {
                model.Items.Add(new WorkShopRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, ActualQuantity = item.Quantity });

            }
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(WorkShopRequest).Name);

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
            model.WorkShopRequestRefNo = "WOR/" + internalId;
            model.WorkShopRequestDate = System.DateTime.Today;
            model.RequiredDate = System.DateTime.Today;
            return View(model);
        }
        public ActionResult Pending()
        {
            return View();
        }
        public ActionResult WorkShopRequestPending(string saleOrder="")
        {

            var rep = new SaleOrderRepository();


            var slist = rep.GetSaleOrdersPendingWorkshopRequest(OrganizationId, saleOrder.Trim());

            //var pager = new Pager(slist.Count(), page);

            //var viewModel = new PagedSaleOrderViewModel
            //{
            //    SaleOrders = slist.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
            //    Pager = pager
            //};

            return PartialView("_PendingGrid", slist);
        }
        [HttpPost]
        public ActionResult Save(WorkShopRequest model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                string id = new WorkShopRequestRepository().InsertWorkShopRequest(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. WorkShop Request Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("Pending");
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
            return View("Pending", model);
        }
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousList", new WorkShopRequestRepository().GetPrevious(from, to, id, cusid, OrganizationId));
        }
        public ActionResult Edit(int? id)
        {

            var repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetWorkshopRequestHdData(id ?? 0);
            model.Items = repo.GetWorkShopRequestDtData(id ?? 0);
            //model.Items = new List<WorkShopRequestItem>();
            //foreach (var item in WSList)
            //{
            //    model.Items.Add(new WorkShopRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId });

            //}
            return View("Edit", model);
        }
        public void FillWRNo()
        {
            ViewBag.WRNoList = new SelectList(new DropdownRepository().WRNODropdown(OrganizationId), "Id", "Name");
        }
        public void FillCustomerinWR()
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().WRCustomerDropdown(OrganizationId), "Id", "Name");
        }
    }
}