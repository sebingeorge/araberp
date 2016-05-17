using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;

namespace ArabErp.Web.Controllers
{
    public class VehicleModelController : Controller
    {
        // GET: VehicleModel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        {
            return View();
        }
        public ActionResult Save(VehicleModel objVehicleModel)
        {
           // var repo = new VehicleModelRepository();
            //new VehicleModelRepository().InsertVehicleModel(objVehicleModel);
            return View("Create");
        }

    }
}