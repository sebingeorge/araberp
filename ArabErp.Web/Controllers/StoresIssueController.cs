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
    public class StoresIssueController : BaseController
    {
        // GET: StoresIssue
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Issuance(int id = 0)
        {
            string referenceNo = DatabaseCommonRepository.GetNextDocNo(24, OrganizationId);
            FillDropdowns();
            if (id == 0) return RedirectToAction("Pending");
            return View(new StoreIssue { WorkShopRequestId = id, StoreIssueDate = DateTime.Today, StoreIssueRefNo = referenceNo });
        }
        [HttpPost]
        public ActionResult Issuance(StoreIssue model)
        {
            try
            {
                List<int> temp = (from StoreIssueItem i in model.Items
                                  where i.CurrentIssuedQuantity > 0
                                  select i.StoreIssueId).ToList();
                List<StoreIssueItem> items = model.Items.Where(m => m.CurrentIssuedQuantity > 0).ToList();
                if (temp.Count == 0)
                {
                    TempData["error"] = "Atleast one of the quantities must be greater than zero";
                    goto ReturnSameView;
                }

                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string result = new StoreIssueRepository().InsertStoreIssue(model);
                if (result.Length != 0) //if insert success
                {
                    TempData["success"] = "Saved succesfully. Reference No. is " + result;
                    TempData["error"] = "";
                    return RedirectToAction("Pending");
                }
                else throw new Exception();
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
            }
            catch (NullReferenceException nx)
            {
                TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }

        ReturnSameView:
            FillDropdowns();
            TempData["success"] = "";
            return View(model); //if insert fails
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    FillDropdowns();
                    StoreIssue StoreIssue = new StoreIssue();
                    StoreIssue = new StoreIssueRepository().GetStoreIssueHD(id);
                    StoreIssue.Items = new StoreIssueItemRepository().GetStoreIssueDT(id);

                    return View(StoreIssue);
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

        private void FillDropdowns()
        {
            EmployeeDropdown();
            StockpointDropdown();
        }
        public ActionResult Pending()
        {
            return View();
        }
        public PartialViewResult PendingWorkshopRequests(string Request = "", string Jobcard = "", string Customer = "", string jcno = "")
        {
            return PartialView("_PendingWorkshopRequests", new WorkShopRequestRepository().PendingWorkshopRequests(Request, Jobcard, Customer, jcno));
        }
        public PartialViewResult PendingWorkshopRequestDetails()
        {
            StoreIssue _model = new StoreIssue { Items = new StoreIssueRepository().PendingWorkshopRequestItems(Convert.ToInt32(Request.QueryString["id"])).ToList() };
            return PartialView("_IssuanceItems", _model);
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public void StockpointDropdown()
        {
            ViewBag.stockpointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }
        public JsonResult WorkshopRequestHeadDetails(int workshopRequestId)
        {
            string data = new StoreIssueRepository().WorkshopRequestHeadDetails(workshopRequestId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PreviousList()
        {
            return View(new StoreIssueRepository().PreviousList(OrganizationId));
        }

        public ActionResult GetStockQuantity(string date, int item = 0, int stockpoint = 0)
        {
            try
            {
                if (item == 0 || stockpoint == 0 || date == null) throw new InvalidOperationException();
                ClosingStock stock = new ClosingStockRepository().GetClosingStockData(Convert.ToDateTime(date), stockpoint, 0, item, OrganizationId).First();
                return Json(stock.Quantity, JsonRequestBehavior.AllowGet);
            }
            catch (InvalidOperationException)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Pendinglist(int? page, string Request = "", string Jobcard = "", string Customer = "")
        {
            //int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            return PartialView("PendingWorkshopRequests", new StoreIssueRepository().GetItems(Request: Request.Trim(), Jobcard: Jobcard.Trim(), Customer: Customer.Trim()));
            //var repo = new ItemRepository();
            //var List = repo.GetItems();
            //return PartialView("_ItemListView",List);
        }




        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "StoreIssue.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");
            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("StoreIssueRefNo");
            ds.Tables["Head"].Columns.Add("StoreIssueDate"); 
            ds.Tables["Head"].Columns.Add("StockpointName");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("WONODATE");
            ds.Tables["Head"].Columns.Add("SONODATE");
            ds.Tables["Head"].Columns.Add("RequiredDate");
            ds.Tables["Head"].Columns.Add("Remarks");
            ds.Tables["Head"].Columns.Add("EmployeeName");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("ContactPerson");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("JobCardNo");


            //-------DT
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("RequiredQuantity");
            ds.Tables["Items"].Columns.Add("IssuedQuantity");
            ds.Tables["Items"].Columns.Add("PendingQuantity");
            ds.Tables["Items"].Columns.Add("StockQuantity");
            ds.Tables["Items"].Columns.Add("CurrentIssuedQuantity");
            ds.Tables["Items"].Columns.Add("UnitName");
            ds.Tables["Items"].Columns.Add("PartNo");



            StoreIssueRepository repo = new StoreIssueRepository();
            var Head = repo.GetStoreIssueHDPrint(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["StoreIssueRefNo"] = Head.StoreIssueRefNo;
            dr["StoreIssueDate"] = Head.StoreIssueDate.ToString("dd-MMM-yyyy");
            dr["StockpointName"] = Head.StockpointName;
            dr["CustomerName"] = Head.CustomerName;
            dr["WONODATE"] = Head.WONODATE;
            dr["SONODATE"] = Head.SONODATE;
            dr["RequiredDate"] = Head.RequiredDate;
            dr["Remarks"] = Head.Remarks;
            dr["EmployeeName"] = Head.EmployeeName;
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["ContactPerson"] = Head.ContactPerson;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Head.Image1;
            dr["JobCardNo"] = Head.JobCardNo;

            ds.Tables["Head"].Rows.Add(dr);

            StoreIssueItemRepository repo1 = new StoreIssueItemRepository();
            var Items = repo1.GetStoreIssueDTPrint(Id,OrganizationId);
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
                    PartNo=item.PartNo
                    

                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["ItemName"] = pritem.ItemName;
                dri["RequiredQuantity"] = pritem.RequiredQuantity;
                dri["IssuedQuantity"] = pritem.IssuedQuantity;
                dri["PendingQuantity"] = pritem.PendingQuantity;
                dri["StockQuantity"] = pritem.StockQuantity;
                dri["CurrentIssuedQuantity"] = pritem.CurrentIssuedQuantity;
                dri["UnitName"] = pritem.UnitName;
                dri["PartNo"] = pritem.PartNo;

                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "StoreIssue.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("StoreIssue{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}