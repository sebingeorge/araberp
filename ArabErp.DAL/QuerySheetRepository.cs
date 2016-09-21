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

                    string sql = @"insert  into QuerySheet(QuerySheetRefNo,QuerySheetDate,ProjectName,ContactPerson,ContactNumber,Email,CreatedBy,CreatedDate,OrganizationId)
                                 Values (@QuerySheetRefNo,@QuerySheetDate,@ProjectName,@ContactPerson,@ContactNumber,@Email,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objQuerySheet, txn).Single();

                    foreach (ProjectCost item in objQuerySheet.Items)
                    {
                        item.QuerySheetId = id;
                        new ProjectCostRepository().InsertProjectCosting(item, connection, txn);
                    }

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

        public QuerySheet GetQuerySheet(int QuerySheetId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query;
                IDbTransaction txn = connection.BeginTransaction();

                query = @"select * from QuerySheet
                        where QuerySheetId=@QuerySheetId";

                var model = connection.Query<QuerySheet>(query, new
                {
                    QuerySheetId = QuerySheetId
                }, txn).First<QuerySheet>();

                try
                {
                    query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId
                            DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId
                            DELETE FROM QuerySheet WHERE QuerySheetId = @QuerySheetId";
                    connection.Execute(query, new { QuerySheetId = QuerySheetId }, txn);
                    txn.Rollback();

                    model.isUsed = false;
                    return model;
                }
                catch
                {
                    txn.Rollback();
                    model.isUsed = true;
                }
                return model;
            }
        }

        public IList<QuerySheet> GetQuerySheets(string querysheet, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from QuerySheet
                                where isActive=1 and OrganizationId = @OrganizationId
	                                and QuerySheetDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
	                                AND QuerySheetRefNo LIKE '%'+@querysheet+'%'
                                    ORDER BY QuerySheetDate DESC, CreatedDate DESC";
                return connection.Query<QuerySheet>(sql, new { OrganizationId = OrganizationId, querysheet = querysheet, to = to, from = from }).ToList();

            }
        }

        public IEnumerable<ProjectCost> GetProjectCostingParameter()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select CostingId,Description,''Remarks,0.00 Amount from CostingParameters";

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
        public string DeleteQuerySheet(int Id, string CreatedBy, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId;
                                DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId;
                                DELETE FROM QuerySheet OUTPUT deleted.QuerySheetRefNo WHERE QuerySheetId = @QuerySheetId;";

                    string ref_no = connection.Query<string>(query, new { QuerySheetId = Id }, txn).First();
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
                    string query = @"DELETE FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId
                                DELETE FROM ProjectCosting WHERE QuerySheetId = @QuerySheetId";

                    connection.Execute(query, new { QuerySheetId = model.QuerySheetId }, txn);

                    foreach (ProjectCost item in model.Items)
                    {
                        item.QuerySheetId = model.QuerySheetId;
                        new ProjectCostRepository().InsertProjectCosting(item, connection, txn);
                    }

                    foreach (QuerySheetItem item in model.QuerySheetItems)
                    {
                        item.QuerySheetId = model.QuerySheetId;
                        new ProjectCostRepository().InsertQuerySheetItem(item, connection, txn);
                    }

                    query = @"UPDATE QuerySheet SET
	                            QuerySheetRefNo = @QuerySheetRefNo,
	                            QuerySheetDate = @QuerySheetDate,
	                            ProjectName = @ProjectName,
	                            ContactPerson = @ContactPerson,
	                            ContactNumber = @ContactNumber,
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
    }
}