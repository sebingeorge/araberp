using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class BayRepository : BaseRepository
    {

        public int InsertBay(Bay objBay)
        {
            string sql = @"INSERT INTO Bay (BayRefNo,BayName,CreatedBy,CreatedDate,OrganizationId) VALUES(@BayRefNo,@BayName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objBay).Single();
            return id;
        }


        public Bay GetBay(int BayId)
        {

            string sql = @"select * from Bay
                        where BayId=@BayId";

            var objBay = connection.Query<Bay>(sql, new
            {
                BayId = BayId
            }).First<Bay>();

            return objBay;
        }

        public List<Bay> GetBays()
        {
            string sql = @"select * from Bay
                        where OrganizationId>0";

            var objBays = connection.Query<Bay>(sql).ToList<Bay>();

            return objBays;
        }


    }
}
