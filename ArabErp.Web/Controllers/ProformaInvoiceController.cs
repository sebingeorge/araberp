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
        public ActionResult Index(int type)
        {
            FillProformaInvoice(type);
            FillCustomer(type);
            ViewBag.type = type;
            return View();
        }

        public ActionResult PreviousList(int type, DateTime? from, DateTime? to, int customer = 0, int id = 0)
        {
            return PartialView("_PreviousListGrid", new ProformaInvoiceRepository().PreviousList(OrganizationId: OrganizationId, type: type, from: from, to: to, customer: customer, id: id));
        }
        public ActionResult PendingProforma(int? ProjectBased)
        {
            var repo = new ProformaInvoiceRepository();
            ViewBag.ProjectBased = ProjectBased;
            IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForPerforma(ProjectBased ?? 0);
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
                model.Items.Add(new ProformaInvoiceItem { WorkDescription = item.WorkDescription, VehicleModelName = item.VehicleModelName, Quantity = item.Quantity, UnitName = item.UnitName, Rate = item.Rate, Amount = item.Amount, Discount = item.Discount, SaleOrderItemId = item.SaleOrderItemId });

            }

            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(6, OrganizationId);

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

            model.ProformaInvoiceRefNo = internalId;
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
                model.CreatedBy = UserID.ToString();
                string id = new ProformaInvoiceRepository().InsertProformaInvoice(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Proforma Invoice No. is " + id.Split('|')[1];
                    //TempData["error"] = "";
                    return RedirectToAction("PendingProforma", new { ProjectBased = model.isProjectBased });
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

        public void FillProformaInvoice(int type)
        {
            ViewBag.proformaInvoiceList = new SelectList(new DropdownRepository().ProformaInvoiceDropdown(OrganizationId: OrganizationId, type: type), "Id", "Name");
        }
        private void FillCustomer(int type)
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerForProformaInvoice(OrganizationId: OrganizationId, type: type), "Id", "Name");
        }
        public ActionResult Edit( int type,int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home", new {type = type});
            ProformaInvoice model = new ProformaInvoiceRepository().GetProformaInvoiceHdDetails(id);
            model.Items = new ProformaInvoiceRepository().GetProformaInvoiceItemDetails(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Edit(ProformaInvoice model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
          
              {
                try
                {
                    var result2 = new ProformaInvoiceRepository().DeleteProformaInvoiceDT(model.ProformaInvoiceId);
                    string id =   new ProformaInvoiceRepository().UpdateProformaInvoiceHD(model);
                    string id1 =  new ProformaInvoiceRepository().InsertProformaInvoiceDT(model);

                    TempData["success"] = "Updated successfully. Proforma Invoice Reference No. is " + id;
                    TempData["error"] = "";
                    return RedirectToAction("Index", new { type =model.isProjectBased});
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
                return View("Edit", model);
            }

        }


    }
}