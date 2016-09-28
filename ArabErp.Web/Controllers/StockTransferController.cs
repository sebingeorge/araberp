using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class StockTransferController : BaseController
    {
        // GET: StockTransfer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PreviousList()
        {
            return PartialView("_PreviousList", new StockTransferRepository().PreviousList(OrganizationId: OrganizationId));
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

        public ActionResult Create()
        {
            FillDropdowns();
            return View(new StockTransfer
            {
                StockTransferDate = DateTime.Today,
                StockTransferRefNo = DatabaseCommonRepository.GetNextDocNo(29, OrganizationId),
                Items = TransferItemsGrid()
            });
        }

        [HttpPost]
        public ActionResult Create(StockTransfer model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                string ref_no = new StockTransferRepository().CreateStockTransfer(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try again.";
                FillDropdowns();
                return View(model);
            }
            return RedirectToAction("Create");
        }

        public List<StockTransferItem> TransferItemsGrid()
        {
            var TransferItems = new List<StockTransferItem>();
            var b = new StockTransferItem();
            TransferItems.Add(b);
            return TransferItems;
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    FillDropdowns();
                    StockTransfer StockTransfer = new StockTransfer();
                    StockTransfer = new StockTransferRepository().GetStockTransferHD(id);
                    StockTransfer.Items = new StockTransferRepository().GetStockTransferDT(id);

                    return View(StockTransfer);
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

        #region Dropdowns
        public void FillDropdowns()
        {
            FillMaterial(); FillStockpoint();
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