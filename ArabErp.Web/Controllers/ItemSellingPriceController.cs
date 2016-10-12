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


        public ActionResult Print()
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ItemSellingPrice.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("OrganizationRefNo");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Image1");
            //-------DT
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("CategoryName");
            ds.Tables["Items"].Columns.Add("ItemGroupName");
            ds.Tables["Items"].Columns.Add("ItemSubGroupName");
            ds.Tables["Items"].Columns.Add("UnitName");
            ds.Tables["Items"].Columns.Add("SellingPrice");
            ds.Tables["Items"].Columns.Add("Average");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["OrganizationName"] = Head.OrganizationName;
            dr["OrganizationRefNo"] = Head.OrganizationRefNo;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["Phone"] = Head.Phone;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["Zip"] = Head.Zip;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1; 
           
            ds.Tables["Head"].Rows.Add(dr);


             ItemSellingPriceRepository repo1 = new ItemSellingPriceRepository();
             var Items = repo1.GetItemSellingPricesReport(OrganizationId);
            
             foreach (var item in Items)
             {
                 
                 DataRow dri = ds.Tables["Items"].NewRow();
                 dri["ItemName"] = item.ItemName;
                 dri["PartNo"] = item.PartNo;
                 dri["CategoryName"] = item.CategoryName;
                 dri["ItemGroupName"] = item.ItemGroupName;
                 dri["ItemSubGroupName"] = item.ItemSubGroupName;
                 dri["UnitName"] = item.UnitName;
                 dri["SellingPrice"] = item.SellingPrice;
                 dri["Average"] = item.Average;
                 ds.Tables["Items"].Rows.Add(dri);
             }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "ItemSellingPrice.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("ItemSellingPrice.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}