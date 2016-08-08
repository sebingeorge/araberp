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
                    qs.CreatedBy = UserID.ToString();
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

        public ActionResult QuerySheetList(DateTime? from, DateTime? to, int id = 0)
        {

            return PartialView("QuerySheetList", new QuerySheetRepository().GetQuerySheets(OrganizationId: OrganizationId, id: id, from: from, to: to));
            
            //var qs = new QuerySheetRepository().GetQuerySheets(id, OrganizationId,from, to);
            //return View( qs);
        }

        public void FillRefNo()
        {
            ViewBag.QSnoList = new SelectList(new DropdownRepository().QuerySheetRefNoDropdown(), "Id", "Name");
        }
        public ActionResult Edit(int id = 0)
        {
            try
            {
                if (id != 0)
                {
                    QuerySheet QuerySheet = new QuerySheet();
                    QuerySheet = new QuerySheetRepository().GetQuerySheet(id);
                    QuerySheet.Items = new ProjectCostRepository().GetProjectCost(id);

                    return View(QuerySheet);
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
            return RedirectToAction("CreateQuerySheet");
        }

        [HttpPost]
        public ActionResult Edit(QuerySheet model)
        {
            ViewBag.Title = "Edit";
            model.OrganizationId = OrganizationId;
            model.CreatedDate = System.DateTime.Now;
            model.CreatedBy = UserID.ToString();



            var repo = new QuerySheetRepository();

            var result1 = new QuerySheetRepository().CHECK(model.QuerySheetId);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["QuerySheetRefNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {
                    var result2 = new QuerySheetRepository().DeleteProjectCosting(model.QuerySheetId);
                    var result3 = new QuerySheetRepository().DeleteQuerySheet(model.QuerySheetId, UserID.ToString());
                    string id = new QuerySheetRepository().InsertQuerySheet(model);

                    TempData["success"] = "Updated successfully. Purchase Request Reference No. is " + id;
                    TempData["error"] = "";
                    return RedirectToAction("Index");
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
                return RedirectToAction("CreateQuerySheet");
            }

        }

        public ActionResult Delete(int Id)
        {
            ViewBag.Title = "Delete";

            var result1 = new QuerySheetRepository().CHECK(Id);
            if (result1 > 0)
            {
                TempData["error"] = "Sorry!!..Already Used!!";
                TempData["QuerySheetRefNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }

            else
            {
                var result2 = new QuerySheetRepository().DeleteProjectCosting(Id);
                var result3 = new QuerySheetRepository().DeleteQuerySheet(Id, UserID.ToString());

                if (Id > 0)
                {

                    TempData["Success"] = "Deleted Successfully!";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("Index");
                }

                else
                {

                    TempData["error"] = "Oops!!..Something Went Wrong!!";
                    TempData["QuerySheetRefNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }

            }

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