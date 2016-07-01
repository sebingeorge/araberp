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

        public ActionResult Create(IList<PendingGRN> PendingGRNSelected)

        {
            FillAdditionDeduction();
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

            return View(purchasebill);

        }
        public ActionResult Save(PurchaseBill model)
        {
            try
            {
            model.OrganizationId = 1;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
      
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
        public ActionResult PurchaseBillList()
        {
            var repo = new PurchaseBillRepository();
            IEnumerable<PurchaseBill> PurchaseBillList = repo.GetPurchaseBillPreviousList();
            return View("PurchaseBillList", PurchaseBillList);
           
        }

    }
}