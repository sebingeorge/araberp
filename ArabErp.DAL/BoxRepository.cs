using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class BoxRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertBox(Box objBox)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO Box (BoxRefNo,BoxName,CreatedBy,CreatedDate,OrganizationId) VALUES(@BoxRefNo,@BoxName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objBox).Single();
                return id;
            }
        }


        public Box GetBox(int BoxId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Box
                        where BoxId=@BoxId";

                var objBox = connection.Query<Box>(sql, new
                {
                    BoxId = BoxId
                }).First<Box>();

                return objBox;
            }
        }

        public List<Box> GetBoxs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Box
                        where isActive=1";

                var objBoxs = connection.Query<Box>(sql).ToList<Box>();

                return objBoxs;
            }
        }


    }
}
