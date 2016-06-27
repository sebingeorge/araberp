using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplierItemRateRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSupplierItemRate(SupplierItemRate objSupplierItemRate)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into SupplierItemRate(ItemId,SupplierId,EffectiveDate,FixedRate,CreatedBy,CreatedDate,OrganizationId) Values (@ItemId,@SupplierId,@EffectiveDate,@FixedRate,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSupplierItemRate).Single();
                return id;
            }
        }


        public SupplierItemRate GetSupplierItemRate(int SupplierItemRateId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplierItemRate
                        where SupplierItemRateId=@SupplierItemRateId";

                var objSupplierItemRate = connection.Query<SupplierItemRate>(sql, new
                {
                    SupplierItemRateId = SupplierItemRateId
                }).First<SupplierItemRate>();

                return objSupplierItemRate;
            }
        }

        public List<SupplierItemRate> GetSupplierItemRates()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplierItemRate
                        where isActive=1";

                var objSupplierItemRates = connection.Query<SupplierItemRate>(sql).ToList<SupplierItemRate>();

                return objSupplierItemRates;
            }
        }



        public int DeleteSupplierItemRate(Unit objSupplierItemRate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplierItemRate  OUTPUT DELETED.SupplierItemRateId WHERE SupplierItemRateId=@SupplierItemRateId";


                var id = connection.Execute(sql, objSupplierItemRate);
                return id;
            }
        }


    }
}