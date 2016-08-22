using ArabErp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class PrefixSettingsController : BaseController
    {
        // GET: PrefixSettings
        public ActionResult Index()
        {
            PrefixSettingsRepository repo = new PrefixSettingsRepository();
            return View(repo.GetPrefixSettings(OrganizationId));
        }
    }
}