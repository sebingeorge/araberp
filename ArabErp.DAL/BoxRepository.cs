using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class BoxRepository : BaseRepository
    {

        public int InsertBox(Box objBox)
        {
            string sql = @"INSERT INTO Box (BoxRefNo,BoxName,CreatedBy,CreatedDate,OrganizationId) VALUES(@BoxRefNo,@BoxName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objBox).Single();
            return id;
        }


        public Box GetBox(int BoxId)
        {

            string sql = @"select * from Box
                        where BoxId=@BoxId";

            var objBox = connection.Query<Box>(sql, new
            {
                BoxId = BoxId
            }).First<Box>();

            return objBox;
        }

        public List<Box> GetBoxs()
        {
            string sql = @"select * from Box
                        where isActive=1";

            var objBoxs = connection.Query<Box>(sql).ToList<Box>();

            return objBoxs;
        }


    }
}
