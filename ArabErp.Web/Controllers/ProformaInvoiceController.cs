using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;

namespace ArabErp.Web.Controllers
{
    public class ProformaInvoiceController : BaseController
    {
        // GET: ProformaInvoice
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PendingProforma(int? page, int? isProjectBased)
        {
            var repo = new SaleOrderRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForHold(isProjectBased ?? 0);
            return View(pendingSO);
        }
        public ActionResult Create(int? SaleOrderId)
        {
            ProformaInvoiceRepository repo = new ProformaInvoiceRepository();

            ProformaInvoice model = repo.GetSaleOrderForPorforma(SaleOrderId ?? 0);

            var PIList = repo.GetPorformaInvoiceData(model.SaleOrderId);
            model.Items = new List<ProformaInvoiceItem>();
            foreach (var item in PIList)
            {
                model.Items.Add(new ProformaInvoiceItem { PartNo = item.PartNo, ItemName = item.ItemName, Quantity = item.Quantity, UnitName = item.UnitName, ItemId = item.ItemId, ActualQuantity = item.Quantity });

            }

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(ProformaInvoice).Name);

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

            model.PorformaInvoiceNo = "PINV/" + internalId;
            model.PorformaInvoiceDate = System.DateTime.Today;
           
            return View(model);
        }
    }
}