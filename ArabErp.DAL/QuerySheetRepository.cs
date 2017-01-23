using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class QuerySheetRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public string InsertQuerySheet(QuerySheet objQuerySheet)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objQuerySheet.OrganizationId, 5, true, txn);
                    objQuerySheet.QuerySheetRefNo = internalId;

                    string sql = @"insert  into QuerySheet(QuerySheetRefNo,QuerySheetDate,ProjectName,ContactPerson,ContactNumber,Email,Type,CreatedBy,CreatedDate,OrganizationId)
                                 Values (@QuerySheetRefNo,@QuerySheetDate,@ProjectName,@ContactPerson,@ContactNumber,@Email,@Type,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objQuerySheet, txn).Single();


                    foreach (QuerySheetItem item in objQuerySheet.QuerySheetItems)
                    {
                        item.QuerySheetId = id;
                        new ProjectCostRepository().InsertQuerySheetItem(item, connection, txn);
                    }

                    InsertLoginHistory(dataConnection, objQuerySheet.CreatedBy, "Create", "Query Sheet", id.ToString(), "0");
                    txn.Commit();

                    return id + "|" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
            }
        }

        //        public QuerySheet GetQuerySheet(int QuerySheetId)
        //        {

        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                string query;
        //                IDbTransaction txn = connection.BeginTransaction();

        //                query = @"select * from QuerySheet
        //                        where QuerySheetId=@QuerySheetId";

        //                var model = connection.Query<QuerySheet>(query, new
        //                {
        //                    QuerySheetId = QuerySheetId
        //                }, txn).First<QuerySheet>();

        //                try
        //                {
        //                    query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId
        //                            DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId
        //                            DELETE FROM QuerySheet WHERE QuerySheetId = @QuerySheetId";
        //                    connection.Execute(query, new { QuerySheetId = QuerySheetId }, txn);
        //                    txn.Rollback();

        //                    model.isUsed = false;
        //                    return model;
        //                }
        //                catch
        //                {
        //                    txn.Rollback();
        //                    model.isUsed = true;
        //                }
        //                return model;
        //            }
        //        }

        public IList<QuerySheet> GetQuerySheets(string Type, string querysheet, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from QuerySheet
                                where isActive=1 and OrganizationId = @OrganizationId
	                            and QuerySheetDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
	                            AND QuerySheetRefNo LIKE '%'+@querysheet+'%' 
                                ORDER BY QuerySheetDate DESC, CreatedDate DESC";
                return connection.Query<QuerySheet>(sql, new { Type = Type, OrganizationId = OrganizationId, querysheet = querysheet, to = to, from = from }).ToList();

            }
        }

        public IEnumerable<ProjectCost> GetProjectCostingParameter()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select CostingId,Description,''Remarks,0.00 Amount from CostingParameters WHERE ISNULL(isActive, 1) = 1";

                return connection.Query<ProjectCost>(query);
            }
        }

        public int CHECK(int QuerySheetId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT Count(Q.QuerySheetId)Count FROM QuerySheet Q
                                INNER JOIN SalesQuotation S ON S.QuerySheetId=Q.QuerySheetId
                                WHERE Q.QuerySheetId=@QuerySheetId";

                var id = connection.Query<int>(sql, new { QuerySheetId = QuerySheetId }).FirstOrDefault();

                return id;

            }

        }

        /// <summary>
        /// Delete QuerySheet Details
        /// </summary>
        /// <returns></returns>
        public string DeleteQuerySheet(int Id, string CreatedBy, int OrganizationId, string type)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    //string query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId;
                    //            DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId;
                    //            DELETE FROM QuerySheet OUTPUT deleted.QuerySheetRefNo WHERE QuerySheetId = @QuerySheetId;";
                    string query;

                    if (type == "Costing")
                    {
                        query = @"DELETE FROM ProjectCosting WHERE QuerySheetId = @id;
                                UPDATE QuerySheet SET Type = 'Unit', CostingAmount = NULL WHERE QuerySheetId = @id;
                                SELECT QuerySheetRefNo FROM QuerySheet WHERE QuerySheetId = @id;";
                    }
                    else if (type == "Unit")
                        query = @"DELETE FROM QuerySheetItemUnit WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM QuerySheetItem WHERE QuerySheetId = @id);
                                DELETE FROM QuerySheetItemDoor WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM QuerySheetItem WHERE QuerySheetId = @id);
                                UPDATE QuerySheet SET Type = 'RoomDetails' WHERE QuerySheetId = @id;
                                SELECT QuerySheetRefNo FROM QuerySheet WHERE QuerySheetId = @id;";
                    else
                        query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @id;
                                DELETE FROM QuerySheet OUTPUT deleted.QuerySheetRefNo WHERE QuerySheetId = @id";

                    string ref_no = connection.Query<string>(query, new { id = Id }, txn).First();
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", typeof(QuerySheet).Name, Id.ToString(), OrganizationId.ToString());
                    txn.Commit();
                    return ref_no;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Delete ProjectCosting DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteProjectCosting(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM ProjectCosting WHERE QuerySheetId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        public string UpdateQuerySheet(QuerySheet model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {

                    string query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId";

                    //DELETE FROM QuerySheetItemUnit U inner join QuerySheetItem I on U.QuerySheetItemId=I.QuerySheetItemId WHERE I.QuerySheetId = @QuerySheetId
                    //DELETE FROM QuerySheetItemDoor D inner join QuerySheetItem I on D.QuerySheetItemId=I.QuerySheetItemId WHERE I.QuerySheetId = @QuerySheetId
                    //DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId

                    connection.Execute(query, new { QuerySheetId = model.QuerySheetId }, txn);

                    //var row = 0;
                    //foreach (ProjectCost item in model.Items)
                    //{
                    //    item.QuerySheetId = model.QuerySheetId;
                    //    new ProjectCostRepository().InsertProjectCosting(item, connection, txn);
                    //}

                    foreach (QuerySheetItem item in model.QuerySheetItems)
                    {
                        item.QuerySheetId = model.QuerySheetId;
                        new ProjectCostRepository().InsertQuerySheetItem(item, connection, txn);
                    }
                    //                    foreach (QuerySheetItem items in model.QuerySheetItems)
                    //                    {
                    //                        items.QuerySheetId = model.QuerySheetId;
                    //                        new ProjectCostRepository().InsertQuerySheetItem(items, connection, txn);

                    //                        foreach (QuerySheetUnit item in items.ProjectRoomUnits)
                    //                        {
                    //                            item.QuerySheetItemId = items.QuerySheetItemId;

                    //                            query = @"insert  into QuerySheetItemUnit(QuerySheetItemId,EvaporatorUnitId,CondenserUnitId,Quantity) 
                    //                                       Values (@QuerySheetItemId,@EvaporatorUnitId,@CondenserUnitId,@Quantity)";

                    //                            row = connection.Execute(query, item, txn);

                    //                        }
                    //                        foreach (QuerySheetDoor item in items.ProjectRoomDoors)
                    //                        {
                    //                            item.QuerySheetItemId = items.QuerySheetItemId;
                    //                            query = @"insert  into QuerySheetItemDoor(QuerySheetItemId,DoorId,Quantity) 
                    //                                       Values (@QuerySheetItemId,@DoorId,@Quantity)";

                    //                            row = connection.Execute(query, item, txn);

                    //                        }
                    //                    }

                    query = @"UPDATE QuerySheet SET  QuerySheetRefNo = @QuerySheetRefNo,QuerySheetDate = @QuerySheetDate,ProjectName = @ProjectName,ContactPerson = @ContactPerson,ContactNumber = @ContactNumber,
	   	                      Email = @Email
	                          OUTPUT inserted.QuerySheetRefNo
                              WHERE QuerySheetId = @QuerySheetId";
                    string ref_no = connection.Query<string>(query, model, txn).First();

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Update", typeof(QuerySheet).Name, model.QuerySheetId.ToString(), model.OrganizationId.ToString());

                    txn.Commit();
                    return ref_no;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public string UpdateQuerySheetUnitSelection(QuerySheet model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = @"DELETE FROM QuerySheetItemUnit WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId);
                                   DELETE FROM QuerySheetItemDoor WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId);";
                    var count = connection.Execute(sql, new { QuerySheetId = model.QuerySheetId }, txn);
                    UpdateQuerySheetUnit(connection, model, txn);
                    //txn.Commit();
                    return model.QuerySheetRefNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public int UpdateQuerySheetUnit(QuerySheet model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    return UpdateQuerySheetUnit(connection, model, txn);
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        private int UpdateQuerySheetUnit(IDbConnection connection, QuerySheet model, IDbTransaction txn)
        {
            string sql = "";
            //                  
            var row = 0;

            foreach (QuerySheetItem item in model.QuerySheetItems)
            {

                sql = @"UPDATE QuerySheetItem SET Kilowatt = @Kilowatt,Cost = @Cost WHERE QuerySheetItemId = @QuerySheetItemId";
                row = connection.Execute(sql, item, txn);

            }
            sql = @"UPDATE QuerySheet  SET Type=@Type WHERE QuerySheetId = @QuerySheetId";
            row = connection.Execute(sql, model, txn);
            foreach (QuerySheetItem items in model.QuerySheetItems)
            {
                foreach (QuerySheetUnit item in items.ProjectRoomUnits)
                {

                    item.QuerySheetItemId = items.QuerySheetItemId;

                    sql = @"insert  into QuerySheetItemUnit(QuerySheetItemId,EvaporatorUnitId,CondenserUnitId,Quantity) 
                                       Values (@QuerySheetItemId,@EvaporatorUnitId,@CondenserUnitId,@Quantity)";

                    row = connection.Execute(sql, item, txn);

                }
                foreach (QuerySheetDoor item in items.ProjectRoomDoors)
                {
                    if (item.DoorId == "") continue;
                    item.QuerySheetItemId = items.QuerySheetItemId;
                    sql = @"insert  into QuerySheetItemDoor(QuerySheetItemId,DoorId,Quantity) 
                                       Values (@QuerySheetItemId,@DoorId,@Quantity)";

                    row = connection.Execute(sql, item, txn);

                }
            }
            InsertLoginHistory(dataConnection, model.CreatedBy, "Update", typeof(QuerySheet).Name, model.QuerySheetId.ToString(), model.OrganizationId.ToString());
            txn.Commit();
            return row;
        }

        public List<QuerySheet> GetPendingQuerySheetforUnit()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = String.Format(@"select * from QuerySheet where Type='RoomDetails'");


                var objQuerySheet = connection.Query<QuerySheet>(sql).ToList<QuerySheet>();

                return objQuerySheet;
            }
        }
        public List<QuerySheet> GetPendingQuerySheetforCosting()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = String.Format(@"select * from QuerySheet where Type='Unit'");


                var objQuerySheet = connection.Query<QuerySheet>(sql).ToList<QuerySheet>();

                return objQuerySheet;
            }
        }


        public decimal GetCostingAmount(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"SELECT CostingAmount FROM QuerySheet WHERE QuerySheetId = @id";
                    return connection.Query<decimal>(sql, new { id = id }).ToList<decimal>().First();
                }
                catch (InvalidOperationException)
                {
                    return 0;
                }
            }
        }


        public List<QuerySheetUnit> GetQuerySheetItemUnit(int QuerySheetId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT * from QuerySheetItemUnit Q INNER JOIN QuerySheetItem QI ON Q.QuerySheetItemId=QI.QuerySheetItemId
                                  WHERE QI.QuerySheetId = @QuerySheetId";
                return connection.Query<QuerySheetUnit>(sql, new { QuerySheetId = QuerySheetId }).ToList();
            }
        }
        public List<QuerySheetDoor> GetQuerySheetItemDoor(int QuerySheetId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT * from QuerySheetItemDoor Q INNER JOIN QuerySheetItem QI ON Q.QuerySheetItemId=QI.QuerySheetItemId
                                  WHERE QI.QuerySheetId = @QuerySheetId";
                return connection.Query<QuerySheetDoor>(sql, new { QuerySheetId = QuerySheetId }).ToList();
            }
        }

        public QuerySheet GetQuerySheetItem(int querySheetId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                QuerySheet model = new QuerySheet();

                string query = @"select * from QuerySheet
                                 where QuerySheetId=@id

                                 SELECT *  FROM QuerySheetItem   WHERE QuerySheetId = @id
                              
                                SELECT QuerySheetItemId INTO #Rooms FROM QuerySheetItem WHERE QuerySheetId = @id

                                SELECT
	                                *
                                FROM QuerySheetItemUnit
                                WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #Rooms)

                                SELECT
	                                *
                                FROM QuerySheetItemDoor
                                WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #Rooms)

                                DROP TABLE #Rooms;";

                using (var dataset = connection.QueryMultiple(query, new { id = querySheetId }))
                {
                    model = dataset.Read<QuerySheet>().First();
                    model.QuerySheetItems = dataset.Read<QuerySheetItem>().AsList();
                    List<QuerySheetUnit> units = dataset.Read<QuerySheetUnit>().AsList();
                    List<QuerySheetDoor> doors = dataset.Read<QuerySheetDoor>().AsList();
                    foreach (var item in model.QuerySheetItems)
                    {
                        item.ProjectRoomUnits = units.Where(x => x.QuerySheetItemId == item.QuerySheetItemId).Select(x => x).ToList();
                        item.ProjectRoomDoors = doors.Where(x => x.QuerySheetItemId == item.QuerySheetItemId).Select(x => x).ToList();
                    }
                }
                return model;
            }
        }
    }
}