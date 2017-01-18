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
//using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class WorkShopRequestController : BaseController
    {
        // GET: WorkShopRequest
        public ActionResult Index(int isProjectBased)
        {
            FillWRNo();
            FillCustomerinWR();
            ViewBag.ProjectBased = isProjectBased;
            return View();
        }
        [HttpGet]
        public ActionResult Create(int? SaleOrderId, int? saleorderitem, int? SaleOrderItemUnitId, int? EvaConUnitId)
        {
            ItemDropdown();
            WorkShopRequestRepository repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetSaleOrderForWorkshopRequest(SaleOrderId ?? 0);
            model.SaleOrderItemId = saleorderitem ?? 0;
            model.WorkDescription = repo.GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(SaleOrderId ?? 0).WorkDescription;
            List<WorkShopRequestItem> WSList = new List<WorkShopRequestItem>();
            if(model.isProjectBased==1)
            {
                WSList = repo.GetWorkShopRequestDataForProject(SaleOrderItemUnitId ?? 0, EvaConUnitId ?? 0);
            }
            else
            {
               WSList = repo.GetWorkShopRequestData(SaleOrderId ?? 0, saleorderitem ?? 0);
            }
          
            model.Items = new List<WorkShopRequestItem>();
            //model.Isused = true;
            foreach (var item in WSList)
            {
                model.Items.Add(new WorkShopRequestItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, ActualQuantity = item.Quantity });

            }
            string internalId = "";
            try
            {
                if (model.isProjectBased == 0)
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(19, OrganizationId);
                }
                else
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(31, OrganizationId);
                }
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
            model.WorkShopRequestRefNo = internalId;
            model.WorkShopRequestDate = System.DateTime.Today;
            model.RequiredDate = System.DateTime.Today;
            model.SaleOrderItemUnitId = SaleOrderItemUnitId ?? 0;
            model.EvaConUnitId = EvaConUnitId ?? 0;
            return View(model);
        }

        public ActionResult CreateDirectMaterialRequest()
        {
            FillPartNo();
            GetMaterials();
            List<WorkShopRequestItem> list = new List<WorkShopRequestItem>();
            list.Add(new WorkShopRequestItem());
            return View(new WorkShopRequest
            {
                Items = list,
                WorkShopRequestDate = DateTime.Today,
                WorkShopRequestRefNo = DatabaseCommonRepository.GetNextDocNo(37, OrganizationId)
            });
            }
        [HttpPost]
        public ActionResult CreateDirectMaterialRequest(WorkShopRequest model)
        {
            try
            {
                model.CreatedBy = UserID.ToString();
                model.OrganizationId = OrganizationId;
                model.CreatedDate = DateTime.Today;
                string ref_no = new WorkShopRequestRepository().InsertDirectMaterialRequest(model);
                if (ref_no.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + model.WorkShopRequestRefNo;
                    return RedirectToAction("CreateDirectMaterialRequest");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View(model);
            }
        }
        public ActionResult EditDirectMaterialRequest(int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                var model = new WorkShopRequestRepository().GetDirectMaterialRequest(id, OrganizationId);
                if (model == null)
                {
                    TempData["error"] = "Could not find the requested Purchase Indent. Please try again.";
                    return RedirectToAction("Index", "Home");
                }
                FillPartNo();
                GetMaterials();
                return View("CreateDirectMaterialRequest", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                return RedirectToAction("DirectMaterialRequestList");
            }
        }

        [HttpPost]
        public ActionResult EditDirectMaterialRequest(WorkShopRequest model)
        {
            try
            {
                model.CreatedBy = UserID.ToString();
                model.CreatedDate = DateTime.Today;
                var success = new WorkShopRequestRepository().UpdateDirectMaterialRequest(model);
                if (success <= 0) throw new Exception();
                TempData["success"] = "Updated successfully (" + model.WorkShopRequestRefNo + ")";
                return RedirectToAction("DirectMaterialRequestList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while saving. Please try again.";
                FillPartNo();
                GetMaterials();
                return View("CreateDirectMaterialRequest", model);
            }
        }
        public ActionResult DeleteDirectMaterialRequest(int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string result = new WorkShopRequestRepository().DeleteWorkShopRequest(id);
                TempData["Success"] = "Deleted Successfully!";
                return RedirectToAction("DirectMaterialRequestList");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("CreateDirectMaterialRequest", new { id = id });
            }
        }
        public ActionResult DirectMaterialRequestList()
        {
            return View(new WorkShopRequestRepository().DirectMaterialRequestList(OrganizationId));
          
        }
        public void FillPartNo()
        {
            ViewBag.partNoList = new SelectList(new DropdownRepository().PartNoDropdown1(), "Id", "Name");
        }
        public void GetMaterials()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public ActionResult Pending(int isProjectBased)
        {
            ViewBag.ProjectBased = isProjectBased;
            return View();
        }
        public ActionResult WorkShopRequestPending(int isProjectBased, string saleOrder = "")
        {

            var rep = new SaleOrderRepository();


            var slist = rep.GetSaleOrdersPendingWorkshopRequest(OrganizationId, isProjectBased, saleOrder.Trim());

            ViewBag.ProjectBased = isProjectBased;

            return PartialView("_PendingGrid", slist);
        }
        [HttpPost]
        public ActionResult Save(WorkShopRequest model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                string id = new WorkShopRequestRepository().InsertWorkShopRequest(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. WorkShop Request Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("Pending", new { model.isProjectBased });
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
            return View("Pending", model);
        }
        public ActionResult PreviousList(int isProjectBased, DateTime? from, DateTime? to, string workshop = "", string customer = "")
        {
            from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousList", new WorkShopRequestRepository().GetPrevious(isProjectBased, from, to, workshop, customer, OrganizationId));
        }
        public ActionResult Edit(int? id)
        {
            ItemDropdown();
            var repo = new WorkShopRequestRepository();
            WorkShopRequest model = repo.GetWorkshopRequestHdData(id ?? 0);
            model.Items = repo.GetWorkShopRequestDtData(id ?? 0);

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(WorkShopRequest model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();



            var repo = new WorkShopRequestRepository();
            try
            {
                new WorkShopRequestRepository().UpdateWorkShopRequest(model);
                TempData["success"] = "Updated Successfully (" + model.WorkShopRequestRefNo + ")";
                return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("Index", new { isProjectBased = model.isProjectBased });
        }

        public ActionResult Delete(int id = 0, int isProjectBased = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string ref_no = new WorkShopRequestRepository().DeleteWorkShopRequest(id);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index", new { isProjectBased = isProjectBased });
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Edit", new { id = id });
            }
        }

        public void FillWRNo()
        {
            ViewBag.WRNoList = new SelectList(new DropdownRepository().WRNODropdown(OrganizationId), "Id", "Name");
        }
        public void FillCustomerinWR()
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().WRCustomerDropdown(OrganizationId), "Id", "Name");
        }

        private void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public JsonResult GetItemUnit(int itemId)
        {
            return Json(new StockReturnItemRepository().GetItemUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemPartNo(int itemId)
        {
            return Json(new WorkShopRequestRepository().GetItemPartNo(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPartNoUnit(int itemId)
        {
            return Json(new ItemRepository().GetPartNoUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Print(int Id)
        {

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "WorkShopRequest.rpt"));

            DataSet ds = new DataSet();
            ds.Tables.Add("Head");

            ds.Tables.Add("Items");

            //-------HEAD
            ds.Tables["Head"].Columns.Add("WorkShopRequestRefNo");
            ds.Tables["Head"].Columns.Add("WorkShopRequestDate");
            ds.Tables["Head"].Columns.Add("CustomerName");
            ds.Tables["Head"].Columns.Add("CustomerOrderRef");
            ds.Tables["Head"].Columns.Add("SaleOrderRefNo");
            ds.Tables["Head"].Columns.Add("EDateArrival");
            ds.Tables["Head"].Columns.Add("EDateDelivery");
            ds.Tables["Head"].Columns.Add("WorkDescription");
            ds.Tables["Head"].Columns.Add("SpecialRemarks");
            ds.Tables["Head"].Columns.Add("RequiredDate");
            ds.Tables["Head"].Columns.Add("DoorNo");
            ds.Tables["Head"].Columns.Add("Street");
            ds.Tables["Head"].Columns.Add("State");
            ds.Tables["Head"].Columns.Add("CountryName");
            ds.Tables["Head"].Columns.Add("Zip");
            ds.Tables["Head"].Columns.Add("Fax");
            ds.Tables["Head"].Columns.Add("Email");
            ds.Tables["Head"].Columns.Add("Phone");
            ds.Tables["Head"].Columns.Add("OrganizationName");
            ds.Tables["Head"].Columns.Add("Image1");
            ds.Tables["Head"].Columns.Add("Title");

            //-------DT
            ds.Tables["Items"].Columns.Add("ItemName");
            ds.Tables["Items"].Columns.Add("PartNo");
            ds.Tables["Items"].Columns.Add("Remarks");
            ds.Tables["Items"].Columns.Add("Quantity");
            ds.Tables["Items"].Columns.Add("UnitName");

            WorkShopRequestRepository repo = new WorkShopRequestRepository();
            var Head = repo.GetWorkshopRequestHdDataPrint(Id, OrganizationId);

            DataRow dr = ds.Tables["Head"].NewRow();
            dr["WorkShopRequestRefNo"] = Head.WorkShopRequestRefNo;
            dr["WorkShopRequestDate"] = Head.WorkShopRequestDate.ToString("dd-MMM-yyyy");
            dr["CustomerName"] = Head.CustomerName;
            dr["CustomerOrderRef"] = Head.CustomerOrderRef;
            dr["SaleOrderRefNo"] = Head.SaleOrderRefNo;
            dr["EDateArrival"] = Head.EDateArrival.ToString("dd-MMM-yyyy");
            dr["EDateDelivery"] = Head.EDateDelivery.ToString("dd-MMM-yyyy");
            dr["WorkDescription"] = Head.WorkDescription;
            dr["SpecialRemarks"] = Head.SpecialRemarks;
            dr["Title"] = Head.title;
            dr["RequiredDate"] = Head.RequiredDate.ToString("dd-MMM-yyyy");
            dr["DoorNo"] = Head.DoorNo;
            dr["Street"] = Head.Street;
            dr["State"] = Head.State;
            dr["CountryName"] = Head.CountryName;
            dr["Zip"] = Head.Zip;
            dr["Fax"] = Head.Fax;
            dr["Email"] = Head.Email;
            dr["Phone"] = Head.Phone;
            dr["OrganizationName"] = Head.OrganizationName;
            dr["Image1"] = Server.MapPath("~/App_images/") + Head.Image1;
            ds.Tables["Head"].Rows.Add(dr);

            WorkShopRequestRepository repo1 = new WorkShopRequestRepository();
            var Items = repo1.GetWorkShopRequestDtDataPrint(Id);
            foreach (var item in Items)
            {
                var pritem = new WorkShopRequestItem
                {
                    ItemName = item.ItemName,
                    PartNo = item.PartNo,
                    Remarks = item.Remarks,
                    Quantity = item.Quantity,
                    UnitName = item.UnitName,


                };


                DataRow dri = ds.Tables["Items"].NewRow();
                dri["ItemName"] = pritem.ItemName;
                dri["PartNo"] = pritem.PartNo;
                dri["Remarks"] = pritem.Remarks;
                dri["Quantity"] = pritem.Quantity;
                dri["UnitName"] = pritem.UnitName;


                ds.Tables["Items"].Rows.Add(dri);
            }

            ds.WriteXml(Path.Combine(Server.MapPath("~/XML"), "WorkShopRequest.xml"), XmlWriteMode.WriteSchema);

            rd.SetDataSource(ds);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf");//, String.Format("WorkShopRequest{0}.pdf", Id.ToString()));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult PendingMaterialRequestApproval()
        {
            return View(new WorkShopRequestRepository().PendingDirectMaterialRequestforApproval(OrganizationId));
        }
        //[HttpGet]
        public ActionResult MaterialRequestApproval(int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                var model = new WorkShopRequestRepository().GetDirectMaterialRequest(id, OrganizationId);
                if (model == null)
                {
                    TempData["error"] = "Could not find the requested Purchase Indent. Please try again.";
                    return RedirectToAction("Index", "Home");
                }
                FillPartNo();
                GetMaterials();
                return View("CreateDirectMaterialRequest", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                return RedirectToAction("DirectMaterialRequestList");
            }
        }
        
        public ActionResult ApproveMaterialReq(int id = 0)
        {
          
            try
            {
                new WorkShopRequestRepository().ApproveMaterialRequest(id);
                TempData["Success"] = "Approved Successfully";
                return RedirectToAction("PendingMaterialRequestApproval");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }

            return RedirectToAction("PendingMaterialRequestApproval");
        }
    }
}