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
    public class ItemController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        // GET: Item
        public ActionResult Create()
        {
            FillItemCategory();
            FillUnit();
            FillItem();
            FillJobCardTaskMaster();
            InitDropdown();
            Item oItem = new Item();
            oItem.PartNo = null;
            oItem.ItemName = null;
            oItem.ItemPrintName = null;
            oItem.ItemShortName = null;
            oItem.CommodityId = null;
            oItem.ItemCategoryId = 0;
            oItem.ItemGroupId = 0;
            oItem.ItemSubGroupId = 0;
            oItem.ItemUnitId = null;
            oItem.ExpiryDate = DateTime.Now;
            oItem.MinLevel = null;
            oItem.ReorderLevel = null;
            oItem.MaxLevel = null;
            oItem.StockRequired = false;
            oItem.BatchRequired = false;
            oItem.ItemRefNo = "ITM/" + DatabaseCommonRepository.GetNextRefNoWithNoUpdate(typeof(Item).Name);
            oItem.ItemVsBom.Add(new WorkVsItem());
            oItem.ItemVsTasks.Add(new WorkVsTask());

            return View("Create", oItem);
        }
        [HttpPost]
        public ActionResult Create(Item oitem)
        {
            FillItemCategory();
            FillUnit();
            InitDropdown();
            oitem.OrganizationId = OrganizationId;
            oitem.CreatedDate = System.DateTime.Now;
            oitem.CreatedBy = UserID.ToString();

            var repo = new ItemRepository();

            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Item", "ItemName", oitem.ItemName, null, null);
            if (!isexists)
            {

                if (oitem.PartNo != null)

                    isexists = repo.IsFieldExists(repo.ConnectionString(), "Item", "PartNo", oitem.PartNo, null, null);

                if (!isexists)
                {


                    var result = new ItemRepository().InsertItem(oitem);
                    if (result.ItemId > 0)
                    {

                        TempData["Success"] = "Saved Successfully! Reference No. is " + result.ItemRefNo;
                        return RedirectToAction("Index");
                    }

                    else
                    {
                        FillUnit();
                        FillItem();
                        FillJobCardTaskMaster();
                        TempData["error"] = "Some error occurred. Please try again.";
                        return View("Create", oitem);
                    }

                }
                else
                {

                    FillUnit();
                    FillItem();
                    FillJobCardTaskMaster();
                    TempData["error"] = "This part no. already exists!";
                    return View("Create", oitem);
                }
            }
            else
            {
                FillUnit();
                FillItem();
                FillJobCardTaskMaster();
                TempData["error"] = "This material/spare name alredy exists!";
                return View("Create", oitem);
            }
        }

        public ActionResult View(int Id)
        {

            Item objItem = new JobCardRepository().GetItem(Id);
            return View("Create", objItem);
        }

        public ActionResult Edit(int Id)
        {
            Item objItem = new ItemRepository().GetItem(Id);
            FillUnit();
            FillItem();
            FillJobCardTaskMaster();
            objItem.ItemVsBom = new ItemRepository().GetItemVsBom(Id);
            objItem.ItemVsTasks = new ItemRepository().GetItemVsTasks(Id);
            return View(objItem);

        }
        [HttpPost]
        public ActionResult Edit(Item model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var repo = new ItemRepository();


            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "Item", "ItemName", model.ItemName, "ItemId", model.ItemId);
            if (!isexists)
            {
                var result = new ItemRepository().UpdateItem(model);

                if (result.ItemId > 0)
                {
                    TempData["Success"] = "Updated Successfully! (" + result.ItemRefNo + ")";
                    return RedirectToAction("Index");
                }
                else
                {
                    FillUnit();
                    TempData["error"] = "Some error occurred. Please try again.";
                    return View("Edit", model);
                }
            }
            else
            {

                FillUnit();
                TempData["error"] = "This material/spare name already exists!";

                return View("Edit", model);
            }

        }


        public ActionResult Delete(int Id)
        {

            Item objItem = new ItemRepository().GetItem(Id);

            FillUnit();


            return View(objItem);


        }
        [HttpPost]
        public ActionResult Delete(Item model)
        {

            int result = new ItemRepository().DeleteItem(model);


            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["ItemRefNo"] = model.ItemRefNo;
                return RedirectToAction("Index");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Item. It Is Already In Use";
                    TempData["ItemRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["ItemRefNo"] = null;
                }
                return RedirectToAction("Index");
            }

        }


        public void FillItemSubGroup(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemSubGroup(Id);
            ViewBag.ItemSubGroupList = new SelectList(List, "Id", "Name");
        }
        public void FillItemCategory()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemCategory();
            ViewBag.ItemCategoryList = new SelectList(List, "Id", "Name");
        }
        public void FillItemGroup(int Id)
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillItemGroup(Id);
            ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
        }
        public void FillUnit()
        {
            ItemRepository Repo = new ItemRepository();
            var List = Repo.FillUnit();
            ViewBag.Unit = new SelectList(List, "Id", "Name");
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.ItemGroupList = new SelectList(List, "Id", "Name");
            ViewBag.ItemSubGroupList = new SelectList(List, "Id", "Name");
        }
        public ActionResult ItemGroup(int Code)
        {
            FillItemGroup(Code);
            return PartialView("_ItemGroupDropdown");
        }
        public ActionResult ItemSubGroup(int Code)
        {
            FillItemSubGroup(Code);
            return PartialView("_ItemSubGroupDropdown");
        }
        public ActionResult ItemCategory()
        {
            FillItemCategory();
            return PartialView("_ItemCategoryDropdown");
        }
        public ActionResult ItemList(int? page, string name = "", string group = "", string subgroup = "")
        {
            //int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            return PartialView("_ItemListView", new ItemRepository().GetItems(name: name.Trim(), group: group.Trim(), subgroup: subgroup.Trim()));
            //var repo = new ItemRepository();
            //var List = repo.GetItems();
            //return PartialView("_ItemListView",List);
        }


        public ActionResult Print(string name)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Material.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD

            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("CurrencyName");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");

            //-------DT
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("ItemCategoryName");
            ds.Tables["Items"].Columns.Add("ItemGrpName");
            ds.Tables["Items"].Columns.Add("ItemSubGrpName");
            ds.Tables["Items"].Columns.Add("UOM");


            OrganizationRepository repo = new OrganizationRepository();
            var Head = repo.GetOrganization(OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();

            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["ContactPerson"] = Head.ContactPerson;
            //dr["CurrencyName"] = Head.CurrencyName;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            ItemRepository repo1 = new ItemRepository();
            var Items = repo1.GetItemsDTPrint(name);
            foreach (var item in Items)
            {
                var pritem = new Item
                {
                    PartNo = item.PartNo,
                    ItemName = item.ItemName,
                    CategoryName = item.CategoryName,
                    ItemGroupName = item.ItemGroupName,
                    ItemSubGroupName = item.ItemSubGroupName,
                    UnitName = item.UnitName,

                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["PartNo"] = pritem.PartNo;
                dri["ItemName"] = pritem.ItemName;
                dri["ItemCategoryName"] = pritem.CategoryName;
                dri["ItemGrpName"] = pritem.ItemGroupName;
                dri["ItemSubGrpName"] = pritem.ItemSubGroupName;
                dri["UOM"] = pritem.UnitName;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "Material.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("Material.pdf"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void FillItem()
        {
            DropdownRepository Repo = new DropdownRepository();
            var List = Repo.FillItem();
            ViewBag.ItemList = new SelectList(List, "Id", "Name");
        }

        public void FillJobCardTaskMaster()
        {
            JobCardTaskMasterRepository Repo = new JobCardTaskMasterRepository();
            var List = Repo.FillJobCardTaskMaster();
            ViewBag.JobCardTaskMasterList = new SelectList(List, "Id", "Name");
        }
    }
}