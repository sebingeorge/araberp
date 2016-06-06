using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class GRNItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertGRNItem(GRNItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

            string sql = @"insert  into GRNItem(GRNId,SONoAndDate,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,Rate,Discount,Amount) Values (@GRNId,@SONoAndDate,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }

      
        public GRNItem GetGRNItem(int GRNItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from GRNItem
                        where GRNItemId=@GRNItemId";

                var objGRNItem = connection.Query<GRNItem>(sql, new
                {
                    GRNItemId = GRNItemId
                }).First<GRNItem>();

                return objGRNItem;
            }
        }

        public List<GRNItem> GetGRNItems()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from GRNItem
                        where isActive=1";

                var objGRNItems = connection.Query<GRNItem>(sql).ToList<GRNItem>();

                return objGRNItems;
            }
        }

        public int UpdateGRNItem(GRNItem objGRNItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE GRNItem SET GRNId = @GRNId ,SONoAndDate = @SONoAndDate ,SlNo = @SlNo ,ItemId = @ItemId,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,Rate = @Rate  OUTPUT INSERTED.GRNItemId  WHERE GRNItemId = @GRNItemId";


                var id = connection.Execute(sql, objGRNItem);
                return id;
            }
        }

        public int DeleteGRNItem(Unit objGRNItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete GRNItem  OUTPUT DELETED.GRNItemId WHERE GRNItemId=@GRNItemId";


                var id = connection.Execute(sql, objGRNItem);
                return id;
            }
        }


    }
}