using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Collections;

namespace ArabErp.DAL
{
    public class StockTransferRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public string ConnectionString()
        {
            return dataConnection;
        }

        public string CreateStockTransfer(StockTransfer model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.StockTransferRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 29, true, txn);

                    int id = Create(model, connection, txn);

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "StockTransfer", id.ToString(), model.OrganizationId.ToString());
                    txn.Commit();
                    return model.StockTransferRefNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        private int Create(StockTransfer model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                #region Inserting into [StockTransfer] head
                string query = @"INSERT INTO StockTransfer
                                (
	                                StockTransferRefNo,
	                                StockTransferDate,
	                                FromStockpointId,
	                                ToStockpointId,
	                                CreatedBy,
	                                CreatedDate,
	                                OrganizationId,
	                                isActive
                                )
                                VALUES
                                (
	                                @StockTransferRefNo,
	                                @StockTransferDate,
	                                @FromStockpointId,
	                                @ToStockpointId,
	                                @CreatedBy,
	                                @CreatedDate,
	                                @OrganizationId,
	                                1
                                );
                                SELECT CAST(SCOPE_IDENTITY() AS INT)";

                int id = connection.Query<int>(query, model, txn).First();
                #endregion

                #region Inserting Transfer Items + Stock Updation
                foreach (var item in model.Items)
                {
                    item.StockTransferId = id;
                    var i = new StockTransferItemRepository().InsertTransferItems(item, connection, txn);
                    new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                    {
                        OrganizationId = model.OrganizationId,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate,
                        StockPointId = model.FromStockpointId,
                        StockType = "StockTransfer",
                        StockInOut = "OUT",
                        stocktrnDate = System.DateTime.Today,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity * (-1),
                        StocktrnId = id,
                        StockUserId = model.StockTransferRefNo
                    }, connection, txn);

                    new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                    {
                        OrganizationId = model.OrganizationId,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate,
                        StockPointId = model.ToStockpointId,
                        StockType = "StockTransfer",
                        StockInOut = "IN",
                        stocktrnDate = System.DateTime.Today,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        StocktrnId = id,
                        StockUserId = model.StockTransferRefNo
                    }, connection, txn);
                }
                #endregion

                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable PreviousList(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                            ST.StockTransferId,
	                            StockTransferRefNo,
	                            CONVERT(VARCHAR, StockTransferDate, 106) StockTransferDate,
	                            S1.StockPointName FromStockpointName,
	                            S2.StockPointName ToStockpointName,
	                            U.UserName CreatedBy,
	                            I.ItemName,
	                            STI.Quantity
                            FROM StockTransfer ST
	                            INNER JOIN Stockpoint S1 ON ST.FromStockpointId = S1.StockPointId
	                            INNER JOIN Stockpoint S2 ON ST.ToStockpointId = S2.StockPointId
	                            LEFT JOIN [User] U ON ST.CreatedBy = U.UserId
	                            INNER JOIN StockTransferItem STI ON ST.StockTransferId = STI.StockTransferId
	                            INNER JOIN Item I ON STI.ItemId = I.ItemId
                            WHERE ST.isActive = 1 AND STI.isActive = 1
	                            AND ST.OrganizationId = @OrganizationId
                            ORDER BY ST.StockTransferDate DESC, ST.CreatedDate DESC";
                return connection.Query<StockTransfer>(query, new { OrganizationId = OrganizationId }).ToList();
            }
        }
    }
}
