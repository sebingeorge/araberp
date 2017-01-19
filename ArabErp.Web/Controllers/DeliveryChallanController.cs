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
using System.Data.SqlClient;

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
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown1(OrganizationId), "Id", "Name");
        }
        public ActionResult PendingDeliveryChallan(int customerId)
        {
            if (customerId == 0)
            {
                return PartialView("_PendingDeliveryChallan", new List<PendingJC>());
            }
            return PartialView("_PendingDeliveryChallan", new DeliveryChallanRepository().PendingDeliveryChallan(customerId, OrganizationId));
        }
        public ActionResult Save(int id = 0)
        {
            if (id != 0)
            {

                try
                {
                    EmployeeDropdown();
                    List<PrintDescription> list = new List<PrintDescription>();
                    list.Add(new PrintDescription());
                    DeliveryChallan model = new DeliveryChallanRepository().GetDetailsFromJobCard(id, OrganizationId);
                    if (model == null) throw new NullReferenceException();
                    model.JobCardId = id;
                    model.DeliveryChallanRefNo = DatabaseCommonRepository.GetNextDocNo(18, OrganizationId);
                    model.DeliveryChallanDate = DateTime.Now;
                    model.TransportWarrantyExpiryDate = DateTime.Today.AddYears(1).AddDays(-1);
                    model.ItemBatches = new DeliveryChallanRepository().GetSerialNos(id).ToList();
                    model.PrintDescriptions = list;
                    return View(model);
                }
                catch (NullReferenceException)
                {
                    TempData["error"] = "Could not find the requested Delivery Challan. Please try again.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Some error occurred. Please try again.";
                }
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
                foreach (ItemBatch item in model.ItemBatches)
                {
                    item.WarrantyStartDate = model.DeliveryChallanDate;
                    item.WarrantyExpireDate = model.DeliveryChallanDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                }
            }
            catch (NullReferenceException) { }
            string ref_no = new DeliveryChallanRepository().InsertDeliveryChallan(model);
            if (ref_no.Length > 0)
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
                ChassisNo = data.ChassisNo,
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
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0, string RegNo = "", string Customer="")
        {
            return PartialView("_PreviousList", new DeliveryChallanRepository().GetAllDeliveryChallan(id, cusid, OrganizationId, from, to, RegNo, Customer));
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    EmployeeDropdown();
                    DeliveryChallan DeliveryChallan = new DeliveryChallan();
                    DeliveryChallan = new DeliveryChallanRepository().ViewDeliveryChallanHD(id);
                    DeliveryChallan.ItemBatches = new DeliveryChallanRepository().GetDeliveryChallanDT(id);
                    DeliveryChallan.PrintDescriptions = new DeliveryChallanRepository().GetPrintDescriptions(id);
                    if (DeliveryChallan.PrintDescriptions == null || DeliveryChallan.PrintDescriptions.Count == 0)
                        DeliveryChallan.PrintDescriptions.Add(new PrintDescription());
                    return View(DeliveryChallan);
                }
                else
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    TempData["success"] = "";
                }
            }
            catch (InvalidOperationException iox)
            {
                TempData["error"] = "Sorry, we could not find the requested item. Please try again.|" + iox.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please try again after sometime.|" + sx.Message;
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(DeliveryChallan model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                new DeliveryChallanRepository().UpdateDeliveryChallan(model);
                TempData["success"] = "Updated Successfully (" + model.DeliveryChallanRefNo + ")";
                TempData["JobCardQCRefNo"] = model.DeliveryChallanRefNo;
                return RedirectToAction("DeliveryChallanList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            EmployeeDropdown();
            return View(model);
        }
        public ActionResult Delete(int DeliveryChallanId = 0)
        {
            try
            {
                if (DeliveryChallanId == 0) return RedirectToAction("Index", "Home");
                string ref_no = new DeliveryChallanRepository().DeleteDeliveryChallan(DeliveryChallanId);

                TempData["Success"] = "Deleted Successfully!";
                TempData["DeliveryChallanRefNo"] = ref_no;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = DeliveryChallanId });
            }
        }

        public ActionResult DeliveryChallan(int DeliveryChallanId)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "DeliveryChallan.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            #region head
            ds.Tables["Head"].Columns.Add("DeliveryChallanRefNo");
            ds.Tables["Head"].Columns.Add("DeliveryChallanDate");
            ds.Tables["Head"].Columns.Add("Customer");
            ds.Tables["Head"].Columns.Add("CDoorNo");
            ds.Tables["Head"].Columns.Add("CStreet");
            ds.Tables["Head"].Columns.Add("CCountry");
            ds.Tables["Head"].Columns.Add("SaleRefNo");
            ds.Tables["Head"].Columns.Add("SONoDate");
            ds.Tables["Head"].Columns.Add("JobcardNo");
            ds.Tables["Head"].Columns.Add("JCDate");
            ds.Tables["Head"].Columns.Add("RegistrationNo");
            ds.Tables["Head"].Columns.Add("ChassisNo");
            ds.Tables["Head"].Columns.Add("WorkDesc");
            ds.Tables["Head"].Columns.Add("VehicleModel");
            ds.Tables["Head"].Columns.Add("Employee");
            ds.Tables["Head"].Columns.Add("PaymentTerms");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");
            ds.Tables["Head"].Columns.Add("QuotationRefNo");
            ds.Tables["Head"].Columns.Add("FreezerName");
            ds.Tables["Head"].Columns.Add("FreezerPartNo");
            ds.Tables["Head"].Columns.Add("BoxName");
            ds.Tables["Head"].Columns.Add("BoxPartNo");
            ds.Tables["Head"].Columns.Add("FreezerId");
            ds.Tables["Head"].Columns.Add("Box");
            ds.Tables["Head"].Columns.Add("SupplyOrderNo");
            ds.Tables["Head"].Columns.Add("SupplyOrderDate");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("Country");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("LPONo");
            ds.Tables["Head"].Columns.Add("LPODate");
            ds.Tables["Head"].Columns.Add("CreatedUser");
            ds.Tables["Head"].Columns.Add("CreateSignature");
            ds.Tables["Head"].Columns.Add("ApprovedUsersig");
            ds.Tables["Head"].Columns.Add("Printdescr");
            //ds.Tables["Head"].Columns.Add("TransportWarrantyExpiryDate"); 
            #endregion
            //-------DT
            ds.Tables["Items"].Columns.Add("Description");
            ds.Tables["Items"].Columns.Add("UoM");
            ds.Tables["Items"].Columns.Add("Quantity");
            //DeliveryChallanRepository model = new DeliveryChallanRepository();
            //var s = model.Get(Id);
            //if (s.isService == 0)
            //{
            DeliveryChallanRepository repo = new DeliveryChallanRepository();
            var Head = repo.GetDeliveryChallanHD(DeliveryChallanId, OrganizationId);


            DataRow dr = ds.Tables["Head"].NewRow();
            dr["DeliveryChallanRefNo"] = Head.DeliveryChallanRefNo;
            dr["DeliveryChallanDate"] = Head.DeliveryChallanDate.ToString("dd-MMM-yyyy");
            dr["Customer"] = Head.Customer;
            dr["CDoorNo"] = Head.CDoorNo;
            dr["CStreet"] = Head.CStreet;
            dr["CCountry"] = Head.CCountry;
            dr["SaleRefNo"] = Head.SaleRefNo;
            dr["SONoDate"] = Head.SONODATE;
            dr["JobcardNo"] = Head.JobCardNo;
            dr["JCDate"] = Head.JobCardDate;
            dr["RegistrationNo"] = Head.RegistrationNo;
            dr["ChassisNo"] = Head.ChassisNo;
            dr["WorkDesc"] = Head.WorkDescr;
            dr["VehicleModel"] = Head.VehicleModel;
            dr["Employee"] = Head.EmployeeName;
            dr["PaymentTerms"] = Head.PaymentTerms;
            dr["SpecialRemarks"] = Head.Remarks;
            dr["QuotationRefNo"] = Head.QuotationRefNo;
            dr["FreezerName"] = Head.FreezerName;
            dr["FreezerId"] = Head.ReeferId;
            dr["FreezerPartNo"] = Head.FreezerPartNo;
            dr["Box"] = Head.Box;
            dr["BoxName"] = Head.BoxPartNo;
            dr["BoxPartNo"] = Head.BoxPartNo;
            dr["SupplyOrderNo"] = Head.SupplyOrderNo;
            dr["SupplyOrderDate"] = Head.SupplyOrderDate.ToString("dd/MMM/yyyy");
            dr["SpecialRemarks"] = Head.Remarks;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["Country"] = Head.CountryName;
            dr["Phone"] = Head.Phone;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Zip"] = Head.Zip;
            dr["OrganizationName"] = Head.OrganizationName;
            //dr["TransportWarrantyExpiryDate"] = Head.tra;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["LPONo"] = Head.LPONo;
            dr["LPODate"] = Head.LPODate.ToString("dd/MMM/yyyy");
            dr["CreatedUser"] = Head.CreatedUser;
            dr["CreateSignature"] = Server.MapPath("~/App_Images/") + Head.CreatedUsersig;
            dr["ApprovedUsersig"] = Server.MapPath("~/App_Images/") + Head.ApprovedUsersig;
            dr["Printdescr"] = Head.printdes;
            ds.Tables["Head"].Rows.Add(dr);


            DeliveryChallanRepository repo1 = new DeliveryChallanRepository();
            var Items = repo1.GetDeliveryChallanDTPrint(DeliveryChallanId);
            foreach (var item in Items)
            {
                var DCItem = new PrintDescription
                {
                    Description = item.Description,
                    UoM = item.UoM,
                    Quantity = item.Quantity

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["Description"] = DCItem.Description;
                dri["UoM"] = DCItem.UoM;
                dri["Quantity"] = DCItem.Quantity;
                ds.Tables["Items"].Rows.Add(dri);
            }
            //}

            //var list = new DeliveryChallanRepository().GetSerialNos(Head.JobCardId).ToList();
            //ds.Tables.Add("ItemBatches");
            //ds.Tables["ItemBatches"].Columns.Add("SerialNo");
            //ds.Tables["ItemBatches"].Columns.Add("ItemName");
            //ds.Tables["ItemBatches"].Columns.Add("WarrantyPeriod");
            //dr = ds.Tables["ItemBatches"].NewRow();
            //foreach (var item in list)
            //{
            //    dr["SerialNo"] = item.SerialNo;
            //    dr["ItemName"] = item.ItemName;
            //    dr["WarrantyPeriod"] = item.WarrantyPeriodInMonths;
            //}

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "DeliveryChallan.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");//, String.Format("DeliveryChallan{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
       } 

    }
}