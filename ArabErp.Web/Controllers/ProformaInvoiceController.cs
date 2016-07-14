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
        public ActionResult PendingProforma(int? isProjectBased)
        {
            var repo = new ProformaInvoiceRepository();
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForPerforma(isProjectBased ?? 0);
            return View(pendingSO);
        }
        public ActionResult Create(int? SaleOrderId)
        {
            ProformaInvoiceRepository repo = new ProformaInvoiceRepository();
            CurrencyRepository repo1 = new CurrencyRepository();
            ProformaInvoice model = repo.GetSaleOrderForPorforma(SaleOrderId ?? 0);
            model.SymbolName = repo1.GetCurrencyFrmOrganization(OrganizationId).SymbolName;
            var PIList = repo.GetPorformaInvoiceData(model.SaleOrderId);
            model.Items = new List<ProformaInvoiceItem>();
            foreach (var item in PIList)
            {
                model.Items.Add(new ProformaInvoiceItem { WorkDescription  = item.WorkDescription, VehicleModelName = item.VehicleModelName, Quantity = item.Quantity, UnitName = item.UnitName,Rate=item.Rate,Amount=item.Amount,Discount=item.Discount });

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

            model.ProformaInvoiceRefNo = "PINV/" + internalId;
            model.ProformaInvoiceDate = DateTime.Now;
           
            return View(model);
        }
          [HttpPost]
        public ActionResult Create(ProformaInvoice model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = System.DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                string id = new ProformaInvoiceRepository().InsertProformaInvoice(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Proforma Invoice No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("PendingProforma", new { ProjectBased = 0 });
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
            TempData["success"] = "";
           

            return View(model);
        }

    }
}