using ArabErp.DAL;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class PurchaseBillController : BaseController
    {
        // GET: PurchaseBill
        public ActionResult Index()
        {
            GrnSupplierDropdown();
            FillCurrency();
            return View();
            
        }
        public PartialViewResult pendingGRN(int supplierId = 0)
        {
            try
            {
                return PartialView("_pendingGRN", new PurchaseBillRepository().GetGRNPending(supplierId));
            }
            catch (Exception)
            {
                return PartialView("_pendingGRN", new List<PendingGRN>());
            }
        }
        public void GrnSupplierDropdown()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().GrnSupplierDropdown(), "Id", "Name");
        }
        public void FillCurrency()
        {
            var repo = new PurchaseBillRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }
        public ActionResult Create(IList<PendingGRN> PendingGRNSelected)

        {
            FillAdditionDeduction();
            FillCurrency();
            PurchaseBill purchasebill = new PurchaseBill();
            PurchaseBillRepository rep = new PurchaseBillRepository();
            if (PendingGRNSelected != null)
            {
                if (PendingGRNSelected.Count > 0)
                {
                    List<int> selectedgrn = (from PendingGRN p in PendingGRNSelected
                                                          where p.Select
                                                          select p.GRNId).ToList<int>();
                    purchasebill.Items = rep.GetGRNItems(selectedgrn);
                }


            }
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(PurchaseBill).Name);

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

            purchasebill.PurchaseBillRefNo = "PRB/" + internalId;
         
            purchasebill.Supplier = PendingGRNSelected[0].SupplierName;
            purchasebill.SupplierId = PendingGRNSelected[0].SupplierId;
            purchasebill.PurchaseBillDate = System.DateTime.Today;
            purchasebill.PurchaseBillDueDate = rep.GetBillDueDate(PendingGRNSelected[0].SupplierId).PurchaseBillDueDate;
            purchasebill.CurrencyId = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId).CurrencyId;
            return View(purchasebill);

        }
        public ActionResult Save(PurchaseBill model)
        {
            try
            {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
      
             string id = new PurchaseBillRepository().InsertPurchaseBill(model);
                   if (id.Split('|')[0] != "0")
                   {
                       TempData["success"] = "Saved successfully. Purchase Bill Reference No. is " + id.Split('|')[1];
                       TempData["error"] = "";
                       return RedirectToAction("Index");
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
        public void FillAdditionDeduction()
        {
            ViewBag.additionList = new SelectList(new DropdownRepository().AdditionDropdown(), "Id", "Name");
            ViewBag.deductionList = new SelectList(new DropdownRepository().DeductionDropdown(), "Id", "Name");
        }

        public void FillBillRefNo()
        {
            ViewBag.BillNoList = new SelectList(new DropdownRepository().PurchaseBillNoDropdown(), "Id", "Name");
        }
        public void FillPBSupplier()
        {
            ViewBag.SupplierList = new SelectList(new DropdownRepository().PurchaseBillSupplierDropdown(), "Id", "Name");
        }

        public ActionResult PurchaseBillList()
        {

            FillBillRefNo();
            FillPBSupplier();
            return View();
           
        }

        public ActionResult PurchaseBillListDatas(DateTime? from, DateTime? to, int id = 0, int supid = 0)
        {
            return PartialView("_PurchaseBillListDatas", new PurchaseBillRepository().GetPurchaseBillPreviousList(id: id, supid: supid, from: from, to: to, OrganizationId: OrganizationId));

            //var repo = new PurchaseBillRepository();
            //IEnumerable<PurchaseBill> PurchaseBillList = repo.GetPurchaseBillPreviousList();
            //return View("PurchaseBillList", PurchaseBillList);

        }

        [HttpGet]
        public JsonResult GetDueDate(DateTime date,int supplierId)
        {
            //var res = (new PurchaseBillRepository()).GetCurrencyIdByCustKey(cusKey);
            DateTime duedate = (new PurchaseBillRepository()).GetDueDate(date, supplierId);
            //var Result = new { VehicleId = List.VehicleModelId, VehicleName = List.VehicleModelName };
            //return Json(Result, JsonRequestBehavior.AllowGet);

            return Json(duedate.ToString("dd/MMMM/yyyy"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    PurchaseBill PurchaseBill = new PurchaseBill();
                    PurchaseBill = new PurchaseBillRepository().GetPurchaseBill(id);
                    PurchaseBill.PurchaseBillAmount *= (decimal)1.00;
                    PurchaseBill.Items = new PurchaseBillItemRepository().GetPurchaseBillItem(id);
                    FillAdditionDeduction();
                    FillCurrency();
                    return View(PurchaseBill);
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

        [HttpPost]
        public ActionResult Edit(PurchaseBill model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillAdditionDeduction();
            FillCurrency();

            var repo = new PurchaseBillRepository();

            //var result1 = new PurchaseBillRepository().CHECK(model.SupplyOrderId);
            //if (result1 > 0)
            //{
            //    TempData["error"] = "Sorry!!..Already Used!!";
            //    TempData["PurchaseRequestNo"] = null;
            //    return View("Edit", model);
            //}

            //else
            {
                try
                {
                    var result2 = new PurchaseBillRepository().DeletePuchaseBillDT(model.PurchaseBillId);
                    var result3 = new PurchaseBillRepository().DeletePuchaseBillHD(model.PurchaseBillId, UserID.ToString());
                    string id = new PurchaseBillRepository().InsertPurchaseBill(model);

                    TempData["success"] = "Updated successfully. Purchase Request Reference No. is " + model.PurchaseBillRefNo;
                    TempData["error"] = "";
                    return RedirectToAction("PurchaseBillList");
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
                return RedirectToAction("PurchaseBillList");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            //var result1 = new SupplyOrderRepository().CHECK(Id);
            //if (result1 > 0)
            //{
            //    TempData["error"] = "Sorry!!..Already Used!!";
            //    TempData["SupplyOrderNo"] = null;
            //    return RedirectToAction("Edit", new { id = Id });
            //}

            //else
            //{
                var result2 = new PurchaseBillRepository().DeletePuchaseBillDT(Id);
                var result3 = new PurchaseBillRepository().DeletePuchaseBillHD(Id, UserID.ToString());

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("PurchaseBillList");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["SupplyOrderNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }

    }
//}