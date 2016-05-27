using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ItemSubGroupRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertItemSubGroup(ItemSubGroup objItemSubGroup)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO ItemSubGroup (ItemSubGroupRefNo,ItemSubGroupName,ItemGroupId,
            CreatedBy,CreatedDate,OrganizationId) VALUES(@ItemSubGroupRefNo,@ItemSubGroupName,@ItemGroupId,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objItemSubGroup).Single();
                return id;
            }
        }

        public IEnumerable<ItemSubGroup> FillItemSubGroupList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemSubGroup>("SELECT ItemSubGroupRefNo,ItemSubGroupName,ItemGroupName FROM ItemSubGroup S INNER JOIN ItemGroup G ON G.ItemGroupId=S.ItemGroupId").ToList();
            }
        }

        public IEnumerable<Dropdown> FillGroup()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("Select ItemGroupId Id,ItemGroupName Name From ItemGroup").ToList();
            }
        }
        public ItemSubGroup GetItemSubGroup(int ItemSubGroupId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from ItemSubGroup
                        where ItemSubGroupId=@ItemSubGroupId";

                var objItemSubGroup = connection.Query<ItemSubGroup>(sql, new
                {
                    ItemSubGroupId = ItemSubGroupId
                }).First<ItemSubGroup>();

                return objItemSubGroup;
            }
        }

        public List<ItemSubGroup> GetItemSubGroups()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemSubGroup
                        where isActive=1";

                var objItemSubGroups = connection.Query<ItemSubGroup>(sql).ToList<ItemSubGroup>();

                return objItemSubGroups;
            }
        }

        public int UpdateItemSubGroup(ItemSubGroup objItemSubGroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update ItemSubGroup Set ItemSubGroupRefNo=@ItemSubGroupRefNo,ItemSubGroupName=@ItemSubGroupName OUTPUT INSERTED.ItemSubGroupId WHERE ItemSubGroupId=@ItemSubGroupId";


                var id = connection.Execute(sql, objItemSubGroup);
                return id;
            }
        }

        public int DeleteItemSubGroup(Unit objItemSubGroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete ItemSubGroup  OUTPUT DELETED.ItemSubGroupId WHERE ItemSubGroupId=@ItemSubGroupId";


                var id = connection.Execute(sql, objItemSubGroup);
                return id;
            }
        }


    }
}