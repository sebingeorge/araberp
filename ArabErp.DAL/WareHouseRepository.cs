using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class WareHouseRepository : BaseRepository
    {

        public int InsertWareHouse(WareHouse objWareHouse)
        {
            string sql = @"insert  into WareHouse(WareHouseNo,WareHouseDescription,CreatedBy,CreatedDate,OrganizationId) Values (@WareHouseNo,@WareHouseDescription,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objWareHouse).Single();
            return id;
        }


        public WareHouse GetWareHouse(int WareHouseId)
        {

            string sql = @"select * from WareHouse
                        where WareHouseId=@WareHouseId";

            var objWareHouse = connection.Query<WareHouse>(sql, new
            {
                WareHouseId = WareHouseId
            }).First<WareHouse>();

            return objWareHouse;
        }

        public List<WareHouse> GetWareHouses()
        {
            string sql = @"select * from WareHouse
                        where isActive=1";

            var objWareHouses = connection.Query<WareHouse>(sql).ToList<WareHouse>();

            return objWareHouses;
        }

        public int UpdateWareHouse(WareHouse objWareHouse)
        {
            string sql = @"UPDATE WareHouse SET WareHouseNo = @WareHouseNo ,WareHouseDescription = @WareHouseDescription ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.WareHouseId  WHERE WareHouseId = @WareHouseId";


            var id = connection.Execute(sql, objWareHouse);
            return id;
        }

        public int DeleteWareHouse(Unit objWareHouse)
        {
            string sql = @"Delete WareHouse  OUTPUT DELETED.WareHouseId WHERE WareHouseId=@WareHouseId";


            var id = connection.Execute(sql, objWareHouse);
            return id;
        }


    }
}