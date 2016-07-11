using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class QuerySheetController : BaseController
    {
        // GET: QuerySheet
        public ActionResult CreateQuerySheet()
        {
            QuerySheet qs = new QuerySheet();

            return View(qs);
        }

        [HttpPost]
        public ActionResult CreateQuerySheet(QuerySheet qs)
        {

            qs.ProjectName = "Saved";
            return View(qs);
        }
    }
}