using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using ArabErp.Web.Models;
using System.Data;
namespace ArabErp.Web.Controllers
{
    public class ServiceEnquiryProjectController : BaseController
    {
        // GET: ServiceEnquiryProject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ServiceEnquiryProject()
        {
            FillDropDowns();
            return View(new ServiceEnquiry
            {
                ServiceEnquiryRefNo = DatabaseCommonRepository.GetNextDocNo(41, OrganizationId),
                ServiceEnquiryDate = DateTime.Today,
                isProjectBased = 0,
                isService = 1,
                IsConfirmed = 0
            });
        }
        [HttpPost]
        public ActionResult ServiceEnquiryProject(ServiceEnquiry model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString(); ;
                model.CreatedDate = System.DateTime.Now;
                model.IsConfirmed = 0;
                model.isProjectBased = 1;
                string ref_no = new SaleOrderRepository().InsertServiceEnquiry(model);
                if (ref_no.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                    return RedirectToAction("ServiceEnquiryProject");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View(model);
            }
        }
        public ActionResult EnquiryList(int? isProjectBased)
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiryList(OrganizationId, isProjectBased??0));
        }
        public ActionResult PendingEnquiries(int? isProjectBased)
        {
            return View(new SaleOrderRepository().GetPendingServiceEnquiries(OrganizationId, isProjectBased??0));
        }
        public ActionResult ServiceOrder(int id = 0)//ServiceEnquiryId is received here
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid request. Please try again.";
                    RedirectToAction("Index", "Home");
                }

                FillDropDowns();
               
                ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
                model.SaleOrderRefNo = DatabaseCommonRepository.GetNextDocNo(42, OrganizationId);
                model.SaleOrderDate = DateTime.Today;
                model.isProjectBased = 1;
                model.isService = 1;
                model.IsConfirmed = 1;
                model.Items = new List<SaleOrderItem>();
                model.Items.Add(new SaleOrderItem());
                return View("ServiceEnquiryProject", model);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Requested data could not be found. Please try again.";
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PendingEnquiries", new {isProjectBased =1});
        }

        [HttpPost]
        public ActionResult ServiceOrder(ServiceEnquiry model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString();
                string ref_no = new SaleOrderRepository().InsertServiceOrder(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                return RedirectToAction("PendingEnquiries", new { isProjectBased = 1 });
            }
            catch (Exception)
            {
                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View("ServiceEnquiryProject", model);
            }
        }
        public ActionResult ServiceOrderList(int isProjectBased)
        {

            return View("_ServiceOrderList", new SaleOrderRepository().GetPendingServiceOrderList(OrganizationId, isProjectBased));
           
        }
        [HttpGet]
        public ActionResult EditEnquiry(int id)//ServiceEnquiryId is received here
        {
            try
            {
                if (id != 0)
                {

                    FillCustomer();
                   
                    ServiceEnquiry model = new SaleOrderRepository().GetServiceEnquiryDetails(id, OrganizationId);
                    model.IsConfirmed = 0;
                    return View(model);
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
        public ActionResult EditEnquiry(ServiceEnquiry model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                new SaleOrderRepository().UpdateServiceEnquiry(model);
                TempData["success"] = "Updated Successfully (" + model.ServiceEnquiryRefNo + ")";
                TempData["ServiceEnquiryRefNo"] = model.ServiceEnquiryRefNo;
                return RedirectToAction("EnquiryList", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            FillCustomer();
          
            return View(model);
        }
        public ActionResult ServiceOrderEdit(int id = 0)//ServiceEnquiryId is received here
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid request. Please try again.";
                    RedirectToAction("Index", "Home");
                }
                FillDropDowns();
                ServiceEnquiry model = new SaleOrderRepository().GetServiceOrderDetails(id, OrganizationId);
                model.Used = new SaleOrderRepository().Count(id);
                model.SaleOrderDate = DateTime.Today;
                model.isService = 1;
                model.IsConfirmed = 1;
                model.Items = new List<SaleOrderItem>();
                var repo = new SaleOrderRepository();
                model.Items = repo.GetSaleOrderItm(id);
                model.Items.Add(new SaleOrderItem());
                return View("ServiceEnquiryProject", model);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Requested data could not be found. Please try again.";
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return RedirectToAction("PendingEnquiries", new { isProjectBased = 0 });
        }

        [HttpPost]
        public ActionResult ServiceOrderEdit(SaleOrder model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedBy = UserID.ToString();
                new SaleOrderRepository().UpdateServiceOrder(model);
                TempData["success"] = "Updated Successfully (" + model.SaleOrderRefNo + ")";
                TempData["ServiceEnquiryRefNo"] = model.SaleOrderRefNo;
                return RedirectToAction("ServiceOrderList", new { isProjectBased=model.isProjectBased });
            }
            catch (Exception)
            {

                FillDropDowns();
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View("_ServiceOrderList", model);
            }
        }

        public ActionResult DeleteServiceorder(int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new SaleOrderRepository().DeleteProjectServiceOrder(id);

                TempData["Success"] = "Deleted Successfully!";
                TempData["ServiceEnquiryRefNo"] = ref_no;
                return RedirectToAction("ServiceOrderList", new { isProjectBased = 1 });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }

        public ActionResult DeleteEnquiry(int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new SaleOrderRepository().DeleteServiceEnquiry(id);

                TempData["Success"] = "Deleted Successfully!";
                TempData["ServiceEnquiryRefNo"] = ref_no;
                return RedirectToAction("EnquiryList", new { isProjectBased = 0 });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }
            
        void FillDropDowns()
        {
            FillCustomer();
           
        }
        public void FillCustomer()
        {

            var repo = new SaleOrderRepository();
            var list = repo.FillCustomer();
            ViewBag.customerlist = new SelectList(list, "Id", "Name");
        }
        public ActionResult PrintJob(int id)//ServiceEnquiryId is received here
        {
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "JobRepairOrderProject.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("ServiceEnquiryRefNo");
            ds.Tables["Head"].Columns.Add("ServiceEnquiryDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CDoorNo");
            ds.Tables["Head"].Columns.Add("CStreet");
            ds.Tables["Head"].Columns.Add("CPhone");
            ds.Tables["Head"].Columns.Add("CZip");
            ds.Tables["Head"].Columns.Add("CState");
            ds.Tables["Head"].Columns.Add("CContactPerson");
            //ds.Tables["Head"].Columns.Add("VehicleMake");
            //ds.Tables["Head"].Columns.Add("VehicleRegNo");
            //ds.Tables["Head"].Columns.Add("VehicleChassisNo");
            //ds.Tables["Head"].Columns.Add("VehicleKm");
            //ds.Tables["Head"].Columns.Add("BoxMake");
            //ds.Tables["Head"].Columns.Add("BoxNo");
            //ds.Tables["Head"].Columns.Add("BoxSize");
            //ds.Tables["Head"].Columns.Add("FreezerMake");
            //ds.Tables["Head"].Columns.Add("FreezerModel");
            //ds.Tables["Head"].Columns.Add("FreezerSerialNo");
            //ds.Tables["Head"].Columns.Add("FreezerHours");
            //ds.Tables["Head"].Columns.Add("TailLiftMake");
            //ds.Tables["Head"].Columns.Add("TailLiftModel");
            //ds.Tables["Head"].Columns.Add("TailLiftSerialNo");
            ds.Tables["Head"].Columns.Add("UnitDetails");
            ds.Tables["Head"].Columns.Add("Complaints");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("CreatedUser");
            ds.Tables["Head"].Columns.Add("CreatedUsersig");
            ds.Tables["Head"].Columns.Add("CreatedDes");

            //-------DT
            ds.Tables["Items"].Columns.Add("Date");
            ds.Tables["Items"].Columns.Add("Description");
            ds.Tables["Items"].Columns.Add("TotalHours");


            SaleOrderRepository repo = new SaleOrderRepository();
            ServiceEnquiry se = new ServiceEnquiry();
            var Head = repo.GetJobPrintHD(id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["ServiceEnquiryRefNo"] = Head.ServiceEnquiryRefNo;
            dr["ServiceEnquiryDate"] = Head.ServiceEnquiryDate.ToString("dd-MMM-yyyy");
            dr["CustomerName"] = Head.CustomerName;
            dr["CDoorNo"] = Head.CDoorNo;
            dr["CStreet"] = Head.CStreet;
            dr["CPhone"] = Head.CPhone;
            dr["CZip"] = Head.CZip;
            dr["CState"] = Head.CState;
            dr["CContactPerson"] = Head.CContactPerson;
            //dr["VehicleMake"] = Head.VehicleMake;
            //dr["VehicleRegNo"] = Head.VehicleRegNo;
            //dr["VehicleChassisNo"] = Head.VehicleChassisNo;
            //dr["VehicleKm"] = Head.VehicleKm;
            //dr["BoxMake"] = Head.BoxMake;
            //dr["BoxNo"] = Head.BoxNo;

            //dr["BoxSize"] = Head.BoxSize;
            //dr["FreezerMake"] = Head.FreezerMake;
            //dr["FreezerModel"] = Head.FreezerModel;
            //dr["FreezerSerialNo"] = Head.FreezerSerialNo;
            //dr["FreezerHours"] = Head.FreezerHours;
            //dr["TailLiftMake"] = Head.TailLiftMake;
            //dr["TailLiftModel"] = Head.TailLiftModel;
            //dr["TailLiftSerialNo"] = Head.TailLiftSerialNo;
            dr["UnitDetails"] = Head.UnitDetails;
            dr["Complaints"] = Head.Complaints;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["CreatedUser"] = Head.CreatedUser;
            dr["CreatedUsersig"] = Server.MapPath("~/App_images/") + Head.CreatedUsersig;
            dr["CreatedDes"] = Head.CreatedDes;
            ds.Tables["Head"].Rows.Add(dr);



            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "JobRepairOrderProject.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}