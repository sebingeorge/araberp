using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class StockReturnItemRepository : BaseRepository
    {

        public int InsertStockReturnItem(StockReturnItem objStockReturnItem)
        {
            string sql = @"insert  into StockReturnItem(StockReturnId,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,CreatedBy,CreatedDate,OrganizationId,CreatedDate,OrganizationId) Values (@StockReturnId,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@CreatedBy,@CreatedDate,@OrganizationId,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objStockReturnItem).Single();
            return id;
        }


        public StockReturnItem GetStockReturnItem(int StockReturnItemId)
        {

            string sql = @"select * from StockReturnItem
                        where StockReturnItemId=@StockReturnItemId";

            var objStockReturnItem = connection.Query<StockReturnItem>(sql, new
            {
                StockReturnItemId = StockReturnItemId
            }).First<StockReturnItem>();

            return objStockReturnItem;
        }

        public List<StockReturnItem> GetStockReturnItems()
        {
            string sql = @"select * from StockReturnItem
                        where isActive=1";

            var objStockReturnItems = connection.Query<StockReturnItem>(sql).ToList<StockReturnItem>();

            return objStockReturnItems;
        }

        public int UpdateStockReturnItem(StockReturnItem objStockReturnItem)
        {
            string sql = @"UPDATE StockReturnItem SET StockReturnId = @StockReturnId ,SlNo = @SlNo ,ItemId = @ItemId ,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.StockReturnItemId  WHERE StockReturnItemId = @StockReturnItemId";


            var id = connection.Execute(sql, objStockReturnItem);
            return id;
        }

        public int DeleteStockReturnItem(Unit objStockReturnItem)
        {
            string sql = @"Delete StockReturnItem  OUTPUT DELETED.StockReturnItemId WHERE StockReturnItemId=@StockReturnItemId";


            var id = connection.Execute(sql, objStockReturnItem);
            return id;
        }


    }
}