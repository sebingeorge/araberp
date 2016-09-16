using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;

namespace ArabErp.Web.Controllers
{
    public class DeliveryChallanController : BaseController
    {
        // GET: DeliveryChallan
        public ActionResult Index()
        {
            CustomerDropdown();
            return View();
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public void CustomerDropdown()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown1(), "Id", "Name");
        }
        public ActionResult PendingDeliveryChallan(int customerId)
        {
            if ( customerId == 0 )
            {
                return PartialView("_PendingDeliveryChallan", new List<PendingJC>());
            }
            return PartialView("_PendingDeliveryChallan", new DeliveryChallanRepository().PendingDeliveryChallan(customerId, OrganizationId));
        }
        public ActionResult Save(int id = 0)
        {
            if ( id != 0 )
            {
                EmployeeDropdown();
                return View(new DeliveryChallan
                {
                    JobCardId = id,
                    DeliveryChallanRefNo = DatabaseCommonRepository.GetNextDocNo(18, OrganizationId),
                    DeliveryChallanDate = DateTime.Now,
                    ItemBatches = new DeliveryChallanRepository().GetSerialNos(id).ToList()
                });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(DeliveryChallan model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            try
            {
                foreach ( ItemBatch item in model.ItemBatches )
                {
                    item.WarrantyStartDate = model.DeliveryChallanDate;
                    item.WarrantyExpireDate = model.DeliveryChallanDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                }
            }
            catch ( NullReferenceException ) { }
            string ref_no = new DeliveryChallanRepository().InsertDeliveryChallan(model);
            if ( ref_no.Length > 0 )
            {
                TempData["success"] = "Saved Successfully. The Reference No. is " + ref_no;
                return RedirectToAction("Index");
            }
            else
            {
                EmployeeDropdown();
                TempData["error"] = "Some error occured while saving. Please try again";
                return View(model);
            }
        }
        public JsonResult GetCustomerAddress(int id = 0)
        {
            return Json(new CustomerRepository().GetCustomerAddress(id), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJobCardDetails(int id = 0)
        {
            var data = new DeliveryChallanRepository().GetJobCardDetails(id);
            return Json(new
            {
                JobCardNoDate = data.JobCardNoDate,
                SaleOrderNoDate = data.SaleOrderNoDate,
                CustomerOrderRef = data.CustomerOrderRef,
                VehicleModel = data.VehicleModel,
                WorkDescr = data.WorkDescr,
                CustomerName = data.CustomerName,
                RegistrationNo = data.RegistrationNo,
                PaymentTerms = data.PaymentTerms
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeliveryChallanList()
        {
            FillDCNo();
            return View();
        }
        public void FillDCNo()
        {
            ViewBag.DCNoList = new SelectList(new DropdownRepository().DCNODropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            return PartialView("_PreviousList", new DeliveryChallanRepository().GetAllDeliveryChallan(id, cusid, OrganizationId, from, to));
        }

        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "DeliveryChallan.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("DeliveryChallanRefNo");
            ds.Tables["Head"].Columns.Add("DeliveryChallanDate");
            ds.Tables["Head"].Columns.Add("Customer");
            ds.Tables["Head"].Columns.Add("SONoDate");
            ds.Tables["Head"].Columns.Add("JCNoDate");
            ds.Tables["Head"].Columns.Add("RegistrationNo");
            ds.Tables["Head"].Columns.Add("WorkDesc");
            ds.Tables["Head"].Columns.Add("VehicleModel");
            ds.Tables["Head"].Columns.Add("Employee");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");
            //-------DT
            ds.Tables["Items"].Columns.Add("SerialNo");
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("WarrantyStartDate");
            ds.Tables["Items"].Columns.Add("WarrantyExpireDate");

            DeliveryChallanRepository repo = new DeliveryChallanRepository();
            var Head = repo.GetDeliveryChallanHD(Id);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["DeliveryChallanRefNo"] = Head.DeliveryChallanRefNo;
            dr["DeliveryChallanDate"] = Head.DeliveryChallanDate.ToString("dd-MMM-yyyy");
            dr["Customer"] = Head.Customer;
            dr["SONoDate"] = Head.SONODATE;
            dr["JCNoDate"] = Head.JobCardNo;
            dr["RegistrationNo"] = Head.RegistrationNo;
            dr["WorkDesc"] = Head.WorkDescr;
            dr["VehicleModel"] = Head.VehicleModel;
            dr["Employee"] = Head.EmployeeName;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["SpecialRemarks"] = Head.Remarks;
            ds.Tables["Head"].Rows.Add(dr);


            DeliveryChallanRepository repo1 = new DeliveryChallanRepository();
            var Items = repo1.GetDeliveryChallanDT(Id); 
            foreach (var item in Items)
            {
                var DCItem = new ItemBatch
                {
                    SerialNo = item.SerialNo,
                    ItemName = item.ItemName,
                    WarrantyStartDate = item.WarrantyStartDate,
                    WarrantyExpireDate = item.WarrantyExpireDate
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["SerialNo"] = DCItem.SerialNo;
                dri["ItemName"] = DCItem.ItemName;
                dri["WarrantyStartDate"] = DCItem.WarrantyStartDate.ToString("dd-MMM-yyyy");
                dri["WarrantyExpireDate"] = DCItem.WarrantyExpireDate.ToString("dd-MMM-yyyy");
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "DeliveryChallan.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("DeliveryChallan{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}