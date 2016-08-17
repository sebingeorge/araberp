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
        public ActionResult Index()
        {
            FillItem();
            //FillPartNo();
            InitDropdown();
            FillBatch();
            MaterialPlanning mp = new MaterialPlanning();
            mp.ItemId = 0;
            return View("Index", mp);
        }
        public void InitDropdown()
        {
            var List = "";
            ViewBag.PartNoList = new SelectList(List, "Id", "Name");
           
        }
        public void FillBatch()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 0, Name = "Non-Batch" });
            types.Add(new Dropdown { Id = 1, Name = "Batch" });
            ViewBag.BatchList = new SelectList(types, "Id", "Name");
        }
        public void FillItem()
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.ItemFGDropdown();
            ViewBag.ItemList = new SelectList(result, "Id", "Name");
        }

        public void FillPartNo(int Id)
        {
            DropdownRepository repo = new DropdownRepository();
            var result = repo.PartNoDropdown(Id);
            ViewBag.PartNoList = new SelectList(result, "Id", "Name");
        }

        public ActionResult Planning(int? batch,string partNo , int itmid = 0)
        {
            return PartialView("_Planning", new MaterialPlanningRepository().GetMaterialPlanning(partNo,itmid,batch));
        }
        public ActionResult Item()
        {
            FillItem();
            return PartialView("_ItemDropDown");
        }
        public ActionResult PartNo(int Code)
        {
            FillPartNo(Code);
            return PartialView("_PartNoDropDown");
        }


    }
}