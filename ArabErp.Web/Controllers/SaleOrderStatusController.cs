using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections;
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
    public class SaleOrderStatusController : BaseController
    {
        // GET: SaleOrderStatus
        public ActionResult Index()
        {
            SaleOrderStatusRepository repo = new SaleOrderStatusRepository();
            return View(repo.GetSaleOrderStatus());
        }

        public ActionResult Print()
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "SaleOrderStatusReport.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //    //-------HEAD

        
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT

            ds.Tables["Items"].Columns.Add("CustomerName");
            ds.Tables["Items"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Items"].Columns.Add("SaleOrderDate");
            ds.Tables["Items"].Columns.Add("VehicleMdlNameReg");
            ds.Tables["Items"].Columns.Add("SOAgeDays");
            ds.Tables["Items"].Columns.Add("WRNodue");
            ds.Tables["Items"].Columns.Add("Purreqdue");
            ds.Tables["Items"].Columns.Add("LpoNoDue");
            ds.Tables["Items"].Columns.Add("GRN");
            ds.Tables["Items"].Columns.Add("BatchAlloc");
            ds.Tables["Items"].Columns.Add("Inpassdate");
            ds.Tables["Items"].Columns.Add("Jobcardnodate");
            ds.Tables["Items"].Columns.Add("JobcardComp");
           
            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);


            SaleOrderStatusRepository repo1 = new SaleOrderStatusRepository();
            var Items = repo1.GetSaleOrderStatusDTPrint();

            foreach (var item in Items)
            {
                var SupplyOrderRegItem = new SaleOrderStatus
                {
                    CustomerName = item.CustomerName,
                    SaleOrderRefNo = item.SaleOrderRefNo,
                    SaleOrderDate = item.SaleOrderDate,
                    VehicleMdlNameReg = item.VehicleMdlNameReg,
                    SOAgeDays = item.SOAgeDays,
                    WorkShopRequest = item.WorkShopRequest,
                    PurchaseRequest = item.PurchaseRequest,
                    SuppyOrder = item.SuppyOrder,
                    GRN = item.GRN,
                    Allocation = item.Allocation,
                    VehicleInpass = item.VehicleInpass,
                    JobCard = item.JobCard,
                    JobCardComplete = item.JobCardComplete,
                

                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["CustomerName"] = SupplyOrderRegItem.CustomerName;
                dri["SaleOrderRefNo"] = SupplyOrderRegItem.SaleOrderRefNo;
                dri["SaleOrderDate"] = SupplyOrderRegItem.SaleOrderDate;
                dri["VehicleMdlNameReg"] = SupplyOrderRegItem.VehicleMdlNameReg;
                dri["SOAgeDays"] = SupplyOrderRegItem.SOAgeDays;
                dri["WRNodue"] = SupplyOrderRegItem.WorkShopRequest;
                dri["Purreqdue"] = SupplyOrderRegItem.PurchaseRequest;
                dri["LpoNoDue"] = SupplyOrderRegItem.SuppyOrder;
                dri["GRN"] = SupplyOrderRegItem.GRN;
                dri["BatchAlloc"] = SupplyOrderRegItem.Allocation;
                dri["Inpassdate"] = SupplyOrderRegItem.VehicleInpass;
                dri["Jobcardnodate"] = SupplyOrderRegItem.JobCard;
                dri["JobcardComp"] = SupplyOrderRegItem.JobCardComplete;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "SaleOrderStatusReport.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("SaleOrderStatusReport.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}