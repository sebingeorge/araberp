using ArabErp.DAL;
using ArabErp.Domain;
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
            PrefixSettings model = new PrefixSettings();
            model.Prefixes = new List<PrefixSettingsVsOrganization>();
            var res = repo.GetPrefixSettings(OrganizationId);
            foreach(var item in res)
            {
                model.Prefixes.Add(item);
            }
            return View(model);
        }
        public ActionResult Save(PrefixSettings model)
        {
            PrefixSettingsRepository repo = new PrefixSettingsRepository();
            repo.SavePrefixSettings(model.Prefixes, OrganizationId);
            TempData["Success"] = "Updated Successfully!";
            return RedirectToAction("Index");
        }
    }
}