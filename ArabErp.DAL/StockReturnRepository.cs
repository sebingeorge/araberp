using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockReturnRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Insert into stock return head table. (StockReturn table)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertStockReturn(StockReturn model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.StockReturnRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 21, true,txn);
                    string sql = @"insert  into StockReturn(
                            StockReturnRefNo,
                            StockReturnDate,
                            JobCardId,
                            SpecialRemarks,
                            CreatedBy,
                            CreatedDate,
                            OrganizationId) Values

                            (@StockReturnRefNo,
                            @StockReturnDate,
                            @JobCardId,
                            @SpecialRemarks,
                            @CreatedBy,
                            @CreatedDate,
                            @OrganizationId);

                            SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, model, txn).Single();
                    foreach (var item in model.Items)
                    {
                        item.StockReturnId = id;
                        new StockReturnItemRepository().InsertStockReturnItem(item, connection, txn);
                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Stock Return", id.ToString(), "0");
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return 0;
                }
            }
        }

        public StockReturn GetStockReturn(int StockReturnId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturn
                        where StockReturnId=@StockReturnId";

                var objStockReturn = connection.Query<StockReturn>(sql, new
                {
                    StockReturnId = StockReturnId
                }).First<StockReturn>();

                return objStockReturn;
            }
        }

        public List<StockReturn> GetStockReturns()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturn
                        where isActive=1";

                var objStockReturns = connection.Query<StockReturn>(sql).ToList<StockReturn>();

                return objStockReturns;
            }
        }

        public int UpdateStockReturn(StockReturn objStockReturn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE StockReturn SET StockReturnDate = @StockReturnDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.StockReturnId  WHERE StockReturnId = @StockReturnId";

                var id = connection.Execute(sql, objStockReturn);
                InsertLoginHistory(dataConnection, objStockReturn.CreatedBy, "Update", "Stock Return", id.ToString(), "0");
                return id;
            }
        }

        public int DeleteStockReturn(Unit objStockReturn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockReturn  OUTPUT DELETED.StockReturnId WHERE StockReturnId=@StockReturnId";
                var id = connection.Execute(sql, objStockReturn);
                InsertLoginHistory(dataConnection, objStockReturn.CreatedBy, "Delete", "Stock Return", id.ToString(), "0");
                return id;
            }
        }
        /// <summary>
        /// Returns all incomplete job cards
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> FillJobCard()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT JobCardId Id, JobCardNo Name FROM JobCard").ToList();
            }
        }
        /// <summary>
        /// Returns all materials that were requested for a particular job card (contains items in additional request also)
        /// </summary>
        /// <param name="jobCardId"></param>
        /// <returns></returns>
        public List<Dropdown> FillProduct(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT ItemId Id, ItemName Name
                                FROM Item WHERE ItemId IN(
                                SELECT 
	                                DISTINCT ItemId 
                                FROM WorkVsItem WI 
                                INNER JOIN JobCard J ON WI.WorkDescriptionId = J.WorkDescriptionId 
                                WHERE J.JobCardId = @jobCardId
                                UNION
                                SELECT WRI.ItemId
                                FROM WorkShopRequestItem WRI
                                INNER JOIN WorkShopRequest WR ON WRI.WorkShopRequestId = WR.WorkShopRequestId
                                WHERE JobCardId = @jobCardId
                                )";
                return connection.Query<Dropdown>(query,
                    new { JobCardId = jobCardId }).ToList();
            }
        }
        /// <summary>
        /// Get details of a job card such as customer name, work description ...
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetJobCardDetails(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT C.CustomerName+'|'+W.WorkDescr FROM JobCard J 
                                INNER JOIN SaleOrder S ON J.SaleOrderId = S.SaleOrderId 
                                INNER JOIN Customer C ON S.CustomerId = C.CustomerId 
                                INNER JOIN WorkDescription W ON J.WorkDescriptionId = W.WorkDescriptionId
                                WHERE J.JobCardId = @JobCardId";
                return connection.Query<string>(query,
                    new { JobCardId = id }).First<string>();
            }
        }

        public IEnumerable<StockReturn> PreviousList(int OrganizationId, DateTime? from, DateTime? to, int id, int jobcard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SR.StockReturnId,
	                                SR.StockReturnRefNo,
	                                CONVERT(VARCHAR, SR.StockReturnDate, 106) StockReturnDate,
	                                JC.JobCardNo,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                ISNULL(SR.SpecialRemarks, '-') SpecialRemarks,

	                                STUFF((SELECT ', ' + CAST(T2.ItemName + ' ('+CAST(T1.Quantity AS VARCHAR)+')' AS VARCHAR) [text()]
	                                FROM StockReturnItem T1 INNER JOIN Item T2 ON T1.ItemId = T2.ItemId
	                                WHERE SR.StockReturnId = T1.StockReturnId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') ItemName

                                FROM StockReturn SR
	                                INNER JOIN JobCard JC ON SR.JobCardId = JC.JobCardId
                                WHERE SR.OrganizationId = @OrganizationId
                                    AND SR.isActive = 1
                                    AND CONVERT(DATE, SR.StockReturnDate, 106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                    AND SR.StockReturnId = ISNULL(NULLIF(CAST(@id AS INT), 0), SR.StockReturnId)
                                    AND SR.JobCardId = ISNULL(NULLIF(CAST(@jobcard AS INT), 0), SR.JobCardId)";
                return connection.Query<StockReturn>(query, new
                                                            {
                                                                OrganizationId = OrganizationId,
                                                                from = from,
                                                                to = to,
                                                                id = id,
                                                                jobcard = jobcard
                                                            }).ToList();
            }
        }

        public StockReturn GetStockReturnHD(int StockReturnId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT StockReturnRefNo,StockReturnDate,S.JobCardId,C.CustomerName,W.WorkDescr,S.SpecialRemarks 
                                FROM StockReturn S
                                INNER JOIN JobCard J ON J.JobCardId=S.JobCardId
                                INNER JOIN SaleOrder SO ON SO.SaleOrderId=J.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=SO.CustomerId
                                INNER JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
                                WHERE  StockReturnId=@StockReturnId";

                var objStockReturn = connection.Query<StockReturn>(sql, new
                {
                    StockReturnId = StockReturnId
                }).First<StockReturn>();

                return objStockReturn;
            }
        }
    }
}
