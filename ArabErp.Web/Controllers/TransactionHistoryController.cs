using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;
using ArabErp.Domain;

namespace ArabErp.Web.Controllers
{
    public class TransactionHistoryController : BaseController
    {
        // GET: TransactionHistory
        public ActionResult Index()
        {
            string From = DateTime.Today.AddMonths(-1).ToString("dd-MMMM-yyyy");
            string To = DateTime.Today.ToString("dd-MMMM-yyyy");
            return View((new TransactionHistoryRepository().GetTransactionHistorys(From, To)));
        }

        public ActionResult Grid(string FromDate, string ToDate)
        {
            return PartialView("_Grid",(new TransactionHistoryRepository().GetTransactionHistorys(FromDate, ToDate)));
        }
    }
}