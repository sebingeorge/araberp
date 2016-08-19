using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class SubAssemblyController : BaseController
    {
        //TODO Check whether entered quantity is greater than stock quantity
        //TODO Swap order of finished and consumed goods
        // GET: SubAssembly
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            FillDropdowns();
            return View(new SubAssembly
            {
                StockCreationDate = DateTime.Today,
                StockCreationRefNo = "SUB/" + DatabaseCommonRepository.GetNextReferenceNo(typeof(SubAssembly).Name),
                isSubAssembly = true,
                ConsumedItems=ConsumedItemsGrid(),
                FinishedGoods=FinishedGoodsGrid()
            });
        }

        [HttpPost]
        public ActionResult Create(SubAssembly model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();

                string ref_no = new SubAssemblyRepository().CreateSubAssembly(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured while saving. Please try again.|" + ex.Message;
                FillEmployee(); FillStockpoint();
                return View("Create", model);
            }
            return RedirectToAction("Create");
        }

        public List<StockCreationFinishedGood> FinishedGoodsGrid()
        {
            var FinishedGoods = new List<StockCreationFinishedGood>();
            var a = new StockCreationFinishedGood();
            FinishedGoods.Add(a);
            return FinishedGoods;
        }

        public List<StockCreationConsumedItem> ConsumedItemsGrid()
        {
            var ConsumedItems = new List<StockCreationConsumedItem>();
            var b = new StockCreationConsumedItem();
            ConsumedItems.Add(b);
            return ConsumedItems;
        }

        public ActionResult GetStockQuantity(string date, int id = 0, int stockpoint = 0)
        {
            try
            {
                if (id == 0 || stockpoint == 0 || date == null) throw new InvalidOperationException();
                ClosingStock stock = new ClosingStockRepository().GetClosingStockData(Convert.ToDateTime(date), stockpoint, 0, id, OrganizationId).First();
                return Json(stock.Quantity, JsonRequestBehavior.AllowGet);
            }
            catch (InvalidOperationException)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PreviousList()
        {//TODO Show stockpoints in previous list
            return PartialView("_PreviousList", new SubAssemblyRepository().GetSubAssemblies(organizationId: OrganizationId));
        }

        public ActionResult Edit(int id = 0)//StockCreationId is received here
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request";
                return RedirectToAction("Index", "Home");
            }
            var model = new SubAssemblyRepository().GetSubAssembly(id, OrganizationId);
            if (model == null)
            {
                TempData["error"] = "Requested item not found!";
                return RedirectToAction("Index", "Home");
            }
            FillDropdowns();
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SubAssembly model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = UserID.ToString();
                string ref_no = new SubAssemblyRepository().UpdateSubAssembly(model);
                TempData["success"] = "Updated Successfully - " + ref_no;
                return RedirectToAction("Index");
            }
            catch (SqlException sx)
            {
                if (sx.Errors[0].Number == 547)
                    TempData["error"] = "Cannot update, because the record is in use elsewhere";
                else
                    TempData["error"] = "Some error occured while connecting to database|" + sx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured while saving. Please try again.|" + ex.Message;
            }
            FillDropdowns();
            return View("Create", model);
        }

        public ActionResult Delete(int id = 0)//StockCreationId is received here
        {
            if (id == 0)
            {
                TempData["error"] = "That was an invalid/unknown request";
                return RedirectToAction("Index", "Home");
            }
            try
            {
                if (new SubAssemblyRepository().DeleteSubAssembly(id, UserID, OrganizationId) > 0)
                {
                    TempData["success"] = "Deleted Successfully";
                    return RedirectToAction("Index");
                }
                else { throw new Exception(); }
            }
            catch (SqlException sx)
            {
                if (sx.Errors[0].Number == 547)
                    TempData["error"] = "Cannot delete, because the record is in use elsewhere.";
                else
                    TempData["error"] = "Some error occured while connecting to database.|" + sx.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "Some error occured. Please try again.|" + ex.Message;
            }
            FillDropdowns();
            return RedirectToAction("Edit", new { id = id });
        }

        #region Dropdowns
        public void FillDropdowns()
        {
            FillEmployee(); FillMaterial(); FillStockpoint();
        }

        public void FillEmployee()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }

        public void FillMaterial()
        {
            ViewBag.materialList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }

        public void FillStockpoint()
        {
            ViewBag.stockpointList = new SelectList(new DropdownRepository().StockpointDropdown(), "Id", "Name");
        }

        #endregion
    }
}