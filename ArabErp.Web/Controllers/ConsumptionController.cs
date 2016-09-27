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
            FillConsumption();
            FillJobCardForPreviousList();
            return View();
        }

        public ActionResult PreviousList(DateTime? from, DateTime? to, int id = 0, int jobcard = 0)
        {
            return PartialView("_PreviousListGrid", new ConsumptionRepository().PreviousList(OrganizationId: OrganizationId, from: from, to: to, id: id, jobcard: jobcard));
        }

        public ActionResult Create()
        {
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(23, OrganizationId);
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
            return View(new Consumption { ConsumptionItems = Items, ConsumptionDate = DateTime.Today, ConsumptionNo = internalId });
        }

        [HttpPost]
        public ActionResult Create(Consumption model)
        {
            try
            {
                model.OrganizationId = OrganizationId;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = UserID.ToString();
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

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    JobCardDropdown();
                    ItemDropdown();
                    Consumption Consumption = new Consumption();
                    Consumption = new ConsumptionRepository().GetConsumptionHD(id);
                    Consumption.ConsumptionItems = new ConsumptionItemRepository().GetConsumptionDT(id);

                    return View(Consumption);
                }
                else
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    TempData["success"] = "";
                }
            }
            catch (InvalidOperationException iox)
            {
                TempData["error"] = "Sorry, we could not find the requested item. Please try again.|" + iox.Message;
            }
            catch (SqlException sx)
            {
                TempData["error"] = "Some error occured while connecting to database. Please try again after sometime.|" + sx.Message;
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
            return RedirectToAction("Index");
        }

        #region Dropdown
        public void ItemDropdown()
        {
            ViewBag.itemList = new SelectList(new DropdownRepository().ItemDropdown(), "Id", "Name");
        }
        public void JobCardDropdown()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardDropdown(), "Id", "Name");
        }
        public void FillConsumption()
        {
            ViewBag.consumptionList = new SelectList(new DropdownRepository().ConsumptionDropdown(OrganizationId), "Id", "Name");
        }
        public void FillJobCardForPreviousList()
        {
            ViewBag.jobcardList = new SelectList(new DropdownRepository().JobCardForConsumption(OrganizationId), "Id", "Name");
        }
        #endregion
    }
}