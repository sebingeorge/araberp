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

        public IEnumerable<Dropdown> FillSupplier()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT SupplierId Id,SupplierName Name FROM Supplier").ToList();
            }
        }

        public IEnumerable<Dropdown> FillItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT ItemId Id,ItemName Name FROM Item").ToList();
            }
        }



        public int InsertSupplierItemRate(SupplierItemRate objSupplierItemRate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                int id = 0;
                foreach (var item in objSupplierItemRate.SupplierItemRateItem)
                {
                    string sql = @"insert  into SupplierItemRate(SupplierId,ItemId,FixedRate,EffectiveDate,CreatedBy,CreatedDate,OrganizationId,isActive) 
                           Values (@SupplierId,@ItemId,@FixedRate,@CreatedDate,@CreatedBy,@CreatedDate,@OrganizationId,1);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                    //id = connection.Query<int>(sql, objOpeningStock).Single();

                    id = connection.Query<int>(sql, new
                    {
                        SupplierId = objSupplierItemRate.SupplierId,
                        ItemId = item.ItemId,
                        FixedRate = item.FixedRate,
                        CreatedBy = objSupplierItemRate.CreatedBy,
                        EffectiveDate = objSupplierItemRate.CreatedDate,
                        CreatedDate = objSupplierItemRate.CreatedDate,
                        OrganizationId = objSupplierItemRate.OrganizationId
                    }).Single();

                }


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


        public int DeleteSupplierItemRate(SupplierItemRate objSupplierItemRate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete From  SupplierItemRate  WHERE SupplierId=@SupplierId";
                var id = connection.Execute(sql, objSupplierItemRate);
                return id;
            }
        }


        public IEnumerable<SupplierItemRateItem> GetItem(int? SupplierId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT ItemId,FixedRate  FROM SupplierItemRate WHERE SupplierId=@SupplierId";

                return connection.Query<SupplierItemRateItem>(sql, new
                {
                    SupplierId = SupplierId
                }).ToList();
            }
        }


    }
}