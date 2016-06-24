using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class AdditionOrDeductionController : BaseController
    {
        // GET: AdditionOrDeduction
        public ActionResult Index()
        {
            AdditionOrDeductionRepository _repo = new AdditionOrDeductionRepository();
            return View(_repo.GetAdditionDeduction());
        }
        public ActionResult Create()
        {
            FillAdditionDeduction();
            return View(new AdditionOrDeduction());
        }
        public void FillAdditionDeduction()
        {
            List<Dropdown> types = new List<Dropdown>();
            types.Add(new Dropdown { Id = 1, Name="Addtion" });
            types.Add(new Dropdown { Id = 2, Name = "Deduction" });
            ViewBag.AdditionDeduction = new SelectList(types, "Id", "Name");
        }
        [HttpPost]
        public ActionResult Create(AdditionOrDeduction model)
        {
            if(ModelState.IsValid)
            {
                AdditionOrDeductionRepository _repo = new AdditionOrDeductionRepository();
                int id = _repo.Insert(model);
                if(id > 0)
                {
                    return RedirectToAction("Create");
                }
                else
                {
                    FillAdditionDeduction();
                    return View(model);
                }               
            }
            else
            {
                FillAdditionDeduction();
                return View(model);
            }
        }
    }
}