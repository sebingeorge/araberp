using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkShopGRNItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertWorkShopGRNItem(WorkShopGRNItem objWorkShopGRNItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO WorkShopGRNItem (insert  into WorkShopGRN(WorkShopGRNId,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,Rate,Discount,Amount,Remarks) Values (@WorkShopGRNId,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount,@Remarks);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkShopGRNItem).Single();
                return id;
            }
        }


        public WorkShopGRNItem GetWorkShopGRNItem(int WorkShopGRNItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRNItem
                        where WorkShopGRNItemId=@WorkShopGRNItemId";

                var objWorkShopGRNItem = connection.Query<WorkShopGRNItem>(sql, new
                {
                    WorkShopGRNItemId = WorkShopGRNItemId
                }).First<WorkShopGRNItem>();

                return objWorkShopGRNItem;
            }
        }

        public List<WorkShopGRNItem> GetWorkShopGRNItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRNItem
                        where isActive=1";

                var objWorkShopGRNItems = connection.Query<WorkShopGRNItem>(sql).ToList<WorkShopGRNItem>();

                return objWorkShopGRNItems;
            }
        }



        public int DeleteWorkShopGRNItem(Unit objWorkShopGRNItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkShopGRNItem  OUTPUT DELETED.WorkShopGRNItemId WHERE WorkShopGRNItemId=@WorkShopGRNItemId";


                var id = connection.Execute(sql, objWorkShopGRNItem);
                return id;
            }
        }


    }
}
