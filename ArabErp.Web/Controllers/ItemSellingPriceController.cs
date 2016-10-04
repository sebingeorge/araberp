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
    public class ItemSellingPriceController : BaseController
    {
        // GET: ItemSellingPrice
        public ActionResult Index()
        {
            ItemSellingPriceList obj = new ItemSellingPriceList();
            ItemSellingPriceRepository repo = new ItemSellingPriceRepository();
            obj = repo.GetItemSellingPrices(OrganizationId);
            return View(obj);
        }

        public ActionResult Save(ItemSellingPriceList model)
        {

            foreach (ItemSellingPrice item in model.ItemSellingPriceLists)
            {
                item.CreatedDate = System.DateTime.Now;
                item.CreatedBy = UserID.ToString();
                item.OrganizationId = OrganizationId;
            }

            var rtn = new ItemSellingPriceRepository().InsertItemSellingPrice(model.ItemSellingPriceLists);
      
            TempData["Success"] = "Added Successfully!";

            return RedirectToAction("Index");
        }


        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ItemSellingPrice.rpt"));

            DataSet ds = new DataSet();
          //  ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            //ds.Tables["Head"].Columns.Add("JobCardNo");
            //ds.Tables["Head"].Columns.Add("JobCardDate");
            //ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
            //ds.Tables["Head"].Columns.Add("CustomerName");
            //ds.Tables["Head"].Columns.Add("Phone");
            //ds.Tables["Head"].Columns.Add("ContactPerson");
            //ds.Tables["Head"].Columns.Add("Unit");
            //ds.Tables["Head"].Columns.Add("Customer");
            //ds.Tables["Head"].Columns.Add("Technician");
            //-------DT
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("CategoryName");
            ds.Tables["Items"].Columns.Add("ItemGroupName");
            ds.Tables["Items"].Columns.Add("ItemSubGroupName");
             ds.Tables["Items"].Columns.Add("UnitName");
             ds.Tables["Items"].Columns.Add("SellingPrice");
             ds.Tables["Items"].Columns.Add("Average");

            //ItemSellingPriceRepository() repo = new ItemSellingPriceRepository()();
            //var Head = repo.GetJobCardHD(Id);

            //DataRow dr = ds.Tables["Head"].NewRow();
            //dr["JobCardNo"] = Head.JobCardNo;
            //dr["JobCardDate"] = Head.JobCardDate.ToString("dd-MMM-yyyy");
            //dr["SaleOrderRefNo"] = Head.RegistrationNo;
            //dr["CustomerName"] = Head.CustomerName;
            //dr["Phone"] = Head.Phone;
            //dr["ContactPerson"] = Head.ContactPerson;
            //dr["Customer"] = Head.Customer;
            //dr["Unit"] = Head.FreezerUnitName;
            //dr["Technician"] = Head.Technician;
            //ds.Tables["Head"].Rows.Add(dr);


            JobCardTaskRepository repo = new JobCardTaskRepository();
            var Items = repo.GetJobCardDT(Id);
            foreach (var item in Items)
            {
                var JCItem = new JobCardTask
                {
                    TaskDate = item.TaskDate,
                    Employee = item.Employee,
                    Description = item.Description,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                };

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["TaskDate"] = JCItem.TaskDate.ToString("dd-MMM-yyyy");
                dri["Employee"] = JCItem.Employee;
                dri["Description"] = JCItem.Description;
                dri["StartTime"] = JCItem.StartTime;
                dri["EndTime"] = JCItem.EndTime;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "JobCard.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("JobCard{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}