using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class CompanyController : BaseController
    {
        // GET: Company
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Company model)
        {
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            var repo = new CompanyRepository();
            bool isexists = repo.IsFieldExists(repo.ConnectionString(), "mstAccCompany", "cmpUsercode", model.cmpUsercode, null, null);
            if (!isexists)
            {
                var result = new CompanyRepository().InsertCompany(model);
                if (result.cmpCode > 0)
                {

                    TempData["Success"] = "Added Successfully!";
                   // TempData["CustomerRefNo"] = result.CustomerRefNo;
                    return RedirectToAction("Index");
                }

                else
                {
                  
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    //TempData["CustomerRefNo"] = null;
                    return View("Create", model);
                }

            }
            else
            {

                TempData["error"] = "This Name Alredy Exists!!";
             //   TempData["CustomerRefNo"] = null;
                return View("Create", model);
            }


       
        }
    }
}