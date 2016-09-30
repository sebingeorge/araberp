using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace ArabErp.Web.Controllers
{
    public class ItemSellingPriceController : BaseController
    {
        // GET: ItemSellingPrice
        public ActionResult Index()
        {
            ItemSellingPriceList obj = new ItemSellingPriceList();
            ItemSellingPriceRepository repo = new ItemSellingPriceRepository();
            obj = repo.GetItemSellingPrices(OrganizationId);
            return View(obj);
        }

        public ActionResult Save(ItemSellingPriceList model)
        {

            foreach (ItemSellingPrice item in model.ItemSellingPriceLists)
            {
                item.CreatedDate = System.DateTime.Now;
                item.CreatedBy = UserID.ToString();
                item.OrganizationId = OrganizationId;
            }

            var rtn = new ItemSellingPriceRepository().InsertItemSellingPrice(model.ItemSellingPriceLists);
      
            TempData["Success"] = "Added Successfully!";

            return RedirectToAction("Index");
        }
    }
}