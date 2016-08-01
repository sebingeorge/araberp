using ArabErp.DAL;
using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArabErp.Web.Controllers
{
    public class QuerySheetController : BaseController
    {
        // GET: QuerySheet
        public ActionResult Index()
        {
            FillRefNo();
            return View();
        }

        public ActionResult CreateQuerySheet()
        {
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextReferenceNo(typeof(QuerySheet).Name);
               
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
            QuerySheet qs = new QuerySheet();
            qs.QuerySheetDate = DateTime.Now;
            var repo = new QuerySheetRepository();
            var PCList = repo.GetProjectCostingParameter();
            qs.Items = new List<ProjectCost>();
            qs.QuerySheetRefNo = "QSH/" + internalId;
            foreach (var item in PCList)
            {
                var pcitem = new ProjectCost {CostingId=item.CostingId ,Description = item.Description, Remarks = item.Remarks, Amount = item.Amount };
                qs.Items.Add(pcitem);

            }
            return View(qs);
        }

        [HttpPost]
        public ActionResult CreateQuerySheet(QuerySheet qs)
        {

            if (qs.QuerySheetId==0)
            {
                try
                {
                    qs.OrganizationId = OrganizationId;
                    qs.CreatedDate = System.DateTime.Now;
                    qs.CreatedBy = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
                    var id = new QuerySheetRepository().InsertQuerySheet(qs);
                    if (id.Split('|')[0] != "0")
                    {
                        qs.QuerySheetId = Convert.ToInt16( id.Split('|')[0]);
                        qs.QuerySheetRefNo = id.Split('|')[1];
                        TempData["success"] = "Saved successfully.  Reference No. is " + id.Split('|')[1];
                        TempData["error"] = "";
                        return RedirectToAction("CreateQuerySheet");
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (SqlException sx)
                {
                    TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.|" + sx.Message;
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
            }
            return View(qs);

        }

        // GET: QuerySheet
        public ActionResult ViewQuerySheet(int QuerySheetId)
        {

            var qs = new QuerySheetRepository().GetQuerySheet(QuerySheetId);
            return View("CreateQuerySheet",qs);
        }

        public ActionResult QuerySheetList(int id = 0)
        {
            var qs = new QuerySheetRepository().GetQuerySheets(id);
            return View( qs);
        }

        public void FillRefNo()
        {
            ViewBag.QSnoList = new SelectList(new DropdownRepository().QuerySheetRefNoDropdown(), "Id", "Name");
        }

      //public ActionResult ProjectCosting()
      //  {
      //      var repo = new QuerySheetRepository();
      //      QuerySheet model=new QuerySheet ();
      //      var PCList = repo.GetProjectCostingParameter();
      //      model.Items  = new List<ProjectCost>();

      //      foreach (var item in PCList)
      //      {
      //          var pcitem = new ProjectCost { Description = item.Description, Remarks = item.Remarks, Amount = item.Amount };
      //          model.Items.Add(pcitem);

      //      }
      //      return PartialView("_ProjectCosting", model);
      //  }

    }
}