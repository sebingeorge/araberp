using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;

namespace ArabErp.Web.Controllers
{
    public class SalesInvoiceController : Controller
    {
        SalesInvoiceRepository Repo = new SalesInvoiceRepository();
        // GET: SalesInvoice
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(List<SalesInvoiceItem> ObjSaleInvoiceItem)
        {
                SalesInvoice saleinvoice = new SalesInvoice();
                SalesInvoiceRepository  SalesInvoiceRepo=new SalesInvoiceRepository();
                SalesInvoiceItemRepository SalesInvoiceItemRepo =new SalesInvoiceItemRepository();
            if(ObjSaleInvoiceItem.Count>0)
            {
                
                saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(ObjSaleInvoiceItem[0].SaleOrderId);
               
                
                List<int> SelectedSaleOrderItemId = (from SalesInvoiceItem s in ObjSaleInvoiceItem
                                                     where s.SelectStatus
                                                     select s.SaleOrderItemId).ToList<int>();

                saleinvoice.SaleInvoiceItems = SalesInvoiceItemRepo.GetSelectedSalesInvoiceDT(SelectedSaleOrderItemId, ObjSaleInvoiceItem[0].SaleOrderId);
                //SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
                //SalesInvoice saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(SelectedSaleOrderItemId);

               

            }
            return View("Create", saleinvoice);
        }
        public ActionResult PendingSalesInvoice()
        {
            var List = Repo.GetSalesInvoiceCustomerList();
            return View("PendingSalesInvoice",List);
         
        }
        public ActionResult PendingSalesInvoiceDt(int SalesOrderId, string Customer, string SaleOrderRefNoWithDate)
        {
            ViewBag.CustomerName = Customer;
            ViewBag.SaleOrderRefNoWithDate = SaleOrderRefNoWithDate;
            var List = Repo.GetPendingSalesInvoiceList(SalesOrderId);
            return PartialView("_PendingSalesInvoiceList",List);

        }
        public ActionResult Save(SalesInvoice model)
        {
            //var List = Repo.GetPendingSalesInvoiceList(SalesOrderId);
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];

            SalesInvoiceRepository SalesInvoiceRepo=new SalesInvoiceRepository();

            SalesInvoice Result = SalesInvoiceRepo.InsertSalesInvoice(model);


            if (Result.SalesInvoiceId > 0)
            {
                TempData["success"] = "Saved successfully";
                TempData["error"] = null ;
              
                TempData["SalesInvoiceRefNo"] = null;
                return RedirectToAction("PendingSalesInvoice");
            }
            else
            {
                TempData["success"] = null;
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }
            

        }
        //public ActionResult GetSalesInvoiceCustomerList(int? page)
        //{
        //    //int itemsPerPage = 10;
        //    int pageNumber = page ?? 1;


        //}
    }
}