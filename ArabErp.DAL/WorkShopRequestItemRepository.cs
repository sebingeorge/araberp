using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class WorkShopRequestItemRepository : BaseRepository
    {

        public int InsertWorkShopRequestItem(WorkShopRequestItem objWorkShopRequestItem)
        {
            string sql = @"INSERT INTO WorkShopRequestItem (WorkShopRequestItemRefNo,WorkShopRequestItemName,CreatedBy,CreatedDate,OrganizationId) VALUES(@WorkShopRequestItemRefNo,@WorkShopRequestItemName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objWorkShopRequestItem).Single();
            return id;
        }


        public WorkShopRequestItem GetWorkShopRequestItem(int WorkShopRequestItemId)
        {

            string sql = @"select * from WorkShopRequestItem
                        where WorkShopRequestItemId=@WorkShopRequestItemId";

            var objWorkShopRequestItem = connection.Query<WorkShopRequestItem>(sql, new
            {
                WorkShopRequestItemId = WorkShopRequestItemId
            }).First<WorkShopRequestItem>();

            return objWorkShopRequestItem;
        }

        public List<WorkShopRequestItem> GetWorkShopRequestItems()
        {
            string sql = @"select * from WorkShopRequestItem
                        where isActive=1";

            var objWorkShopRequestItems = connection.Query<WorkShopRequestItem>(sql).ToList<WorkShopRequestItem>();

            return objWorkShopRequestItems;
        }

        public int UpdateWorkShopRequestItem(WorkShopRequestItem objWorkShopRequestItem)
        {
            string sql = @"Update WorkShopRequestItem Set WorkShopRequestItemRefNo=@WorkShopRequestItemRefNo,WorkShopRequestItemName=@WorkShopRequestItemName OUTPUT INSERTED.WorkShopRequestItemId WHERE WorkShopRequestItemId=@WorkShopRequestItemId";


            var id = connection.Execute(sql, objWorkShopRequestItem);
            return id;
        }

        public int DeleteWorkShopRequestItem(Unit objWorkShopRequestItem)
        {
            string sql = @"Delete WorkShopRequestItem  OUTPUT DELETED.WorkShopRequestItemId WHERE WorkShopRequestItemId=@WorkShopRequestItemId";


            var id = connection.Execute(sql, objWorkShopRequestItem);
            return id;
        }


    }
}