using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class GRNItemRepository : BaseRepository
    {

        public int InsertGRNItem(GRNItem objGRNItem)
        {
            string sql = @"insert  into GRNItem(GRNId,SONoAndDate,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,Rate,Discount,Amount,CreatedBy,CreatedDate,OrganizationId) Values (@GRNId,@SONoAndDate,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objGRNItem).Single();
            return id;
        }


        public GRNItem GetGRNItem(int GRNItemId)
        {

            string sql = @"select * from GRNItem
                        where GRNItemId=@GRNItemId";

            var objGRNItem = connection.Query<GRNItem>(sql, new
            {
                GRNItemId = GRNItemId
            }).First<GRNItem>();

            return objGRNItem;
        }

        public List<GRNItem> GetGRNItems()
        {
            string sql = @"select * from GRNItem
                        where isActive=1";

            var objGRNItems = connection.Query<GRNItem>(sql).ToList<GRNItem>();

            return objGRNItems;
        }

        public int UpdateGRNItem(GRNItem objGRNItem)
        {
            string sql = @"UPDATE GRNItem SET GRNId = @GRNId ,SONoAndDate = @SONoAndDate ,SlNo = @SlNo ,ItemId = @ItemId,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,Rate = @Rate  OUTPUT INSERTED.GRNItemId  WHERE GRNItemId = @GRNItemId";


            var id = connection.Execute(sql, objGRNItem);
            return id;
        }

        public int DeleteGRNItem(Unit objGRNItem)
        {
            string sql = @"Delete GRNItem  OUTPUT DELETED.GRNItemId WHERE GRNItemId=@GRNItemId";


            var id = connection.Execute(sql, objGRNItem);
            return id;
        }


    }
}