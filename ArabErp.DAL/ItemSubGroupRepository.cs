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

        public ItemSubGroup InsertItemSubGroup(ItemSubGroup model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"INSERT INTO ItemSubGroup (ItemSubGroupRefNo,ItemSubGroupName,ItemGroupId,
            CreatedBy,CreatedDate,OrganizationId) VALUES(@ItemSubGroupRefNo,@ItemSubGroupName,@ItemGroupId,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                int id = 0;

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ItemSubGroup).Name, "0", 1);
                    model.ItemSubGroupRefNo = "ISG/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.ItemSubGroupId = id;

                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.ItemSubGroupId = 0;
                    model.ItemSubGroupRefNo = null;
                }
                return model;
            }
        }

        public IEnumerable<ItemSubGroup> FillItemSubGroupList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemSubGroup>("SELECT ItemSubGroupId,ItemSubGroupRefNo,ItemSubGroupName,ItemGroupName FROM ItemSubGroup S INNER JOIN ItemGroup G ON G.ItemGroupId=S.ItemGroupId where S.isActive=1").ToList();
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

        public ItemSubGroup UpdateItemSubGroup(ItemSubGroup model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE ItemSubGroup SET ItemSubGroupName = @ItemSubGroupName,ItemGroupId = @ItemGroupId, CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.ItemSubGroupId  WHERE ItemSubGroupId = @ItemSubGroupId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.ItemSubGroupId = id;
                }
                catch (Exception ex)
                {

                    model.ItemSubGroupId = 0;

                }
                return model;
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
        public ItemSubGroup DeleteItemSubGroup(ItemSubGroup model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE ItemSubGroup SET isActive = 0 OUTPUT INSERTED.ItemSubGroupId  WHERE ItemSubGroupId = @ItemSubGroupId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.ItemSubGroupId = id;
                }
                catch (Exception ex)
                {

                    model.ItemSubGroupId = 0;

                }
                return model;
            }
        }


    }
}