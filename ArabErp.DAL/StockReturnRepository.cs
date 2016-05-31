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
                return id;
            }
        }

        public int DeleteStockReturn(Unit objStockReturn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockReturn  OUTPUT DELETED.StockReturnId WHERE StockReturnId=@StockReturnId";


                var id = connection.Execute(sql, objStockReturn);
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
        /// Returns all materials that were requested for a particular job card
        /// </summary>
        /// <param name="jobCardId"></param>
        /// <returns></returns>
        public List<Dropdown> FillProduct(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT I.ItemId Id, I.ItemName Name FROM WorkVsItem WI INNER JOIN WorkDescription WD ON WI.WorkVsItemId = WD.WorkDescriptionId INNER JOIN SaleOrderItem SOI ON WD.WorkDescriptionId = SOI.WorkDescriptionId INNER JOIN JobCard JC ON JC.SaleOrderId = SOI.SaleOrderId INNER JOIN Item I ON I.ItemId = WI.ItemId WHERE JC.JobCardId = @JobCardId";
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
                string query = "SELECT C.CustomerName+'|'+J.WorkDescription FROM JobCard J INNER JOIN SaleOrder S ON J.SaleOrderId = S.SaleOrderId INNER JOIN Customer C ON S.CustomerId = C.CustomerId WHERE J.JobCardId = @JobCardId";
                return connection.Query<string>(query, 
                    new { JobCardId = id }).First<string>();
            }
        } 
    }
}
