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
            var repo = new VehicleModelRepository();
            new VehicleModelRepository().InsertVehicleModel(objVehicleModel);
            return View("Create");
        }

        public ActionResult FillVehicleModelList(int?page)
        {
            int itemsPerPage = 10;
            int pageNumber = page ?? 1;
            var repo = new VehicleModelRepository();
            var List = repo.FillVehicleModelList();
            return PartialView("VehicleModelListView",List);
        }

    }
}