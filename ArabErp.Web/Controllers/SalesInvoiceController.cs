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
    public class SalesInvoiceController : BaseController
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
                
                saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(ObjSaleInvoiceItem[0].SaleOrderId, ObjSaleInvoiceItem[0].invType);
                saleinvoice.InvoiceType = ObjSaleInvoiceItem[0].invType;

                List<int> SelectedSaleOrderItemId = (from SalesInvoiceItem s in ObjSaleInvoiceItem
                                                     where s.SelectStatus
                                                     select s.SaleOrderItemId).ToList<int>();

                saleinvoice.SaleInvoiceItems = SalesInvoiceItemRepo.GetSelectedSalesInvoiceDT(SelectedSaleOrderItemId, ObjSaleInvoiceItem[0].SaleOrderId);
                
                //SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
                //SalesInvoice saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(SelectedSaleOrderItemId);

               

            }
            if(saleinvoice.InvoiceType == "Inter" || saleinvoice.InvoiceType == "Final")
            {
                saleinvoice.isProjectBased = 1;
            }
            else
            {
                saleinvoice.isProjectBased = 0;
            }
            return View("Create", saleinvoice);
        }
        public ActionResult PendingSalesInvoice(string invType)
        {
            var List = Repo.GetSalesInvoiceCustomerList(invType);
            return View("PendingSalesInvoice",List);
         
        }
        public ActionResult PendingSalesInvoiceDt(int SalesOrderId, string Customer, string SaleOrderRefNoWithDate, string invType)
        {
            ViewBag.CustomerName = Customer;
            ViewBag.SaleOrderRefNoWithDate = SaleOrderRefNoWithDate;
            var List = Repo.GetPendingSalesInvoiceList(SalesOrderId, invType);
            foreach(var item in List)
            {
                item.invType = invType;
            }
            return PartialView("_PendingSalesInvoiceList",List);

        }
        [HttpPost]
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
                return RedirectToAction("PendingSalesInvoice", new { invType = model.InvoiceType});
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