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
    public class VehicleInPassController : BaseController
    {
        // GET: VehicleInPass
        public ActionResult Index()
        {
            CustomerDropdown();
            return View();
        }
        public void CustomerDropdown()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown(), "Id", "Name");
        }
        public PartialViewResult PendingVehicleInPass(int customerId)
        {
            if (customerId == 0)
            {
                List<PendingSO> list = new List<PendingSO>();
                return PartialView("_PendingVehicleInPass", list);
            }
            return PartialView("_PendingVehicleInPass", new VehicleInPassRepository().PendingVehicleInpass(customerId));
        }
        public ActionResult Save(int id = 0)
        {
            if (id != 0)
            {
                EmployeeDropdown();
                string internalId = "";
                try
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(15, OrganizationId);

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
                return View(new VehicleInPass { SaleOrderItemId = id, VehicleInPassDate = DateTime.Now, VehicleInPassNo = internalId });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(VehicleInPass model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                if (new VehicleInPassRepository().InsertVehicleInPass(model) > 0)
                {
                    TempData["Success"] = "Saved Successfully! Vehicle In-Pass No. is " + model.VehicleInPassNo;
                    //TempData["VehicleInPassNo"] = model.VehicleInPassNo;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Some error occurred. Please try again.";
                    //TempData["VehicleInPassNo"] = null;
                    EmployeeDropdown();
                    return View(new VehicleInPass { SaleOrderItemId = model.SaleOrderItemId });
                }
            }
            catch (DuplicateNameException)
            {
                TempData["error"] = "One vehicle inpass already exists for this Sale Order";
                return RedirectToAction("Index");
            }

        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public JsonResult GetSaleOrderItemDetails(int id = 0)
        {
            var data = new VehicleInPassRepository().GetSaleOrderItemDetails(id);
            return Json(new
            {
                SaleOrderRefNo = data.SaleOrderRefNo,
                WorkDescr = data.WorkDescription,
                VehicleModelName = data.VehicleModelName,
                CustomerName = data.CustomerName
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VehicleInpassList()
        {
            FillVINo();
            FillCustomerinVI();
            return View();
        }
        public void FillVINo()
        {
            ViewBag.VINoList = new SelectList(new DropdownRepository().VINODropdown(OrganizationId), "Id", "Name");
        }
        public void FillCustomerinVI()
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().VICustomerDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousList", new VehicleInPassRepository().GetPreviousList(id, cusid, OrganizationId, from, to));
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    EmployeeDropdown();
                    VehicleInPass VehicleInPass = new VehicleInPass();
                    VehicleInPass = new VehicleInPassRepository().GetVehicleInPassHD(id);


                    return View(VehicleInPass);
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
            return RedirectToAction("VehicleInpassList");
        }

        [HttpPost]
        public ActionResult Edit(VehicleInPass model)
        {
            EmployeeDropdown();

            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                new VehicleInPassRepository().UpdateVehicleInPass(model);

                TempData["success"] = "Updated Successfully (" + model.VehicleInPassNo + ")";
                //TempData["VehicleInPassNo"] = model.VehicleInPassNo;
                return RedirectToAction("VehicleInpassList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }

        public ActionResult Delete(int VehicleInPassId = 0)
        {
            try
            {
                if (VehicleInPassId == 0) return RedirectToAction("Index", "Home");
                string ref_no = new VehicleInPassRepository().DeleteVehicleInPass(VehicleInPassId);

                TempData["Success"] = "Deleted Successfully! (" + ref_no + ")";
                return RedirectToAction("VehicleInpassList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = VehicleInPassId });
            }
        }

        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "VehicleInPass.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("VehicleInPassNo");
            ds.Tables["Head"].Columns.Add("VehicleInPassDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("WONODATE");
            ds.Tables["Head"].Columns.Add("SONODATE");
            ds.Tables["Head"].Columns.Add("RequiredDate");
            ds.Tables["Head"].Columns.Add("Remarks");
            ds.Tables["Head"].Columns.Add("EmployeeName");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");


            //-------DT
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("RequiredQuantity");
            ds.Tables["Items"].Columns.Add("IssuedQuantity");
            ds.Tables["Items"].Columns.Add("PendingQuantity");
            ds.Tables["Items"].Columns.Add("StockQuantity");
            ds.Tables["Items"].Columns.Add("CurrentIssuedQuantity");
            ds.Tables["Items"].Columns.Add("UnitName");



            StoreIssueRepository repo = new StoreIssueRepository();
            var Head = repo.GetStoreIssueHDPrint(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["StoreIssueRefNo"] = Head.StoreIssueRefNo;
            dr["StoreIssueDate"] = Head.StoreIssueDate;
            dr["StockpointName"] = Head.StockpointName;
            dr["CustomerName"] = Head.CustomerName;
            dr["WONODATE"] = Head.WONODATE;
            dr["SONODATE"] = Head.SONODATE;
            dr["RequiredDate"] = Head.RequiredDate;
            dr["Remarks"] = Head.Remarks;
            dr["EmployeeName"] = Head.EmployeeName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Head.Image1;

            ds.Tables["Head"].Rows.Add(dr);

            StoreIssueItemRepository repo1 = new StoreIssueItemRepository();
            var Items = repo1.GetStoreIssueDTPrint(Id, OrganizationId);
            foreach (var item in Items)
            {
                var pritem = new StoreIssueItem
                {
                    ItemName = item.ItemName,
                    RequiredQuantity = item.RequiredQuantity,
                    IssuedQuantity = item.IssuedQuantity,
                    PendingQuantity = item.PendingQuantity,
                    StockQuantity = item.StockQuantity,
                    CurrentIssuedQuantity = item.CurrentIssuedQuantity,
                    UnitName = item.UnitName,

                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["ItemName"] = pritem.ItemName;
                dri["RequiredQuantity"] = pritem.RequiredQuantity;
                dri["IssuedQuantity"] = pritem.IssuedQuantity;
                dri["PendingQuantity"] = pritem.PendingQuantity;
                dri["StockQuantity"] = pritem.StockQuantity;
                dri["CurrentIssuedQuantity"] = pritem.CurrentIssuedQuantity;
                dri["UnitName"] = pritem.UnitName;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "VehicleInPass.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("VehicleInPass{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}