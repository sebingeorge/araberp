using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabErp.Domain;
using ArabErp.DAL;
using ArabErp.Web.Models;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class MaterialPlanningController : BaseController
    {
        // GET: MaterialPlanning
        //public ActionResult Index(int? isProjectBased)
        //{
        //    var repo = new SaleOrderRepository();
        //    IEnumerable<PendingSO> pendingSO = repo.GetSaleOrdersForHold(isProjectBased ?? 0);
        //    return View(pendingSO);
        //}
        public ActionResult Planning()
        {
            MaterialPlanningRepository repo = new MaterialPlanningRepository();
            return View(repo.GetMaterialPlanning());
        }
    }
}