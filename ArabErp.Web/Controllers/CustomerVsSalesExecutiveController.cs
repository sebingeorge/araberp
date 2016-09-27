using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class CustomerVsSalesExecutiveController : BaseController
    {
        // GET: CustomerVsSalesExecutive
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            CustomerVsSalesExecutivesRepository repo = new CustomerVsSalesExecutivesRepository();
           
             var model= repo.GetCustomerVsSalesExecutives(OrganizationId);
            
            foreach (var i in model.CustomerVsSalesExecutives)
            {

                if (i.EffectiveDate == DateTime.MinValue)
                {
                    i.EffectiveDate = DateTime.Now;
                }
               
            }
            FillEmployee();

            return View(model);
        }


        [HttpPost]
        public ActionResult Save(CustomerVsSalesExecutiveList model)
        {

            foreach (CustomerVsSalesExecutive item in model.CustomerVsSalesExecutives)
            {
                item.CreatedDate=System.DateTime.Now;
                item.CreatedBy=UserID.ToString();
                
            }

            var rtn = new CustomerVsSalesExecutivesRepository().InsertCustomerSalesExecutive(model.CustomerVsSalesExecutives);

            FillEmployee();


            TempData["Success"] = "Added Successfully!";
            CustomerVsSalesExecutive CustomerVsSales = new CustomerVsSalesExecutive();
            //    OpeningStock.OpeningStockItem = new List<OpeningStockItem>();
            //OpeningStock.OpeningStockItem.Add(new OpeningStockItem());
            return RedirectToAction("Create");
        }

        public void FillEmployee()
        {
            JobCardRepository repo = new JobCardRepository();
            var result = repo.GetEmployeeList();
            ViewBag.EmployeeList = new SelectList(result, "EmployeeId", "EmployeeName");
        }
    }

}