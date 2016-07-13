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
            return View();
        }

        [HttpGet]
         
           
        public ActionResult Create(int? SaleOrderId)
        {
            WorkShopRequestRepository repo = new WorkShopRequestRepository();

            WorkShopRequest model = repo.GetSaleOrderForWorkshopRequest(SaleOrderId ?? 0);
            model.WorkDescription = repo.GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(SaleOrderId ?? 0).WorkDescription;
           
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
        public ActionResult WorkShopRequestPending(int? page)
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

      

               public ActionResult WorkShopRequestData(SaleOrder model)
             {

                var repo = new WorkShopRequestRepository();
                var WSList = repo.GetWorkShopRequestData(model.SaleOrderId);
                model.Items = new List<SaleOrderItem>();
                foreach (var item in WSList)
                {
                    model.Items.Add(new SaleOrderItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, ActualQuantity=item.Quantity });

                }
                return PartialView("_DisplayWorkShopRequestData", model);
           }
               [HttpPost]
               public ActionResult Save(WorkShopRequest model)
               {
                   try {
                       model.OrganizationId = OrganizationId;
                   model.CreatedDate = System.DateTime.Now;
                   model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                   //new WorkShopRequestRepository().InsertWorkShopRequest(model);
                   string id = new WorkShopRequestRepository().InsertWorkShopRequest(model);
                   if (id.Split('|')[0] != "0")
                   {
                       TempData["success"] = "Saved successfully. WorkShop Request Reference No. is " + id.Split('|')[1];
                       TempData["error"] = "";
                       return RedirectToAction("WorkShopRequestPending");
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
                  return RedirectToAction("WorkShopRequestPending");
               }
    }

}