﻿using ArabErp.DAL;
using ArabErp.Domain;
using ArabErp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace ArabErp.Web.Controllers
{
    public class IndentVsSOVsInpassStatusController : BaseController
    {
        // GET: IndentVsSOVsInpassStatus
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndentVsSOVsInpassStatus(string supplier = "", string item = "", string indentno = "", string sono = "", string grnno = "")
        {
            return PartialView("IndentVsSOVsInpassStatus", new IndentVsSOVsInpassStatusRepository().GetIndentRegister(supplier, item, indentno, sono, grnno));
        }

    }
}