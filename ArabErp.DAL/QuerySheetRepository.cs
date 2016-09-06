﻿using System;
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
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objQuerySheet.OrganizationId, 5, true,txn);
                    objQuerySheet.QuerySheetRefNo= internalId;

                    string sql = @"insert  into QuerySheet(QuerySheetRefNo,QuerySheetDate,ProjectName,ContactPerson,ContactNumber,Email,CreatedBy,CreatedDate,OrganizationId)
                                 Values (@QuerySheetRefNo,@QuerySheetDate,@ProjectName,@ContactPerson,@ContactNumber,@Email,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objQuerySheet, txn).Single();

                    foreach (ProjectCost item in objQuerySheet.Items)
                    {
                        item.QuerySheetId= id;
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
                string sql = @"select * from QuerySheet
                        where QuerySheetId=@QuerySheetId";

                var objQuerySheet = connection.Query<QuerySheet>(sql, new
                {
                    QuerySheetId = QuerySheetId
                }).First<QuerySheet>();

                return objQuerySheet;
            }
        }



        public IList<QuerySheet> GetQuerySheets(int id, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = " select * from QuerySheet";
                sql += " where isActive=1 and OrganizationId = @OrganizationId ";
                sql += " and QuerySheetDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) ";
                sql += " and QuerySheetId = ISNULL(NULLIF(@id, 0),QuerySheetId)";
                return connection.Query<QuerySheet>(sql, new { OrganizationId = OrganizationId, id = id, to = to, from = from }).ToList();

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
        public int DeleteQuerySheet(int Id, string CreatedBy)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM QuerySheet WHERE QuerySheetId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", "Query Sheet", id.ToString(), "0");
                    return id;

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

    }
}