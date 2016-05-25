using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplierRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertSupplier(Supplier objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into Supplier(SupplierName,Address1,Address2,Address3,Phone,CreatedBy,CreatedDate,OrganizationId) Values (@SupplierName,@Address1,@Address2,@Address3,@Phone,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSupplier).Single();
                return id;
            }
        }

        public Supplier GetSupplier(int SupplierId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Supplier
                        where SupplierId=@SupplierId";

                var objSupplier = connection.Query<Supplier>(sql, new
                {
                    SupplierId = SupplierId
                }).First<Supplier>();

                return objSupplier;
            }
        }

        public List<Supplier> GetSuppliers()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Supplier
                        where isActive=1";

                var objSuppliers = connection.Query<Supplier>(sql).ToList<Supplier>();

                return objSuppliers;
            }
        }

        public int UpdateSupplier(Supplier objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Supplier SET SupplierName = @SupplierName ,Address1 = @Address1 ,Address2 = @Address2 ,Address3 = @Address3,Phone = @Phone,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.SupplierId  WHERE SupplierId = @SupplierId";


                var id = connection.Execute(sql, objSupplier);
                return id;
            }
        }

        public int DeleteSupplier(Unit objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete Supplier  OUTPUT DELETED.SupplierId WHERE SupplierId=@SupplierId";


                var id = connection.Execute(sql, objSupplier);
                return id;
            }
        }


    }
}