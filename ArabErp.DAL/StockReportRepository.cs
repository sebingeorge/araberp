using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockReportRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<StockReportSummary> GetStockReport()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " with StockIn as (";
                sql += " select ItemId, SUM(Quantity) InQuantity from StockUpdate where StockInOut = 'IN'";
                sql += " group by ItemId),";
                sql += " StockOut as (";
                sql += " select ItemId, 0- SUM(Quantity) OutQuantity from StockUpdate where StockInOut = 'OUT'";
                sql += " group by ItemId)";
                sql += " select I.ItemId, I.ItemName, SI.InQuantity, SO.OutQuantity, Balance = ISNULL(SI.InQuantity,0) - ISNULL(SO.OutQuantity,0)";
                sql += " from Item I";
                sql += " left join StockIn SI on I.ItemId = SI.ItemId";
                sql += " left join StockOut SO on I.ItemId = SO.ItemId";
                sql += " where SI.InQuantity is not null and SO.OutQuantity is not null";

                return connection.Query<StockReportSummary>(sql);
            }
        }
        public IEnumerable<StockSummaryDrillDown> GetStockReportItemWise(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select I.ItemName, StockType, ";
                sql += " TrnNo = case when StockType = 'GRN' then (select top 1 GRNNo + ' - ' + convert(varchar(50), GRNDate, 106) from GRN where GRNId = S.StocktrnId)";
                sql += " when StockType = 'DirectGRN' then (select top 1 GRNNo + ' - ' + convert(varchar(50), GRNDate, 106) from GRN where GRNId = S.StocktrnId)";
                sql += " when StockType = 'StoreIssue' then (select top 1 StoreIssueRefNo + ' - ' + convert(varchar(50), StoreIssueDate, 106) from StoreIssue where StoreIssueId = S.StocktrnId)";
                sql += " when StockType = 'StockJournal' then (select top 1 StockJournalRefno + ' - ' + convert(varchar(50), StockJournalDate, 106) from StockJournal where StockJournalId = S.StocktrnId)";
                sql += " else '' end,";
                sql += " INQty = case when S.StockinOut = 'IN' then Quantity else 0 end,";
                sql += " OUTQty = case when S.StockInOut = 'OUT' then 0-Quantity else 0 end";
                sql += " from ";
                sql += " StockUpdate S";
                sql += " inner join Item I on S.ItemId = I.ItemId";
                sql += " where S.ItemId = " + itemId.ToString();

                return connection.Query<StockSummaryDrillDown>(sql);
            }
        }
        public IEnumerable<StockReportSummary> GetStockReportDTPrint()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " with StockIn as (";
                sql += " select ItemId, SUM(Quantity) InQuantity from StockUpdate where StockInOut = 'IN'";
                sql += " group by ItemId),";
                sql += " StockOut as (";
                sql += " select ItemId, 0- SUM(Quantity) OutQuantity from StockUpdate where StockInOut = 'OUT'";
                sql += " group by ItemId)";
                sql += " select I.ItemId, I.ItemName, SI.InQuantity, SO.OutQuantity, Balance = ISNULL(SI.InQuantity,0) - ISNULL(SO.OutQuantity,0)";
                sql += " from Item I";
                sql += " left join StockIn SI on I.ItemId = SI.ItemId";
                sql += " left join StockOut SO on I.ItemId = SO.ItemId";
                sql += " where SI.InQuantity is not null or SO.OutQuantity is not null and I.ItemGroupId not in (1) and i.ItemCategoryId not in(6)";

                return connection.Query<StockReportSummary>(sql);
            }
        }
    }
}
