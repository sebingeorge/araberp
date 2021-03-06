﻿using System;
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
    public class ProjectCompletionController : BaseController
    {
        // GET: ProjectCompletion
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PreviousList(string project = "", string saleorder = "")
        {
            return PartialView("_PreviousListGrid", new ProjectCompletionRepository().GetPreviousList(project: project, saleorder: saleorder, OrganizationId: OrganizationId));
        }

        public ActionResult Pending()
        {
            try
            {
                return View(new ProjectCompletionRepository().PendingForCompletion(OrganizationId));
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult PendingGrid(string saleorder = "")
        {
            return PartialView("_PendingGrid", new ProjectCompletionRepository().PendingForCompletion(
                OrganizationId: OrganizationId,
                saleorder: saleorder));
        }

        public ActionResult Complete(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            ProjectCompletion model = new ProjectCompletionRepository().GetProjectDetails(id, OrganizationId);
            model.SaleOrderId = id;
            model.ProjectCompletionDate = DateTime.Today;
            model.ProjectWarrantyExpiryDate = DateTime.Today.AddYears(1).AddDays(-1);
            model.ProjectCompletionRefNo = DatabaseCommonRepository.GetNextDocNo(30, OrganizationId);
            model.ItemBatches = new ProjectCompletionRepository().GetSerialNos(id).ToList<ItemBatch>();
            return View(model);
            //return View(new ProjectCompletion
            //{
            //    SaleOrderId = id,
            //    ProjectCompletionDate = DateTime.Today,
            //    ProjectWarrantyExpiryDate = DateTime.Today.AddYears(1).AddDays(-1),
            //    ProjectCompletionRefNo = DatabaseCommonRepository.GetNextDocNo(30, OrganizationId),
            //    ProjectName = model.ProjectName,
            //    Location = model.Location,
            //    CustomerName = model.CustomerName,
            //    ItemBatches = new ProjectCompletionRepository().GetSerialNos(id).ToList<ItemBatch>()
            //});
        }

        public ActionResult SaleOrderDetails(int id)
        {
            return PartialView("_SaleOrderDetails", new SaleOrderRepository().GetSaleOrder(id));
        }//SaleOrderId is received here

        //public ActionResult JobCardDetails(int id)
        //{
        //    return PartialView("_JobCardDetails", new ProjectCompletionRepository().GetJobCardDetails(id));
        //}

        public ActionResult ItemBatchDetails(int id)//SaleOrderId is received here
        {
            ProjectCompletion model = new ProjectCompletion();
            model.ItemBatches = new ProjectCompletionRepository().GetSerialNos(id).ToList<ItemBatch>();
            return PartialView("_ItemBatchDetails", model);
        }

        [HttpPost]
        public ActionResult Complete(ProjectCompletion model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                if (model.ItemBatches != null && model.ItemBatches.Count > 0)
                    foreach (ItemBatch item in model.ItemBatches)
                    {
                        item.WarrantyStartDate = model.ProjectCompletionDate;
                        item.WarrantyExpireDate = model.ProjectCompletionDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                    }
                string ref_no = new ProjectCompletionRepository().InsertProjectCompletion(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                return RedirectToAction("Pending");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try agian.";
            }
            return View(model);
        }

        public ActionResult Details(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            ProjectCompletion model = new ProjectCompletionRepository().GetProjectCompletion(id);

            model.ItemBatches = new ProjectCompletionRepository().GetSerialNosByProjectCompletioId(model.ProjectCompletionId).ToList<ItemBatch>();
            return View(model);
        }

        [HttpPost]
        public ActionResult Details(ProjectCompletion model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                if (model.ItemBatches != null && model.ItemBatches.Count > 0)
                    foreach (ItemBatch item in model.ItemBatches)
                    {
                        item.WarrantyStartDate = model.ProjectCompletionDate;
                        item.WarrantyExpireDate = model.ProjectCompletionDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                    }

                new ProjectCompletionRepository().UpdateProjectCompletion(model);
                TempData["success"] = "Updated Successfully (" + model.ProjectCompletionRefNo + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
            return View(model);
        }

        public ActionResult Delete(int ProjectCompletionId = 0)
        {
            try
            {
                if (ProjectCompletionId == 0) return RedirectToAction("Index", "Home");
                //JobCard model = new JobCardRepository().GetJobCardDetails2(JobCardId, OrganizationId);
                string ref_no = new ProjectCompletionRepository().DeleteProjectCompletion(ProjectCompletionId);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Details", new { id = ProjectCompletionId });
            }

        }

        //public ActionResult ProjectCompletionReport(int Id)
        //{

        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProjectCompletion.rpt"));

        //    DataSet ds = new DataSet();
        //    ds.Tables.Add("Head");

        //    //-------HEAD
        //    ds.Tables["Head"].Columns.Add("ProjectCompletionRefNo");
        //    ds.Tables["Head"].Columns.Add("ProjectCompletionDate");
        //    ds.Tables["Head"].Columns.Add("ProjectName");
        //    ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
        //    ds.Tables["Head"].Columns.Add("SaleOrderDate");
        //    ds.Tables["Head"].Columns.Add("CustomerName");

        //    ds.Tables["Head"].Columns.Add("ChillerTemperature");
        //    ds.Tables["Head"].Columns.Add("ChillerDimension");
        //    ds.Tables["Head"].Columns.Add("ChillerCondensingUnit");
        //    ds.Tables["Head"].Columns.Add("ChillerEvaporator");
        //    ds.Tables["Head"].Columns.Add("ChillerRefrigerant");
        //    ds.Tables["Head"].Columns.Add("ChillerQuantity");

        //    ds.Tables["Head"].Columns.Add("RoomDetails");
        //    ds.Tables["Head"].Columns.Add("FreezerTemperature");
        //    ds.Tables["Head"].Columns.Add("FreezerDimension");
        //    ds.Tables["Head"].Columns.Add("FreezerCondensingUnit");
        //    ds.Tables["Head"].Columns.Add("FreezerEvaporator");
        //    ds.Tables["Head"].Columns.Add("FreezerRefrigerant");
        //    ds.Tables["Head"].Columns.Add("FreezerQuantity");
          
        //    //Organization
        //    ds.Tables["Head"].Columns.Add("DoorNo");
        //    ds.Tables["Head"].Columns.Add("Street");
        //    ds.Tables["Head"].Columns.Add("State");
        //    ds.Tables["Head"].Columns.Add("Country");
        //    ds.Tables["Head"].Columns.Add("Phone");
        //    ds.Tables["Head"].Columns.Add("Fax");
        //    ds.Tables["Head"].Columns.Add("Email");
        //    ds.Tables["Head"].Columns.Add("ContactPerson");
        //    ds.Tables["Head"].Columns.Add("Zip");
        //    ds.Tables["Head"].Columns.Add("OrganizationName");
        //    ds.Tables["Head"].Columns.Add("Image1");
        //    ds.Tables["Head"].Columns.Add("CreatedUser");
        //    ds.Tables["Head"].Columns.Add("CreateSignature");
        //    ds.Tables["Head"].Columns.Add("CreatedDes");
        //    ds.Tables["Head"].Columns.Add("SerialNo");

        //    ProjectCompletionRepository repo = new ProjectCompletionRepository();
        //    var Head = repo.GetProjectCompletionHD(Id, OrganizationId);

        //    DataRow dr = ds.Tables["Head"].NewRow();
        //    dr["ProjectCompletionRefNo"] = Head.ProjectCompletionRefNo;
        //    dr["ProjectCompletionDate"] = Head.ProjectCompletionDate.ToString("dd-MMM-yyyy");
        //    dr["ProjectName"] = Head.ProjectName;
        //    dr["SaleOrderRefNo"] = Head.SaleOrderRefNo;
        //    dr["SaleOrderDate"] = Head.SaleOrderDate;
        //    dr["CustomerName"] = Head.CustomerName;

        //    dr["ChillerTemperature"] = Head.ChillerTemperature;
        //    dr["ChillerDimension"] = Head.ChillerDimension;
        //    dr["ChillerCondensingUnit"] = Head.ChillerDimension;
        //    dr["ChillerEvaporator"] = Head.ChillerEvaporator;
        //    dr["ChillerRefrigerant"] = Head.ChillerRefrigerant;
        //    dr["ChillerQuantity"] = Head.ChillerQuantity;

        //    dr["RoomDetails"] = Head.RoomDetails;
        //    dr["FreezerTemperature"] = Head.FreezerTemperature;
        //    dr["FreezerDimension"] = Head.FreezerDimension;
        //    dr["FreezerCondensingUnit"] = Head.FreezerCondensingUnit;
        //    dr["FreezerEvaporator"] = Head.FreezerEvaporator;
        //    dr["FreezerRefrigerant"] = Head.FreezerRefrigerant;
        //    dr["FreezerQuantity"] = Head.FreezerQuantity;

        //    dr["DoorNo"] = Head.DoorNo;
        //    dr["Street"] = Head.Street;
        //    dr["State"] = Head.State;
        //    dr["Country"] = Head.CountryName;
        //    dr["Phone"] = Head.Phone;
        //    dr["Fax"] = Head.Fax;
        //    dr["Email"] = Head.Email;
        //    dr["ContactPerson"] = Head.ContactPerson;
        //    dr["Zip"] = Head.Zip;
        //    dr["OrganizationName"] = Head.OrganizationName;
        //    dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
        //    dr["CreatedUser"] = Head.CreatedUser;
        //    dr["CreateSignature"] = Server.MapPath("~/App_Images/") + Head.CreatedUsersig;
        //    dr["CreatedDes"] = Head.CreatedDes;
        //    dr["SerialNo"] = Head.CreatedDes;
        //    ds.Tables["Head"].Rows.Add(dr);


        //    ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProjectCompletion.xml"), XmlWriteMode.WriteSchema);

        //    rd.SetDataSource(ds);

        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();


        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public ActionResult ProjectCompletionReport(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ProjectCommissioning.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Item");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("ProjectCompletionRefNo");
            ds.Tables["Head"].Columns.Add("ProjectCompletionDate");
            ds.Tables["Head"].Columns.Add("ProjectName");
            ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Head"].Columns.Add("SaleOrderDate");
            ds.Tables["Head"].Columns.Add("CustomerName");

            ds.Tables["Head"].Columns.Add("ChillerTemperature");
            ds.Tables["Head"].Columns.Add("ChillerDimension");
            ds.Tables["Head"].Columns.Add("ChillerCondensingUnit");
            ds.Tables["Head"].Columns.Add("ChillerEvaporator");
            ds.Tables["Head"].Columns.Add("ChillerRefrigerant");
            ds.Tables["Head"].Columns.Add("ChillerQuantity");

           
            //Organization
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("Country");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("CreatedUser");
            ds.Tables["Head"].Columns.Add("CreateSignature");
            ds.Tables["Head"].Columns.Add("CreatedDes");
         
            //-------DT
            ds.Tables["Item"].Columns.Add("RoomDetails");
            ds.Tables["Item"].Columns.Add("FreezerTemperature");
            ds.Tables["Item"].Columns.Add("FreezerDimension");
            ds.Tables["Item"].Columns.Add("FreezerCondensingUnit");
            ds.Tables["Item"].Columns.Add("FreezerEvaporator");
            ds.Tables["Item"].Columns.Add("FreezerRefrigerant");
            ds.Tables["Item"].Columns.Add("FreezerQuantity");
            ds.Tables["Item"].Columns.Add("SerialNo");


            ProjectCompletionRepository repo = new ProjectCompletionRepository();
            var Head = repo.GetProjectCompletionHD(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["ProjectCompletionRefNo"] = Head.ProjectCompletionRefNo;
            dr["ProjectCompletionDate"] = Head.ProjectCompletionDate.ToString("dd-MMM-yyyy");
            dr["ProjectName"] = Head.ProjectName;
            dr["SaleOrderRefNo"] = Head.SaleOrderRefNo;
            dr["SaleOrderDate"] = Head.SaleOrderDate;
            dr["CustomerName"] = Head.CustomerName;

            dr["ChillerTemperature"] = Head.ChillerTemperature;
            dr["ChillerDimension"] = Head.ChillerDimension;
            dr["ChillerCondensingUnit"] = Head.ChillerDimension;
            dr["ChillerEvaporator"] = Head.ChillerEvaporator;
            dr["ChillerRefrigerant"] = Head.ChillerRefrigerant;
            dr["ChillerQuantity"] = Head.ChillerQuantity;

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
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            dr["CreatedUser"] = Head.CreatedUser;
            dr["CreateSignature"] = Server.MapPath("~/App_Images/") + Head.CreatedUsersig;
            dr["CreatedDes"] = Head.CreatedDes;
             ds.Tables["Head"].Rows.Add(dr);

            ProjectCompletionRepository repo1 = new ProjectCompletionRepository();
            var Items = repo1.GetProjectCompletionDT(Id, OrganizationId);
           
             foreach (var item in Items)
            {
                DataRow dri = ds.Tables["Item"].NewRow();
                dri["RoomDetails"] = item.RoomDetails;
                dri["FreezerTemperature"] = item.FreezerTemperature;
                dri["FreezerDimension"] = item.FreezerDimension;
                dri["FreezerCondensingUnit"] = item.FreezerCondensingUnit;
                dri["FreezerEvaporator"] = item.FreezerEvaporator;
                dri["FreezerRefrigerant"] = item.FreezerRefrigerant;
                dri["FreezerQuantity"] = item.FreezerQuantity;
                dri["SerialNo"] = item.SerialNo;
                ds.Tables["Item"].Rows.Add(dri);
            }
             
            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ProjectCommissioning.xml"), XmlWriteMode.WriteSchema);

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