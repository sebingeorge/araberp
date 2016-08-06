using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class PurchaseBillRegisterController : BaseController
    {
        // GET: PurchaseBillRegister
        public ActionResult Index()
        {
            FillItem();
            FillSupplier();
            return View();
        }
    
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }
        public void FillSupplier()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.GrnSupplierDropdown();
            ViewBag.SupplierList = new SelectList(result, "Id", "Name");
        }
      
    }
}