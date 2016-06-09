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
            SalesInvoiceItemRepository SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
            var List=SalesInvoiceItemRepo.GetSalesInvoiceItems(ObjSaleInvoiceItem);
            return View("Create", List);
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
        public ActionResult Save(SalesInvoice ObjSaleInvoiceItem)
        {
            //var List = Repo.GetPendingSalesInvoiceList(SalesOrderId);
            SalesInvoiceItemRepository SalesInvoiceItemRepo=new SalesInvoiceItemRepository();
            ArrayList Result=  SalesInvoiceItemRepo.SalesInvoice(ObjSaleInvoiceItem);
            if (Result[0].ToString() == "0")
                return RedirectToAction("PendingSalesInvoice");
            else
                return RedirectToAction("SalesOrderId", ObjSaleInvoiceItem.SaleOrderId);
            
            

        }
        //public ActionResult GetSalesInvoiceCustomerList(int? page)
        //{
        //    //int itemsPerPage = 10;
        //    int pageNumber = page ?? 1;


        //}
    }
}