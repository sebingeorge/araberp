using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class QCParameterController : BaseController
    {
        // GET: QCParameter
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            QCParameters QC = new QCParameters();
            QC.QCRefNo = new QCParametersRepository().GetRefNo(QC);
            dropdown();
            return View(QC);
        }

        public void dropdown()
        {
            var repo = new QCParametersRepository();
            var List = repo.FillParaType();
            ViewBag.QCParaType = new SelectList(List, "Id", "Name");

        }

        [HttpPost]
        public ActionResult Create(QCParameters model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var repo = new QCParametersRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "QCParam", "QCParamName", model.QCParamName, null, null);
            if (!isexists)
            {
                var result = new QCParametersRepository().InsertBox(model);
                if (result.OrganizationId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["QCRefNo"] = result.QCRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    dropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["QCRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                dropdown();
                TempData["error"] = "This Organization Name Alredy Exists!!";
                TempData["QCRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult FillQCParameterList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new QCParametersRepository();
            var List = repo.FillQCParameterList();
            return PartialView("QCParameterListView", List);
        }
        public ActionResult Edit(int Id)
        {
            dropdown();
            ViewBag.Title = "Edit";
            QCParameters objQCParameters = new QCParametersRepository().GetQCParameters(Id);
            return View("Create", objQCParameters);
        }

        [HttpPost]
        public ActionResult Edit(QCParameters model)
        {

            var repo = new QCParametersRepository();
            model.CreatedBy = UserID.ToString();
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "QCParam", "QCParamName", model.QCParamName, "QCParamId", model.QCParamId);
            if (!isexists)
            {
                var result = new QCParametersRepository().UpdateQCParameter(model);

                if (result.OrganizationId > 0)
                {
                    TempData["Success"] = "Updated Successfully!";
                    TempData["QCRefNo"] = result.QCRefNo;
                    return RedirectToAction("Create");
                }
                else
                {
                    dropdown();
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["QCRefNo"] = null;
                    return View("Edit", model);
                }
            }
            else
            {
                dropdown();
                TempData["error"] = "This Organization Name Alredy Exists!!";
                TempData["QCRefNo"] = null;
                return View("Create", model);
            }

        }

        public ActionResult Delete(int Id)
        {
            dropdown();
            ViewBag.Title = "Delete";
            QCParameters objOrganization = new QCParametersRepository().GetQCParameters(Id);
            return View("Create", objOrganization);

        }

        [HttpPost]
        public ActionResult Delete(QCParameters model)
        {
            model.CreatedBy = UserID.ToString();
            int result = new QCParametersRepository().DeleteQCPara(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["QCRefNo"] = model.QCRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Organization It Is Already In Use";
                    TempData["QCRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["QCRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }

    }
}