using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WareHouseRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertWareHouse(WareHouse objWareHouse)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into WareHouse(WareHouseNo,WareHouseDescription,CreatedBy,CreatedDate,OrganizationId) Values (@WareHouseNo,@WareHouseDescription,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWareHouse).Single();
                return id;
            }
        }


        public WareHouse GetWareHouse(int WareHouseId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WareHouse
                        where WareHouseId=@WareHouseId";

                var objWareHouse = connection.Query<WareHouse>(sql, new
                {
                    WareHouseId = WareHouseId
                }).First<WareHouse>();

                return objWareHouse;
            }
        }

        public List<WareHouse> GetWareHouses()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WareHouse
                        where isActive=1";

                var objWareHouses = connection.Query<WareHouse>(sql).ToList<WareHouse>();

                return objWareHouses;
            }
        }

        public int UpdateWareHouse(WareHouse objWareHouse)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE WareHouse SET WareHouseNo = @WareHouseNo ,WareHouseDescription = @WareHouseDescription ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.WareHouseId  WHERE WareHouseId = @WareHouseId";


                var id = connection.Execute(sql, objWareHouse);
                return id;
            }
        }

        public int DeleteWareHouse(Unit objWareHouse)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WareHouse  OUTPUT DELETED.WareHouseId WHERE WareHouseId=@WareHouseId";


                var id = connection.Execute(sql, objWareHouse);
                return id;
            }
        }


    }
}