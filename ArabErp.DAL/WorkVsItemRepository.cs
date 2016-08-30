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
        public int InsertWorkVsItem(WorkVsItem objWorkVsItem, IDbConnection connection, IDbTransaction trn)
        {

             try
            {
                string sql = @"insert  into WorkVsItem(ItemId,WorkDescriptionId,Quantity,CreatedBy,CreatedDate,OrganizationId) Values (@ItemId,@WorkDescriptionId,@Quantity,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkVsItem,trn).Single();
                return id;
            }
             catch (Exception)
             {
                 throw;
             }

        }




        public List<WorkVsItem> GetWorkDescriptionWorkVsItems(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT  WorkVsItemId,W.ItemId,WorkDescriptionId,Quantity,UnitName UoM,W.isActive  From WorkVsItem W
                                INNER JOIN Item I On I.ItemId=W.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                WHERE W.isActive=1 and WorkDescriptionId=@Id";

                var objWorkVsItems = connection.Query<WorkVsItem>(sql, new
                {
                    Id = Id
                }).ToList<WorkVsItem>();

                return objWorkVsItems;
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