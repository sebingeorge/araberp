﻿using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class ProjectCompletionController : BaseController
    {
        // GET: ProjectCompletion
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PreviousList(string project = "", string saleorder = "")
        {
            return PartialView("_PreviousListGrid", new ProjectCompletionRepository().GetPreviousList(project: project, saleorder: saleorder, OrganizationId: OrganizationId));
        }

        public ActionResult Pending()
        {
            try
            {
                return View(new ProjectCompletionRepository().PendingForCompletion(OrganizationId));
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult PendingGrid(string saleorder = "")
        {
            return PartialView("_PendingGrid", new ProjectCompletionRepository().PendingForCompletion(
                OrganizationId: OrganizationId,
                saleorder: saleorder));
        }

        public ActionResult Complete(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            ProjectCompletion model = new ProjectCompletionRepository().GetProjectDetails(id, OrganizationId);
            return View(new ProjectCompletion
            {
                SaleOrderId = id,
                ProjectCompletionDate = DateTime.Today,
                ProjectWarrantyExpiryDate = DateTime.Today.AddYears(1).AddDays(-1),
                ProjectCompletionRefNo = DatabaseCommonRepository.GetNextDocNo(30, OrganizationId),
                ProjectName = model.ProjectName,
                Location = model.Location,
                CustomerName = model.CustomerName,
                ItemBatches = new ProjectCompletionRepository().GetSerialNos(id).ToList<ItemBatch>()
            });
        }

        public ActionResult SaleOrderDetails(int id)
        {
            return PartialView("_SaleOrderDetails", new SaleOrderRepository().GetSaleOrder(id));
        }//SaleOrderId is received here

        //public ActionResult JobCardDetails(int id)
        //{
        //    return PartialView("_JobCardDetails", new ProjectCompletionRepository().GetJobCardDetails(id));
        //}

        public ActionResult ItemBatchDetails(int id)//SaleOrderId is received here
        {
            ProjectCompletion model = new ProjectCompletion();
            model.ItemBatches = new ProjectCompletionRepository().GetSerialNos(id).ToList<ItemBatch>();
            return PartialView("_ItemBatchDetails", model);
        }

        [HttpPost]
        public ActionResult Complete(ProjectCompletion model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                if (model.ItemBatches != null && model.ItemBatches.Count > 0)
                    foreach (ItemBatch item in model.ItemBatches)
                    {
                        item.WarrantyStartDate = model.ProjectCompletionDate;
                        item.WarrantyExpireDate = model.ProjectCompletionDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                    }
                string ref_no = new ProjectCompletionRepository().InsertProjectCompletion(model);
                TempData["success"] = "Saved Successfully. Reference No. is " + ref_no;
                return RedirectToAction("Pending");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured. Please try agian.";
            }
            return View(model);
        }

        public ActionResult Details(int id = 0)
        {
            if (id == 0) return RedirectToAction("Index", "Home");
            ProjectCompletion model = new ProjectCompletionRepository().GetProjectCompletion(id);
            model.ItemBatches = new ProjectCompletionRepository().GetSerialNosByProjectCompletioId(model.ProjectCompletionId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Details(ProjectCompletion model)
        {
            try
            {
                model.CreatedBy = UserID.ToString(); model.CreatedDate = DateTime.Today; model.OrganizationId = OrganizationId;
                if (model.ItemBatches != null && model.ItemBatches.Count > 0)
                    foreach (ItemBatch item in model.ItemBatches)
                    {
                        item.WarrantyStartDate = model.ProjectCompletionDate;
                        item.WarrantyExpireDate = model.ProjectCompletionDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);
                    }

                new ProjectCompletionRepository().UpdateProjectCompletion(model);
                TempData["success"] = "Updated Successfully (" + model.ProjectCompletionRefNo + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occurred. Please try again.";
            }
           return View(model);
        }

        public ActionResult Delete(int ProjectCompletionId = 0)
        {
            try
            {
                if (ProjectCompletionId == 0) return RedirectToAction("Index", "Home");
                //JobCard model = new JobCardRepository().GetJobCardDetails2(JobCardId, OrganizationId);
                string ref_no = new ProjectCompletionRepository().DeleteProjectCompletion(ProjectCompletionId);
                TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Some error occured while deleting. Please try again.";
                return RedirectToAction("Details", new { id = ProjectCompletionId });
            }
        }
    }
}