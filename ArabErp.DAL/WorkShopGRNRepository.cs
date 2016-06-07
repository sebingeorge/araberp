using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkShopGRNRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertWorkShopGRN(WorkShopGRN objWorkShopGRN)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into WorkShopGRN(GRNId,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,Rate,Discount,Amount,Remarks) Values (@GRNId,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount,@Remarks);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkShopGRN).Single();
                return id;
            }
        }


        public WorkShopGRN GetWorkShopGRN(int WorkShopGRNId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRN
                        where WorkShopGRNId=@WorkShopGRNId";

                var objWorkShopGRN = connection.Query<WorkShopGRN>(sql, new
                {
                    WorkShopGRNId = WorkShopGRNId
                }).First<WorkShopGRN>();

                return objWorkShopGRN;
            }
        }

        public List<WorkShopGRN> GetWorkShopGRNs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopGRN
                        where isActive=1";

                var objWorkShopGRNs = connection.Query<WorkShopGRN>(sql).ToList<WorkShopGRN>();

                return objWorkShopGRNs;
            }
        }



        public int DeleteWorkShopGRN(Unit objWorkShopGRN)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkShopGRN  OUTPUT DELETED.WorkShopGRNId WHERE WorkShopGRNId=@WorkShopGRNId";


                var id = connection.Execute(sql, objWorkShopGRN);
                return id;
            }
        }


    }
}