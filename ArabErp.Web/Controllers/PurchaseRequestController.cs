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
    public class PurchaseRequestController : BaseController
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET: PurchaseRequest
        public ActionResult Index()
        {
            FillPRRefNo();
            FillPRCustomer();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int cusid = 0)
        {
            return PartialView("_PreviousList", new PurchaseRequestRepository().GetPurchaseRequest(OrganizationId: OrganizationId, id: id, cusid: cusid, from: from, to: to));
        }

        public ActionResult Pending()
        {
            FillWRCustomer(OrganizationId);
            return View();
        }

        public ActionResult PendingPurchaseRequest(int cusid = 0, string WRNo = "")
        {
            return PartialView("_PendingPurchaseRequest", new PurchaseRequestRepository().GetWorkShopRequestPending(OrganizationId, cusid, WRNo));

            //var repo = new PurchaseRequestRepository();
            //IEnumerable<PendingWorkShopRequest> pendingWR = repo.GetWorkShopRequestPending(OrganizationId);
            //return View(pendingWR);
        }
        public ActionResult Create(int? WorkShopRequestId)
        {
            PurchaseRequestRepository repo = new PurchaseRequestRepository();
           
            PurchaseRequest model = repo.GetPurchaseRequestDetails(WorkShopRequestId ?? 0);

            var PRList = repo.GetPurchaseRequestItem(WorkShopRequestId ?? 0);
            model.items = new List<PurchaseRequestItem>();
            foreach (var item in PRList)
            {
                var pritem = new PurchaseRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, 
                    Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, MinLevel = item.MinLevel, 
                    WRRequestQty = item.WRRequestQty, CurrentStock = item.CurrentStock, WRIssueQty = item.WRIssueQty, 
                    TotalQty = item.TotalQty,InTransitQty=item.InTransitQty,PendingPRQty=item.PendingPRQty,ShortorExcess=item .ShortorExcess  };
                    model.items.Add(pritem);

            }
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(8, OrganizationId);

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

            model.PurchaseRequestNo = internalId;
            model.PurchaseRequestDate = System.DateTime.Today;
            model.RequiredDate = System.DateTime.Today;
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            return View(model);
        }
        [HttpPost]
        public ActionResult Create(PurchaseRequest model)
        {
             
         
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(model);
                }
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
               
                string id = new PurchaseRequestRepository().InsertPurchaseRequest(model);
                   if (id.Split('|')[0] != "0")
                   {
                       TempData["success"] = "Saved successfully. Purchase Request Reference No. is " + id.Split('|')[1];
                       TempData["error"] = "";
                       return RedirectToAction("Pending");
                   }
                   else
                   {
                       throw new Exception();
                   }
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
            return RedirectToAction("Pending");
        }

        public ActionResult Edit(int? id)
        {
            var repo = new PurchaseRequestRepository();
            PurchaseRequest model = repo.GetPurchaseRequestHDDetails(id ?? 0);

            var PRList = repo.GetPurchaseRequestDTDetails(id ?? 0);
            model.items = new List<PurchaseRequestItem>();
            foreach (var item in PRList)
            {
                var pritem = new PurchaseRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity,
                    UnitName = item.UnitName, ItemId = item.ItemId, MinLevel = item.MinLevel, WRRequestQty = item.WRRequestQty, 
                    CurrentStock = item.CurrentStock, WRIssueQty = item.WRIssueQty, TotalQty = item.TotalQty, InTransitQty = item.InTransitQty, 
                    PendingPRQty = item.PendingPRQty, ShortorExcess = item.ShortorExcess,Remarks=item.Remarks  };
                model.items.Add(pritem);

            }
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PurchaseRequest model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new PurchaseRequestRepository();

           var result1 = new PurchaseRequestRepository().CHECK(model.PurchaseRequestId);
           if (result1>0)
          {
              TempData["error"] = "Sorry!!..Already Used!!";
              TempData["PurchaseRequestNo"] = null;
              return View("Edit", model);
          }

          else
          {
                try
            {
              var result2 = new PurchaseRequestRepository().DeletePurchaseRequestDT(model.PurchaseRequestId);
              var result3 = new PurchaseRequestRepository().DeletePurchaseRequestHD(model.PurchaseRequestId, UserID.ToString());
              //var result = new PurchaseRequestRepository().UpdatePurchaseRequest(model);
                string id = new PurchaseRequestRepository().InsertPurchaseRequest(model);
                  if (id.Split('|')[0] != "0")
                   {
                       TempData["success"] = "Updated successfully. Purchase Request Reference No. is " + id.Split('|')[1];
                       TempData["error"] = "";
                       return RedirectToAction("PendingPurchaseRequest");
                       //return View("Edit", model);
                   }
                   else
                   {
                       throw new Exception();
                   }
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
            return RedirectToAction("Index");
        }
              
          }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new PurchaseRequestRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["PurchaseRequestNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result2 = new PurchaseRequestRepository().DeletePurchaseRequestDT(Id);
                var result3 = new PurchaseRequestRepository().DeletePurchaseRequestHD(Id, UserID.ToString());

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("PendingPurchaseRequest");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["PurchaseRequestNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }
            
        }

        public void FillPRRefNo()
        {
            ViewBag.PRNoList = new SelectList(new DropdownRepository().PRRefNoDropdown(), "Id", "Name");
        }
        public void FillPRCustomer()
        {
            ViewBag.CustomerList = new SelectList(new DropdownRepository().PurchaseReqCustomerDropdown(), "Id", "Name");
        }

        public void FillWRCustomer(int OrganizationId)
        {
            ViewBag.CustomerList = new SelectList(new DropdownRepository().WRCustomerDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PurchaseRequest.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("PurchaseRequestNo");
            ds.Tables["Head"].Columns.Add("PurchaseRequestDate");
            ds.Tables["Head"].Columns.Add("WorkShopRequestId");
            ds.Tables["Head"].Columns.Add("CustomerName");
            //ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");
            ds.Tables["Head"].Columns.Add("RequiredDate");
            ds.Tables["Head"].Columns.Add("CustomerOrderRef");
            ds.Tables["Head"].Columns.Add("WorkShopRequestRefNo");
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
        


            //-------DT
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("UnitName");
            ds.Tables["Items"].Columns.Add("ItemId");
            ds.Tables["Items"].Columns.Add("MinLevel");
            ds.Tables["Items"].Columns.Add("WRRequestQty");
            ds.Tables["Items"].Columns.Add("CurrentStock");
            ds.Tables["Items"].Columns.Add("WRIssueQty");
            ds.Tables["Items"].Columns.Add("TotalQty");
            ds.Tables["Items"].Columns.Add("InTransitQty");
            ds.Tables["Items"].Columns.Add("PendingPRQty");
            ds.Tables["Items"].Columns.Add("ShortorExcess");
            ds.Tables["Items"].Columns.Add("Remarks");
          

            PurchaseRequestRepository repo = new PurchaseRequestRepository();
            var Head = repo.GetPurchaseRequestHDDetailsPrint(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["PurchaseRequestNo"] = Head.PurchaseRequestNo;
            dr["PurchaseRequestDate"] = Head.PurchaseRequestDate.ToString("dd-MMM-yyyy");
            dr["WorkShopRequestId"] = Head.WorkShopRequestId;
            dr["CustomerName"] = Head.CustomerName;
            dr["SpecialRemarks"] = Head.SpecialRemarks;
            dr["RequiredDate"] = Head.RequiredDate.ToString("dd-MMM-yyyy"); ;
            dr["CustomerOrderRef"] = Head.CustomerOrderRef;
            dr["WorkShopRequestRefNo"] = Head.WorkShopRequestRefNo;
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
            ds.Tables["Head"].Rows.Add(dr);

            PurchaseRequestRepository repo1 = new PurchaseRequestRepository();
            var Items = repo1.PurchaseRequestItemDetailsPrint(Id);
            foreach (var item in Items)
            {
                var pritem = new PurchaseRequestItem
                {
                    PartNo = item.PartNo,
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    UnitName = item.UnitName,
                    ItemId = item.ItemId,
                    MinLevel = item.MinLevel,
                    WRRequestQty = item.WRRequestQty,
                    CurrentStock = item.CurrentStock,
                    WRIssueQty = item.WRIssueQty,
                    TotalQty = item.TotalQty,
                    InTransitQty = item.InTransitQty,
                    PendingPRQty = item.PendingPRQty,
                    ShortorExcess = item.ShortorExcess,
                    Remarks = item.Remarks
                };
                

                DataRow dri = ds.Tables["Items"].NewRow();
                dri["PartNo"] = pritem.PartNo;
                dri["ItemName"] = pritem.ItemName;
                dri["Quantity"] = pritem.Quantity;
                dri["UnitName"] = pritem.UnitName;
                dri["ItemId"] = pritem.ItemId;
                dri["MinLevel"] = pritem.MinLevel;
                dri["WRRequestQty"] = pritem.WRRequestQty;
                dri["CurrentStock"] = pritem.CurrentStock;
                dri["WRIssueQty"] = pritem.WRIssueQty;
                dri["TotalQty"] = pritem.TotalQty;
                dri["InTransitQty"] = pritem.InTransitQty;
                dri["PendingPRQty"] = pritem.PendingPRQty;
                dri["ShortorExcess"] = pritem.ShortorExcess;
                dri["Remarks"] = pritem.Remarks;
                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "PurchaseRequest.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", String.Format("PurchaseRequest{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult GetLastPurchaseRate(int itemId)
        {
            try
            {
                var list = new PurchaseRequestRepository().GetLastPurchaseRate(itemId, OrganizationId);
                return PartialView("_LastPurchaseRateGrid", list);
            }
            catch (Exception)
            {
                return Json("Some error occurred while fetching the rates!", JsonRequestBehavior.AllowGet);
            }
        }
    }
}