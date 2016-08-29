using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;

namespace ArabErp.Web.Controllers
{
    public class SalesInvoiceController : BaseController
    {
        SalesInvoiceRepository Repo = new SalesInvoiceRepository();
        // GET: SalesInvoice
        public ActionResult Index(string type)
        {
            ViewBag.type = type;
            FillSalesInvoice(type);
            return View();
        }

        public ActionResult PreviousList(string type, DateTime? from, DateTime? to, int id = 0)
        {
            return PartialView("_PreviousListGrid", new SalesInvoiceRepository().PreviousList(type: type, from: from, to: to, id: id, OrganizationId: OrganizationId));
        }
        public ActionResult Create(List<SalesInvoiceItem> ObjSaleInvoiceItem)
        {
            SalesInvoice saleinvoice = new SalesInvoice();
            SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
            SalesInvoiceItemRepository SalesInvoiceItemRepo = new SalesInvoiceItemRepository();
            if (ObjSaleInvoiceItem.Count > 0)
            {

                saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(ObjSaleInvoiceItem[0].SaleOrderId, ObjSaleInvoiceItem[0].invType);
                saleinvoice.InvoiceType = ObjSaleInvoiceItem[0].invType;

                string internalId = "";
                internalId = DatabaseCommonRepository.GetNextDocNo(7, OrganizationId);
                saleinvoice.SalesInvoiceDate = System.DateTime.Today;
                saleinvoice.SalesInvoiceRefNo = internalId;
                List<int> SelectedSaleOrderItemId = (from SalesInvoiceItem s in ObjSaleInvoiceItem
                                                     where s.SelectStatus
                                                     select s.SaleOrderItemId).ToList<int>();

                saleinvoice.SaleInvoiceItems = SalesInvoiceItemRepo.GetSelectedSalesInvoiceDT(SelectedSaleOrderItemId, ObjSaleInvoiceItem[0].SaleOrderId);

                //SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();
                //SalesInvoice saleinvoice = SalesInvoiceRepo.GetSelectedSalesInvoiceHD(SelectedSaleOrderItemId);



            }
            if (saleinvoice.InvoiceType == "Inter" || saleinvoice.InvoiceType == "Final")
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
            return View("PendingSalesInvoice", List);

        }
        public ActionResult PendingSalesInvoiceDt(int SalesOrderId, string Customer, string SaleOrderRefNoWithDate, string invType)
        {
            ViewBag.CustomerName = Customer;
            ViewBag.SaleOrderRefNoWithDate = SaleOrderRefNoWithDate;
            var List = Repo.GetPendingSalesInvoiceList(SalesOrderId, invType);
            foreach (var item in List)
            {
                item.invType = invType;
            }
            return PartialView("_PendingSalesInvoiceList", List);

        }
        [HttpPost]
        public ActionResult Save(SalesInvoice model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            SalesInvoiceRepository SalesInvoiceRepo = new SalesInvoiceRepository();

            SalesInvoice Result = SalesInvoiceRepo.InsertSalesInvoice(model);


            if (Result.SalesInvoiceId > 0)
            {
                TempData["success"] = "Saved successfully";
                TempData["error"] = null;

                TempData["SalesInvoiceRefNo"] = null;
                return RedirectToAction("PendingSalesInvoice", new { invType = model.InvoiceType });
            }
            else
            {
                TempData["success"] = null;
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }


        }

        public JsonResult GetDueDate(DateTime date, int SaleOrderId)
        {
            DateTime duedate = (new SalesInvoiceRepository()).GetDueDate(date, SaleOrderId);
            return Json(duedate.ToString("dd/MMMM/yyyy"), JsonRequestBehavior.AllowGet);
        }

        public void FillSalesInvoice(string type)
        {
            ViewBag.salesInvoiceList = new SelectList(new DropdownRepository().SalesInvoiceDropdown(OrganizationId, type), "Id", "Name");
        }
        public ActionResult Print(int Id)
        {
            
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SalesInvoice.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            ds.Tables["Head"].Columns.Add("SalesInvoiceRefNo");
            ds.Tables["Head"].Columns.Add("SalesInvoiceDate");

            ds.Tables["Items"].Columns.Add("WorkDescription");

            SalesInvoiceRepository repo = new SalesInvoiceRepository();
            var Head = repo.GetSalesInvoice(Id);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["SalesInvoiceRefNo"] = Head.SalesInvoiceRefNo;
            dr["SalesInvoiceDate"] = Head.SalesInvoiceDate.ToString("dd-MMM-yyyy");
            ds.Tables["Head"].Rows.Add(dr);

            for (int i = 0; i < 10; i++)
            {
                DataRow dri = ds.Tables["Items"].NewRow();
                dri["WorkDescription"] = "Work description " + i.ToString();
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SalesInvoice.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", "SalesInvoice.pdf");
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}