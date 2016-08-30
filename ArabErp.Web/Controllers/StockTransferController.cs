using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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