using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class DirectPurchaseController : BaseController
    {
        // GET: LocalPurchase
        public ActionResult Index()
        {
            return View(new DirectPurchaseRepository().GetPreviousList(OrganizationId));
        }
        public ActionResult CreateRequest()
        {
            FillSO();
            FillJC();
            GetMaterials();
            FillPartNo();
            List<DirectPurchaseRequestItem> list = new List<DirectPurchaseRequestItem>();
            list.Add(new DirectPurchaseRequestItem());
            return View("Create", new DirectPurchaseRequest
            {
                items = list,
                PurchaseRequestDate = DateTime.Today,
                RequiredDate = DateTime.Today,
                PurchaseRequestNo = DatabaseCommonRepository.GetNextDocNo(10, OrganizationId)
            });
        }
        [HttpPost]
        public ActionResult CreateRequest(DirectPurchaseRequest model)
        {
            if (!ModelState.IsValid)
            {
                FillSO();
                FillJC();
                GetMaterials();
                return View("Create", model);
            }

            FillSO();
            FillJC();
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            string referenceNo = new DirectPurchaseRepository().InsertDirectPurchaseRequest(model);
            if (referenceNo != "")

            //if (new DirectPurchaseRepository().InsertDirectPurchaseRequest(model) > 0)
            {
                TempData["success"] = "Saved successfully. Reference No. is " + referenceNo;
                TempData["error"] = "";
                return RedirectToAction("CreateRequest");
            }
            else
            {
                GetMaterials();
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return View("Create", model);
            }
        }
        public void GetMaterials()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public JsonResult GetPartNoUnit(int itemId)
        {
            return Json(new ItemRepository().GetPartNoUnit(itemId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPurchaseLimit()
        {
            string str;
            try
            {
                int organizationId = OrganizationId;
                str = new DirectPurchaseRepository().GetPurchaseLimit(organizationId);
            }
            catch (Exception)
            {
                str = "error";
            }
            return Json(str, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ValidateForm(string requestNo, int total = 0)
        {
            int val1 = new DirectPurchaseRepository().isNotExist(requestNo);
            int val2 = new DirectPurchaseRepository().validateTotal(total);
            string str = "";
            if (val1 != 1)
                str += "Purchase request number already exists";
            if (val2 != 1)
                str += "|Total amount should not exceed the purchase limit";
            if (str.Length == 0)
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            return Json(new { status = false, message = str }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Approval()
        {
            return View(new DirectPurchaseRepository().GetUnApprovedRequests());
        }
        public JsonResult Approve(int id)
        {
            try
            {
                TempData["success"] = "Approved successfully";
                TempData["error"] = "";
                new DirectPurchaseRepository().ApproveRequest(id);
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (SqlException)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.";
                return Json("error|Some error occured while connecting to database. Please check your network connection and try again.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.";
                return Json("error|" + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public void FillSO()
        {
            List<Dropdown> list = new DropdownRepository().SaleOrderDropdown2(OrganizationId);
            ViewBag.SO = new SelectList(list, "Id", "Name");
        }
        public void FillJC()
        {
            ExpenseRepository repo = new ExpenseRepository();
            List<Dropdown> list = new DropdownRepository().JobCardDropdown(OrganizationId);
            ViewBag.JC = new SelectList(list, "Id", "Name");
        }
        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    FillSO();
                    FillJC();
                    GetMaterials();

                    DirectPurchaseRequest DirectPurchaseRequest = new DirectPurchaseRequest();
                    DirectPurchaseRequest = new DirectPurchaseRepository().GetDirectPurchaseRequest(id);
                    DirectPurchaseRequest.items = new DirectPurchaseRepository().GetDirectPurchaseRequestItems(id);

                    return View(DirectPurchaseRequest);
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
            return RedirectToAction("CreateRequest");
        }

        [HttpPost]
        public ActionResult Edit(DirectPurchaseRequest model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillSO();
            FillJC();
            GetMaterials();

            var repo = new DirectPurchaseRepository();

            var result1 = new DirectPurchaseRepository().CHECK(model.DirectPurchaseRequestId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["PurchaseRequestNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result2 = new DirectPurchaseRepository().DeleteDirectPurchaseDT(model.DirectPurchaseRequestId);
                    var result3 = new DirectPurchaseRepository().DeleteDirectPurchaseHD(model.DirectPurchaseRequestId, UserID.ToString());
                    string id = new DirectPurchaseRepository().InsertDirectPurchaseRequest(model);

                    TempData["success"] = "Updated successfully. Direct Purchase Request Reference No. is " + id;
                    TempData["error"] = "";
                    return RedirectToAction("Index");
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
                return RedirectToAction("CreateRequest");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new DirectPurchaseRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["PurchaseRequestNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result2 = new DirectPurchaseRepository().DeleteDirectPurchaseDT(Id);
                var result3 = new DirectPurchaseRepository().DeleteDirectPurchaseHD(Id, UserID.ToString());

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("Index");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SupplyOrderNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }

        public ActionResult PurchaseIndent()
        {
            FillPartNo();
            GetMaterials(); 
            List<DirectPurchaseRequestItem> list = new List<DirectPurchaseRequestItem>();
            list.Add(new DirectPurchaseRequestItem());
            return View(new DirectPurchaseRequest
            {
                items = list,
                PurchaseRequestDate = DateTime.Today,
                RequiredDate = DateTime.Today,
                PurchaseRequestNo = DatabaseCommonRepository.GetNextDocNo(36, OrganizationId)
            });
        }

        [HttpPost]
        public ActionResult PurchaseIndent(DirectPurchaseRequest model)
        {
            try
            {
                model.CreatedBy = UserID.ToString();
                model.OrganizationId = OrganizationId;
                model.CreatedDate = DateTime.Today;
                string ref_no = new DirectPurchaseRepository().InsertPurchaseIndent(model);
                if (ref_no.Length > 0)
                {
                    TempData["success"] = "Saved Successfully. Reference No. is " + model.PurchaseRequestNo;
                    return RedirectToAction("PurchaseIndent");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred while saving. Please try again.";
                return View(model);
            }
        }

        public void FillPartNo()
        {
            ViewBag.partNoList = new SelectList(new DropdownRepository().PartNoDropdown1(), "Id", "Name");
        }

        public ActionResult PurchaseIndents()
        {
            return View(new DirectPurchaseRepository().GetPurchaseIndentList(OrganizationId));
        }

        public ActionResult EditPurchaseIndent(int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    return RedirectToAction("Index", "Home");
                }
                var model = new DirectPurchaseRepository().GetPurchaseIndent(id, OrganizationId);
                if (model == null)
                {
                    TempData["error"] = "Could not find the requested Purchase Indent. Please try again.";
                    return RedirectToAction("Index", "Home");
                }
                FillPartNo();
                GetMaterials();
                return View("PurchaseIndent", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                return RedirectToAction("PurchaseIndents");
            }
        }

        [HttpPost]
        public ActionResult EditPurchaseIndent(DirectPurchaseRequest model)
        {
            try
            {
                model.CreatedBy = UserID.ToString();
                model.CreatedDate = DateTime.Today;
                var success = new DirectPurchaseRepository().UpdatePurchaseIndent(model);
                if (success <= 0) throw new Exception();
                TempData["success"] = "Updated successfully (" + model.PurchaseRequestNo + ")";
                return RedirectToAction("PurchaseIndents");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while saving. Please try again.";
                FillPartNo();
                GetMaterials();
                return View("PurchaseIndent", model);
            }
        }

        public ActionResult DeletePurchaseIndent(int id = 0)
        {
            try
            {
                if (id == 0) return RedirectToAction("Index", "Home");
                string result = new DirectPurchaseRepository().DeletePurchaseIndent(id);
                TempData["Success"] = "Deleted Successfully!";
                return RedirectToAction("PurchaseIndents");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("PurchaseIndent", new { id = id });
            }
        }
        public JsonResult GetStockQuantity(int itemId)
        {
            return Json(new DirectPurchaseRepository().GetStockQuantity(itemId), JsonRequestBehavior.AllowGet);
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