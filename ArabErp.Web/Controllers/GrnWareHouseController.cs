using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class GrnWareHouseController : BaseController
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: GrnWareHouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingGrnWareHouse()
        {
            SupplierDropdown1();
            //var repo = new GRNRepository();

            //IEnumerable<PendingSupplyOrder> pendingSO = repo.GetGRNPendingList();
            //return View(pendingSO);
            return View();
        }

        public ActionResult PreviousList()
        {
            var repo = new GRNRepository();
            IEnumerable<GRN> GRNList = repo.GetGRNPreviousList();
            return View("PreviousList", GRNList);
        }

        public ActionResult Create(IList<PendingForGRN> list)
        {
            try
            {
                GRN model = new GRN();
                if (list.Count > 0)
                {
                    List<int?> id;

                    if (list[0].isDirectPurchase) //if direct purchase
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.DirectPurchaseRequestId).ToList();
                        if (id.Count > 0)
                        {
                            List<GRNAddition> Additions = new List<GRNAddition>();
                            Additions.Add(new GRNAddition());

                            List<GRNDeduction> Deductions = new List<GRNDeduction>();
                            Deductions.Add(new GRNDeduction());

                            model.isDirectPurchaseGRN = true;
                            model.Items = new GRNRepository().GetDirectGRNItems(id);
                            model.GRNDate = DateTime.Today;
                            model.Additions = Additions;
                            model.Deductions = Deductions;
                            SupplierDropdown();
                        }
                        else
                        {
                            throw new NullReferenceException("1No purchase requests were selected. Please select atleast one purchase request and try again.");
                        }
                    }
                    else //if not direct purchase
                    {
                        id = (from PendingForGRN p in list
                              where p.isChecked
                              select p.SupplyOrderId).ToList();
                        if (id.Count > 0)
                        {
                            List<GRNAddition> Additions = new List<GRNAddition>();
                            Additions.Add(new GRNAddition());

                            List<GRNDeduction> Deductions = new List<GRNDeduction>();
                            Deductions.Add(new GRNDeduction());

                            model = new GRNRepository().GetGRNDetails(list[0].SupplierId);
                            model.GRNDate = DateTime.Today;
                            model.Items = new GRNRepository().GetGRNItem(id);
                            model.Additions = Additions;
                            model.Deductions = Deductions;
                        }
                        else
                        {
                            throw new NullReferenceException("1No supply orders were selected. Please select atleast one supply order and try again.");
                        }
                    }
                }
                FillWarehouse();
                FillCurrency();
                FillAdditionDeduction();
                FillEmployee();

                model.GRNNo = DatabaseCommonRepository.GetNextDocNo(11, OrganizationId);

                return View(model);
            }
            catch (NullReferenceException nx)
            {
                if (nx.Message.StartsWith("1"))
                    TempData["error"] = nx.Message.Substring(1);
                else TempData["error"] = "Some required data was missing. Please try again.|" + nx.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            TempData["success"] = "";

            try
            {
                if (list[0].isDirectPurchase)
                    return RedirectToAction("PendingDirectPurchase");
                return RedirectToAction("PendingGrnWareHouse");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public void FillWarehouse()
        {
            GRNRepository repo = new GRNRepository();
            var result = repo.GetWarehouseList();
            ViewBag.WarehouseList = new SelectList(result, "StockPointId", "StockPointName");
        }

        public void FillCurrency()
        {
            var repo = new GRNRepository();
            var list = repo.FillCurrency();
            ViewBag.currlist = new SelectList(list, "Id", "Name");
        }

        public ActionResult Save(GRN model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return View(model);
                //}

                if (model == null || model.Items == null || model.Items.Count == 0) throw new NullReferenceException();
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string result = new GRNRepository().InsertGRN(model);
                if (result != "")
                {
                    TempData["success"] = "Saved succesfully. The GRN Reference No. is " + result;
                    TempData["error"] = "";
                    if (model.isDirectPurchaseGRN)
                        return RedirectToAction("PendingDirectPurchase");
                    return RedirectToAction("PendingGrnWareHouse");
                }
                else
                    throw new Exception();
            }
            catch (NullReferenceException nx)
            {
                TempData["success"] = "";
                if (nx.Message.StartsWith("1"))
                    TempData["error"] = nx.Message.Substring(1);
                else TempData["error"] = "Some required value was missing. Please try again.|" + nx.Message;
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            FillWarehouse();
            FillCurrency();
            SupplierDropdown();
            FillAdditionDeduction();
            FillEmployee();

            PendingForGRN pending = new PendingForGRN();
            if (model.isDirectPurchaseGRN)
                foreach (var item in model.Items)
                    pending.DirectPurchaseRequestId = item.DirectPurchaseRequestId;
            else
                foreach (var item in model.Items)
                    pending.SupplyOrderId = item.SupplyOrderId;

            return View("Create", model);
        }

        public ActionResult Edit(int id = 0)
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request.";
                return RedirectToAction("Index", "Home");
            }
            GRNRepository repo = new GRNRepository();
            FillWarehouse();
            FillCurrency();
            FillEmployee();
            FillAdditionDeduction();

            GRN model = repo.GetGRNDetails(id);
            model.Items = repo.GetGRNItems(id);
            model.Additions = repo.GetGRNAdditions(id);
            model.Deductions = repo.GetGRNDeductions(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(GRN model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            FillWarehouse();
            FillCurrency();
            SupplierDropdown();
            FillAdditionDeduction();
            FillEmployee();

            var repo = new GRNRepository();
            var result1 = new GRNRepository().CHECK(model.GRNId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["GRNNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result = new GRNRepository().UpdateGRN(model);
                    var result3 = new GRNItemRepository().DeleteGRNADDDED(model.GRNId);
                    var result2 = new GRNItemRepository().DeleteGRNItem(model.GRNId);
                    var result4 = new StockUpdateRepository().DeleteGRNStockUpdate(model.GRNId);
                    var result5 = new GRNRepository().InsertGRNDT(model);
                    if (result5.GRNId > 0)
                    {
                        TempData["success"] = "Updated successfully. The GRN Reference No. is " + result.GRNNo;
                        TempData["GRNNo"] = result.GRNNo;
                        return RedirectToAction("PreviousList");
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
                return RedirectToAction("PreviousList");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new GRNRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["GRNNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result5 = new GRNItemRepository().DeleteGRNADDDED(Id);
                var result2 = new GRNItemRepository().DeleteGRNItem(Id);
                var result4 = new StockUpdateRepository().DeleteGRNStockUpdate(Id);
                var result3 = new GRNRepository().DeleteGRNHD(Id);

                if (Id > 0)
                {
                    TempData["success"] = "Deleted succesfully";
                    return RedirectToAction("PreviousList");

                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["GRNNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

        }
        public ActionResult PendingDirectPurchase()
        {
            return View();
        }

        public PartialViewResult PendingGrid(int supplierId = 0)
        {
            try
            {
                return PartialView("_PendingGrid", new GRNRepository().GetGRNPendingList(supplierId));
            }
            catch (Exception)
            {
                return PartialView("_PendingGrid", new List<PendingForGRN>());
            }
        }

        public ActionResult PendingDirectPurchaseGrid()
        {
            try
            {
                return PartialView("_PendingDirectPurchase", new GRNRepository().GetPendingDirectPurchase());
            }
            catch (Exception)
            {
                return PartialView("_PendingDirectPurchase", new List<PendingForGRN>());
            }
        }

        public JsonResult GetSupplierCurrency(int id) //SupplierId is received here
        {
            try
            {
                Supplier model = new SupplierRepository().GetSupplierCurrency(id);
                return Json(new { currencyId = model.CurrencyId, currencyName = model.CurrencyName }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { currencyId = "", currencyName = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// All active suppliers
        /// </summary>
        public void SupplierDropdown()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().SupplierDropdown(), "Id", "Name");
        }

        /// <summary>
        /// Suppliers who have pending sale orders
        /// </summary>
        public void SupplierDropdown1()
        {
            ViewBag.supplierList = new SelectList(new DropdownRepository().SupplierDropdown1(), "Id", "Name");
        }

        public void FillAdditionDeduction()
        {
            ViewBag.additionList = new SelectList(new DropdownRepository().AdditionDropdown(), "Id", "Name");
            ViewBag.deductionList = new SelectList(new DropdownRepository().DeductionDropdown(), "Id", "Name");
        }

        public void FillEmployee()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
    }
}