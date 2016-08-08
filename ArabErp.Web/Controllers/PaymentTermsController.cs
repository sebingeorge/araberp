using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;


namespace ArabErp.Web.Controllers
{
    public class PaymentTermsController : BaseController
    {
        // GET: PaymentTerms
        public ActionResult Index()
        {
            return View();
        }

      
        public ActionResult FillPaymentTermsList(int? page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var rep = new PaymentTermsRepository();
            var List = rep.FillPaymentTermsList();
            return PartialView("PaymentTermsListView", List);
        }
        public ActionResult PaymentTermsPopup()
        {
            var rep = new PaymentTermsRepository();
            var List = rep.FillPaymentTermsList();
            return View(List);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Create";
            PaymentTerms PaymentTerms = new PaymentTerms();
            PaymentTerms.PaymentTermsRefNo = new PaymentTermsRepository().GetRefNo(PaymentTerms);
            return View(PaymentTerms);
        }
        [HttpPost]
        public ActionResult Create(PaymentTerms model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
         
            var repo = new PaymentTermsRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "PaymentTerms", "PaymentTermsName", model.PaymentTermsName, null, null);
            if (!isexists)
            {
                var result = new PaymentTermsRepository().InsertPaymentTerms(model);
                if (result.PaymentTermsId > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                    TempData["PaymentTermsRefNo"] = result.PaymentTermsRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["PaymentTermsRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["PaymentTermsRefNo"] = null;
                return View("Create", model);
            }

        }


        public ActionResult Edit(int Id)
        {
            ViewBag.Title = "Edit";
            PaymentTerms objPaymentTerms = new PaymentTermsRepository().GetPaymentTerms(Id);
            return View("Create", objPaymentTerms);
        }

        [HttpPost]
        public ActionResult Edit(PaymentTerms model)
        {

            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();

            var repo = new PaymentTermsRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "PaymentTerms", "PaymentTermsName", model.PaymentTermsName, "PaymentTermsId", model.PaymentTermsId);
            if (!isexists)
            {
                var result = new PaymentTermsRepository().UpdatePaymentTerms(model);
                if (result.PaymentTermsId > 0)
                {

                    TempData["Success"] = "Updated Successfully!";
                    TempData["PaymentTermsRefNo"] = result.PaymentTermsRefNo;
                    return RedirectToAction("Create");
                }

                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["PaymentTermsRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {
                TempData["error"] = "This Name Alredy Exists!!";
                TempData["PaymentTermsRefNo"] = null;
                return View("Create", model);
            }

        }



        //    var result = new PaymentTermsRepository().UpdatePaymentTerms(model);

        //    if (result.PaymentTermsId > 0)
        //    {
        //        TempData["Success"] = "Updated Successfully!";
        //        TempData["PaymentTermsRefNo"] = result.PaymentTermsRefNo;
        //        return RedirectToAction("Create");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Oops!!..Something Went Wrong!!";
        //        TempData["PaymentTermsRefNo"] = null;
        //        return View("Edit", model);
        //    }

        //}

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";
            PaymentTerms objPaymentTerms = new PaymentTermsRepository().GetPaymentTerms(Id);
            return View("Create", objPaymentTerms);

        }

        [HttpPost]
        public ActionResult Delete(PaymentTerms model)
        {
            int result = new PaymentTermsRepository().DeletePaymentTerms(model);

            if (result == 0)
            {
                TempData["Success"] = "Deleted Successfully!";
                TempData["PaymentTermsRefNo"] = model.PaymentTermsRefNo;
                return RedirectToAction("Create");
            }
            else
            {
                if (result == 1)
                {
                    TempData["error"] = "Sorry!! You Cannot Delete This Payment Terms. It Is Already In Use";
                    TempData["PaymentTermsRefNo"] = null;
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["PaymentTermsRefNo"] = null;
                }
                return RedirectToAction("Create");
            }

        }
    }
}