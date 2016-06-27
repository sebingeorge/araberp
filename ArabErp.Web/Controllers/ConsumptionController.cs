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
    public class ConsumptionController : BaseController
    {
        // GET: Consumption
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(Consumption).Name);
            JobCardDropdown();
            ItemDropdown();
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
            List<ConsumptionItem> Items = new List<ConsumptionItem>();
            Items.Add(new ConsumptionItem { SlNo = 1 });
            return View(new Consumption { ConsumptionItems = Items, ConsumptionDate = DateTime.Today, ConsumptionNo = "CON/" + internalId });
        }

        [HttpPost]
        public ActionResult Create(Consumption model)
        {
            try
            {
                model.OrganizationId = 1;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                string id = new ConsumptionRepository().InsertConsumption(model);
                if (id.Split('|')[0] != "0")
                {
                    TempData["success"] = "Saved successfully. Consumption Reference No. is " + id.Split('|')[1];
                    TempData["error"] = "";
                    return RedirectToAction("Create");
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
            JobCardDropdown();
            ItemDropdown();
            return View(model);
        }

        //public ActionResult ConsumptionGrid()
        //{
        //    return PartialView("_ConsumptionGrid");
        //}

        public void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }

        public void JobCardDropdown()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardDropdown(), "Id", "Name");
        }

        public JsonResult JobCardDetails(int jobCardId)
        {
            Consumption j = new JobCardRepository().GetJobCardDetails1(jobCardId);
            return Json(new
            {
                JobCardDate = j.JobCardDate,
                SONoDate = j.SONoDate,
                FreezerUnitName = j.FreezerUnitName,
                BoxName = j.BoxName
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemPartNo(int itemId)
        {
            return Json(new ItemRepository().GetPartNoUnit(itemId), JsonRequestBehavior.AllowGet);
        }
    }
}