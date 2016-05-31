using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkVsItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertWorkVsItem(WorkVsItem objWorkVsItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into WorkVsItem(ItemId,WorkDescriptionId,Quantity,CreatedBy,CreatedDate,OrganizationId) Values (@ItemId,@WorkDescriptionId,@Quantity,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkVsItem).Single();
                return id;
            }
        }


  

        public List<WorkVsItem> GetWorkVsItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkVsItem
                        where isActive=1";

                var objWorkVsItems = connection.Query<WorkVsItem>(sql).ToList<WorkVsItem>();

                return objWorkVsItems;
            }
        }



        public int DeleteWorkVsItem(Unit objWorkVsItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkVsItem  OUTPUT DELETED.WorkVsItemId WHERE WorkVsItemId=@WorkVsItemId";


                var id = connection.Execute(sql, objWorkVsItem);
                return id;
            }
        }


    }
}