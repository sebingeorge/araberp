using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class StockCreationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public string CreateStock(StockCreation model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.StockCreationRefNo = "STO/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(StockCreation).Name, "0", 1);

                    string query = @"INSERT INTO StockCreation
                                (
	                                StockCreationRefNo,
	                                StockCreationDate,
                                    CreatedBy,
                                    CreatedDate,
                                    OrganizationId
                                )
                                VALUES
                                (
                                    @StockCreationRefNo,
	                                @StockCreationDate,
                                    @CreatedBy,
                                    @CreatedDate,
                                    @OrganizationId
                                );
                                SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    int id = connection.Query<int>(query, model, txn).First();

                    #region Inserting Finished Goods + Stock Updation
                    foreach (var item in model.FinishedGoods)
                    {
                        item.StockCreationId = id;
                        var i = new StockCreationFinishedGoodsRepository().InsertFinishedGoods(item, connection, txn);
                        new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                        {
                            OrganizationId = model.OrganizationId,
                            CreatedBy = model.CreatedBy,
                            CreatedDate = model.CreatedDate,
                            StockPointId = model.FinishedGoodStockpointId,
                            StockType = "StockCreation",
                            StockInOut = "IN",
                            stocktrnDate = System.DateTime.Today,
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            StocktrnId = id,
                            StockUserId = model.StockCreationRefNo
                        }, connection, txn);
                    }
                    #endregion

                    #region Inserting Consumed Items + Stock Updation
                    foreach (var item in model.ConsumedItems)
                    {
                        item.StockCreationId = id;
                        var i = new StockCreationConsumedItemsRepository().InsertConsumedItems(item, connection, txn);
                        new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                        {
                            OrganizationId = model.OrganizationId,
                            CreatedBy = model.CreatedBy,
                            CreatedDate = model.CreatedDate,
                            StockPointId = model.FinishedGoodStockpointId,
                            StockType = "StockCreation",
                            StockInOut = "OUT",
                            stocktrnDate = System.DateTime.Today,
                            ItemId = item.ItemId,
                            Quantity = item.Quantity * (-1),
                            StocktrnId = id,
                            StockUserId = model.StockCreationRefNo
                        }, connection, txn);
                    }
                    #endregion

                    txn.Commit();
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }

                return model.StockCreationRefNo;
            }
        }

        public IEnumerable<StockCreation> GetStockCreations()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SC.StockCreationId,
	                                SC.StockCreationRefNo,
									CONVERT(VARCHAR, SC.StockCreationDate, 106) StockCreationDate,
									SC.CreatedBy,

	                                STUFF((SELECT ', ' + CAST(I.ItemName+'('+CAST(T1.Quantity AS VARCHAR(MAX)) +')' AS VARCHAR(MAX)) [text()]
	                                FROM StockCreationFinishedGoods T1 INNER JOIN StockCreation T2 on T1.StockCreationId = T2.StockCreationId
	                                INNER JOIN Item I ON T1.ItemId = I.ItemId
	                                WHERE T1.StockCreationId = SC.StockCreationId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') Finished,

	                                STUFF((SELECT ', ' + CAST(I.ItemName+'('+CAST(T1.Quantity AS VARCHAR(MAX)) +')' AS VARCHAR(MAX)) [text()]
	                                FROM StockCreationConsumedItems T1 INNER JOIN StockCreation T2 on T1.StockCreationId = T2.StockCreationId
	                                INNER JOIN Item I ON T1.ItemId = I.ItemId
	                                WHERE T1.StockCreationId = SC.StockCreationId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') Consumed

                                FROM StockCreation SC
								ORDER BY SC.StockCreationDate DESC, SC.CreatedDate DESC";
                return connection.Query<StockCreation>(query).ToList();
            }
        }

        /// <summary>
        /// Get stock creation by id
        /// </summary>
        /// <param name="id">StockCreationId</param>
        /// <returns></returns>
        public IEnumerable<StockCreation> GetStockCreation(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT * FROM StockCreation WHERE StockCreationId = @id";
                return connection.Query<StockCreation>(query, new { id = id }).ToList();
            }
        }
    }
}
