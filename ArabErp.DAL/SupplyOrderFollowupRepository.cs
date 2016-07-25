using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderFollowupRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSupplyOrderFollowup(SupplyOrderFollowup objSupplyOrderFollowup)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into SupplyOrderFollowup(SupplyOrderItemId,SupplyOrderFollowupDate,ExpectedDate,Remarks,CreatedBy,CreatedDate,OrganizationId) Values (@SupplyOrderItemId,@SupplyOrderFollowupDate,@ExpectedDate,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSupplyOrderFollowup).Single();
                return id;
            }
        }


        public SupplyOrderFollowup GetSupplyOrderFollowup(int SupplyOrderFollowupId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrderFollowup
                        where SupplyOrderFollowupId=@SupplyOrderFollowupId";

                var objSupplyOrderFollowup = connection.Query<SupplyOrderFollowup>(sql, new
                {
                    SupplyOrderFollowupId = SupplyOrderFollowupId
                }).First<SupplyOrderFollowup>();

                return objSupplyOrderFollowup;
            }
        }

        public List<SupplyOrderFollowup> GetSupplyOrderFollowups()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrderFollowup
                        where isActive=1";

                var objSupplyOrderFollowups = connection.Query<SupplyOrderFollowup>(sql).ToList<SupplyOrderFollowup>();

                return objSupplyOrderFollowups;
            }
        }



        public int DeleteSupplyOrderFollowup(Unit objSupplyOrderFollowup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrderFollowup  OUTPUT DELETED.SupplyOrderFollowupId WHERE SupplyOrderFollowupId=@SupplyOrderFollowupId";


                var id = connection.Execute(sql, objSupplyOrderFollowup);
                return id;
            }
        }


    }
}
