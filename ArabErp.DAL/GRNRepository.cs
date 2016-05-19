using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class GRNRepository : BaseRepository
    {

        public int InsertGRN(GRN objGRN)
        {
            string sql = @"insert  into GRN(GRNNo,GRNDate,SupplierId,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@GRNNo,@GRNDate,@SupplierId,@WareHouseId,@SupplierDCNoAndDate,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objGRN).Single();
            return id;
        }


        public GRN GetGRN(int GRNId)
        {

            string sql = @"select * from GRN
                        where GRNId=@GRNId";

            var objGRN = connection.Query<GRN>(sql, new
            {
                GRNId = GRNId
            }).First<GRN>();

            return objGRN;
        }

        public List<GRN> GetGRNs()
        {
            string sql = @"select * from GRN
                        where isActive=1";

            var objGRNs = connection.Query<GRN>(sql).ToList<GRN>();

            return objGRNs;
        }

        public int UpdateGRN(GRN objGRN)
        {
            string sql = @"UPDATE GRN SET GRNNo = @GRNNo ,GRNDate = @GRNDate ,SupplierId = @SupplierId ,WareHouseId = @WareHouseId,SupplierDCNoAndDate = @SupplierDCNoAndDate  OUTPUT INSERTED.GRNId  WHERE GRNId = @GRNId";


            var id = connection.Execute(sql, objGRN);
            return id;
        }

        public int DeleteGRN(Unit objGRN)
        {
            string sql = @"Delete GRN  OUTPUT DELETED.GRNId WHERE GRNId=@GRNId";


            var id = connection.Execute(sql, objGRN);
            return id;
        }


    }
}