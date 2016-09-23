using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class VehicleInPassController : BaseController
    {
        // GET: VehicleInPass
        public ActionResult Index()
        {
            CustomerDropdown();
            return View();
        }
        public void CustomerDropdown()
        {
            ViewBag.customerList = new SelectList(new DropdownRepository().CustomerDropdown(), "Id", "Name");
        }
        public PartialViewResult PendingVehicleInPass(int customerId)
        {
            if (customerId == 0)
            {
                List<PendingSO> list = new List<PendingSO>();
                return PartialView("_PendingVehicleInPass", list);
            }
            return PartialView("_PendingVehicleInPass", new VehicleInPassRepository().PendingVehicleInpass(customerId));
        }
        public ActionResult Save(int id=0)
        {
            if (id != 0)
            {
                EmployeeDropdown();
                string internalId = "";
                try
                {
                    internalId = DatabaseCommonRepository.GetNextDocNo(15, OrganizationId);

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
                return View(new VehicleInPass { SaleOrderItemId = id ,VehicleInPassDate = DateTime.Now,VehicleInPassNo = internalId});
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Save(VehicleInPass model)
        {
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();
            if(new VehicleInPassRepository().InsertVehicleInPass(model) > 0)
               
                {
                    TempData["Success"] = "Added Successfully!";
                    TempData["VehicleInPassNo"] = model.VehicleInPassNo;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["VehicleInPassNo"] = null;
                    return View(new VehicleInPass { SaleOrderItemId = model.SaleOrderItemId });
                }
                   
        }
        public void EmployeeDropdown()
        {
            ViewBag.employeeList = new SelectList(new DropdownRepository().EmployeeDropdown(), "Id", "Name");
        }
        public JsonResult GetSaleOrderItemDetails(int id = 0)
        {
            var data = new VehicleInPassRepository().GetSaleOrderItemDetails(id);
            return Json(new
            {
                SaleOrderRefNo = data.SaleOrderRefNo,
                WorkDescr = data.WorkDescription ,
                VehicleModelName = data.VehicleModelName,
                CustomerName = data.CustomerName
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VehicleInpassList()
        {
            FillVINo();
            FillCustomerinVI();
            return View();
        }
        public void FillVINo()
        {
            ViewBag.VINoList = new SelectList(new DropdownRepository().VINODropdown(OrganizationId), "Id", "Name");
        }
        public void FillCustomerinVI()
        {
            ViewBag.CusList = new SelectList(new DropdownRepository().VICustomerDropdown(OrganizationId), "Id", "Name");
        }
        public ActionResult PreviousList(DateTime? from , DateTime? to,int id = 0, int cusid = 0)
        {
             from = from ?? DateTime.Today.AddMonths(-1);
            to = to ?? DateTime.Today;
            return PartialView("_PreviousList", new VehicleInPassRepository().GetPreviousList(id, cusid, OrganizationId, from, to));
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    EmployeeDropdown();
                    VehicleInPass VehicleInPass = new VehicleInPass();
                    VehicleInPass = new VehicleInPassRepository().GetVehicleInPassHD(id);
                    
                  
                    return View(VehicleInPass);
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

    }
}