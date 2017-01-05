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
        public ActionResult Index(string type)
        {

            ViewBag.Type = type;
            return View();
        }

        public ActionResult CreateQuerySheet(string type)
        {
            UnitDropDown();
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(5, OrganizationId);

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
            qs.Type = type;
            qs.QuerySheetDate = DateTime.Now;
            var repo = new QuerySheetRepository();
            qs.QuerySheetItems = new List<QuerySheetItem>();
            qs.QuerySheetItems.Add(new QuerySheetItem());
            qs.QuerySheetRefNo = internalId;

            return View(qs);
        }

        public ActionResult CreateQuerySheetUnit(string type, int QuerySheetId)
        {
            var repo = new QuerySheetRepository();
            UnitDropDown();
            var qs = new QuerySheetRepository().GetQuerySheet(QuerySheetId);
            qs.Type = type;
            qs.QuerySheetItems = new ProjectCostRepository().GetQuerySheetItem(QuerySheetId);
            if (type == "Costing")
            {
                var PCList = repo.GetProjectCostingParameter();
                qs.Items = new List<ProjectCost>();
                foreach (var item in PCList)
                {
                    var pcitem = new ProjectCost { CostingId = item.CostingId, Description = item.Description, Remarks = item.Remarks, Amount = item.Amount };
                    qs.Items.Add(pcitem);

                }
            }

            return View("CreateQuerySheet", qs);
        }

        public ActionResult CreateQuerySheetCosting(string type)
        {
            string internalId = "";
            try
            {
                internalId = DatabaseCommonRepository.GetNextDocNo(5, OrganizationId);

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
            qs.Type = type;
            qs.QuerySheetDate = DateTime.Now;
            var repo = new QuerySheetRepository();
            qs.QuerySheetItems = new List<QuerySheetItem>();
            qs.QuerySheetItems.Add(new QuerySheetItem());
            var PCList = repo.GetProjectCostingParameter();
            qs.Items = new List<ProjectCost>();
            qs.QuerySheetRefNo = internalId;
            foreach (var item in PCList)
            {
                var pcitem = new ProjectCost { CostingId = item.CostingId, Description = item.Description, Remarks = item.Remarks, Amount = item.Amount };
                qs.Items.Add(pcitem);

            }
            return View("CreateQuerySheet", qs);
        }

        [HttpPost]
        public ActionResult CreateQuerySheet(QuerySheet qs)
        {

            //if (qs.QuerySheetId==0)
            //{
            try
            {
                qs.OrganizationId = OrganizationId;
                qs.CreatedDate = System.DateTime.Now;
                qs.CreatedBy = UserID.ToString();
                var id = "";
                int row;
                if (qs.Type == "Unit")
                {
                    UnitDropDown();
                    row = new QuerySheetRepository().UpdateQuerySheetUnit(qs);
                    TempData["success"] = "Saved Successfully (" + qs.QuerySheetRefNo + ")";
                    return RedirectToAction("PendingQuerySheetforUnit");
                }
                else if (qs.Type == "Costing")
                {
                    UnitDropDown();
                    if (qs.Items == null || qs.Items.Count <= 0)
                    {
                        TempData["error"] = "Query Sheet cannot be saved without costing parameters.";
                        return RedirectToAction("CreateQuerySheetUnit", new { QuerySheetid = qs.QuerySheetId, type = "Costing" });
                    }
                    row = new ProjectCostRepository().InsertProjectCosting(qs);
                    TempData["success"] = "Saved Successfully (" + qs.QuerySheetRefNo + ")";
                    return RedirectToAction("PendingQuerySheetforCosting");
                }
                else if (qs.Type == "RoomDetails")
                {
                    id = new QuerySheetRepository().InsertQuerySheet(qs);


                    if (id.Split('|')[0] != "0")
                    {
                        qs.QuerySheetId = Convert.ToInt16(id.Split('|')[0]);
                        qs.QuerySheetRefNo = id.Split('|')[1];
                        TempData["success"] = "Saved successfully.  Reference No. is " + id.Split('|')[1];
                        TempData["error"] = "";
                        return RedirectToAction("CreateQuerySheet", new { type = qs.Type });
                    }
                    else
                    {
                        throw new Exception();
                    }
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
            //}
            return RedirectToAction("CreateQuerySheet", new { type = qs.Type });

        }

        // GET: QuerySheet
        public ActionResult ViewQuerySheet(int QuerySheetId)
        {

            var qs = new QuerySheetRepository().GetQuerySheet(QuerySheetId);
            return View("CreateQuerySheet", qs);
        }

        public ActionResult QuerySheetList(string Type, DateTime? from, DateTime? to, string querysheet = "")
        {
            return PartialView("QuerySheetList", new QuerySheetRepository().GetQuerySheets(Type, OrganizationId: OrganizationId, querysheet: querysheet, from: from, to: to));
        }

        public ActionResult Edit(int id = 0)
        {
            try
            {
                UnitDropDown();
                if (id != 0)
                {
                    QuerySheet QuerySheet = new QuerySheet();
                    QuerySheet = new QuerySheetRepository().GetQuerySheet(id);
                    var repo = new ProjectCostRepository();
                    QuerySheet.Items = repo.GetProjectCost(id);
                    QuerySheet.QuerySheetItems = repo.GetQuerySheetItem(id);
                    return View(QuerySheet);
                }
                else
                {
                    TempData["error"] = "That was an invalid/unknown request. Please try again.";
                    return RedirectToAction("Index", "Home");
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
                TempData["error"] = "Sorry! Already Used!";
                TempData["QuerySheetRefNo"] = null;
                return View("Edit", model);
            }

            else
            {
                try
                {

                    string ref_no = new QuerySheetRepository().UpdateQuerySheet(model);

                    TempData["success"] = "Updated successfully. Query Sheet Reference No. is " + ref_no;
                    TempData["error"] = "";
                    return RedirectToAction("Index");
                }
                catch (SqlException)
                {
                    TempData["error"] = "Some error occured while connecting to database. Please check your network connection and try again.";
                }
                catch (NullReferenceException)
                {
                    TempData["error"] = "Some required data was missing. Please try again.";
                }
                catch (Exception)
                {
                    TempData["error"] = "Some error occured. Please try again.";
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
                TempData["error"] = "Sorry! Already Used!";
                TempData["QuerySheetRefNo"] = null;
                return RedirectToAction("Edit", new { id = Id });
            }
            else
            {
                //var result2 = new QuerySheetRepository().DeleteProjectCosting(Id);
                try
                {
                    var ref_no = new QuerySheetRepository().DeleteQuerySheet(Id, UserID.ToString(), OrganizationId);

                    TempData["success"] = "Deleted Successfully (" + ref_no + ")";
                    //return RedirectToAction("PreviousList");
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["error"] = "Oops! Something went wrong!";
                    TempData["QuerySheetRefNo"] = null;
                    return RedirectToAction("Edit", new { id = Id });
                }
            }
        }

        public ActionResult PendingQuerySheetforUnit()
        {
            var repo = new QuerySheetRepository();
            List<QuerySheet> Pending = repo.GetPendingQuerySheetforUnit();
            ViewBag.Type = "Unit";
            return View("PendingQuerySheet", Pending);
        }

        public void UnitDropDown()
        {
            ViewBag.UnitList = new SelectList(new DropdownRepository().FillFreezerUnit(), "Id", "Name");
        }
        public ActionResult UnitSelection()
        {
            UnitDropDown();
            QuerySheet qs = new QuerySheet();
            qs.QuerySheetUnits = new List<QuerySheetUnit>();
            qs.QuerySheetUnits.Add(new QuerySheetUnit());
            return PartialView("_UnitSelection", qs);
        }
        public ActionResult DoorSelection()
        {
            UnitDropDown();
            QuerySheet qs = new QuerySheet();
            qs.QuerySheetDoors = new List<QuerySheetDoor>();
            qs.QuerySheetDoors.Add(new QuerySheetDoor());
            return PartialView("_DoorSelection", qs);
        }
        public ActionResult PendingQuerySheetforCosting()
        {
            var repo = new QuerySheetRepository();
            List<QuerySheet> Pending = repo.GetPendingQuerySheetforCosting();
            ViewBag.Type = "Costing";
            return View("PendingQuerySheet", Pending);
        }
    }
}