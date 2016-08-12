using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;
using System.Collections;

namespace ArabErp.Web.Controllers
{
    public class RateSettingsController : BaseController
    {
        // GET: RateSettings
        public ActionResult Index()
        {
            //List<RateSettingsItems> items = new RateSettingsRepository().GetWorkDescriptions(1); //load project work description by default
            string date = new RateSettingsRepository().GetExpiryDate();
            DateTime expiryDate = Convert.ToDateTime(date.Length == 1 ? DateTime.Today.ToString() : date).AddDays(1);
            return View(new RateSettings
            {
                FromDate = expiryDate,
                ToDate = expiryDate,
                Type = 0
            });
        }
        [HttpPost]
        public ActionResult Index(RateSettings model)
        {
            try
            {
                new RateSettingsRepository().InsertRateSettings(model, UserID.ToString());
                TempData["success"] = "Saved successfully";
                TempData["error"] = "";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["success"] = "";
                TempData["error"] = "Some error occurred. Please try again.|" + ex.Message;
                return View(model);
            }
        }

        public JsonResult GetExpiryDate()
        {
            return Json(new RateSettingsRepository().GetExpiryDate(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult WorkDescriptions(int type = 0)
        {
            RateSettings model = new RateSettings();
            model.Items = new RateSettingsRepository().GetWorkDescriptions(type);
            return PartialView("_grid", model);
        }

        public JsonResult ValidateDate(string date)
        {
            string data = new RateSettingsRepository().ValidateDate(Convert.ToDateTime(date));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PreviousListPopup()
        {
            try
            {
                var rep = new RateSettingsRepository();
                var List = rep.GetPreviousList();
                return View(List);
                //IEnumerable<RateSettings> model = new RateSettingsRepository().GetPreviousList();
                //return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "RateSettingsController", "PreviousListPopup"));
            }
        }

        public ActionResult PreviousWorkDescriptions(string from)
        {
            RateSettings model = new RateSettings();
            model.Items = new RateSettingsRepository().GetPreviousWorkDescriptions(from);
            return PartialView("_grid", model);
        }

        public IEnumerable RateSettingsDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem{ Text = "Custom", Value = "0" },
                new SelectListItem{ Text = "Special", Value = "4" },
                new SelectListItem{ Text = "Min", Value = "1" },
                new SelectListItem{ Text = "Medium", Value = "2" },
                new SelectListItem{ Text = "Max", Value = "3" }

            };
        }

        public JsonResult GetCurrency()
        {
            Currency model = new CurrencyRepository().GetCurrencyFrmOrganization(OrganizationId);
            return Json(new { CurrencyName = model.CurrencyName, SymbolName = model.SymbolName }, JsonRequestBehavior.AllowGet);
        }
    }
}