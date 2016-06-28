using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class LoginHistoryController : BaseController
    {
        // GET: LoginHistory
        public ActionResult Index()
        {
            return View((new LoginHistoryRepository()).GetLoginHistory());
        }
    }
}