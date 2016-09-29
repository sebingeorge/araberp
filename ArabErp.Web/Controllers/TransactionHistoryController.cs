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
            //string user = "";
            //string form = "";
            //string mode = "";

            return View();
            //return View((new TransactionHistoryRepository().GetTransactionHistorys(user, form, mode, From, To)));
        }

        //public ActionResult Grid(string FromDate, string ToDate)
        public ActionResult Grid(DateTime? from, DateTime? to,string user = "", string form = "", string mode = "")
        {
             from = from ?? DateTime.Today.AddMonths(-1);
             to = to?? DateTime.Today;
            //from = from ?? FYStartdate; ;
            //to = to ?? DateTime.Today;
            return PartialView("_Grid", (new TransactionHistoryRepository().GetTransactionHistorys(from, to,user.Trim(), form.Trim(), mode.Trim())).ToList());
        }
    }
}