using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class CustomerVsWorkRateRepository : BaseRepository
    {

        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<Dropdown> FillCustomer()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CustomerId Id,CustomerName Name FROM Customer").ToList();
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
        public int DeleteCustomerItemRate(CustomerVsWorkDescriptionRate objCustomerVsWorkRate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete From  CustomerVsWorkRate  WHERE CustomerId=@CustomerId";
                var id = connection.Execute(sql, objCustomerVsWorkRate);
                InsertLoginHistory(dataConnection, objCustomerVsWorkRate.CreatedBy, "Delete", "Customer Vs Work Rate", id.ToString(), "0");
                return id;
            }
        }

        public int InsertCustomerItemRate(CustomerVsWorkDescriptionRate objCustomerVsWorkRate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                int id = 0;
                foreach (var item in objCustomerVsWorkRate.CustomerVsWorkRateItem)
                {
                    string sql = @"insert  into CustomerVsWorkRate(CustomerId,WorkDescriptionId,FixedRate,CreatedBy,CreatedDate,OrganizationId,isActive) 
                           Values (@CustomerId,@WorkDescriptionId,@FixedRate,@CreatedBy,@CreatedDate,@OrganizationId,1);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                    //id = connection.Query<int>(sql, objOpeningStock).Single();

                    id = connection.Query<int>(sql, new
                    {
                        CustomerId = objCustomerVsWorkRate.CustomerId,
                        WorkDescriptionId = item.WorkDescriptionId,
                        FixedRate = item.FixedRate,
                        CreatedBy = objCustomerVsWorkRate.CreatedBy,
                        CreatedDate = objCustomerVsWorkRate.CreatedDate,
                        OrganizationId = objCustomerVsWorkRate.OrganizationId
                    }).Single();
                    InsertLoginHistory(dataConnection, objCustomerVsWorkRate.CreatedBy, "Update", "Customer Vs Work Rate", id.ToString(), "0");
                }


                return id;

            }
        }

        public IEnumerable<CustomerVsWorkRateItem> GetWorkList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT W.WorkDescriptionId,WorkDescr WorkDescription,0.00 FixedRate FROM WorkDescription W 
                               LEFT JOIN CustomerVsWorkRate C ON C.WorkDescriptionId=W.WorkDescriptionId";

                return connection.Query<CustomerVsWorkRateItem>(sql, new
                {
                    
                }).ToList();
            }
        }
        public IEnumerable<CustomerVsWorkRateItem> GetItem(int? CustomerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT 
                                    WD.WorkDescriptionId,
	                                WorkDescr WorkDescription,
	                                ISNULL(FixedRate, 0.00) FixedRate
                                FROM WorkDescription WD
	                                LEFT JOIN (SELECT WorkDescriptionId, FixedRate FROM CustomerVsWorkRate WHERE CustomerId = @CustomerId) CR ON WD.WorkDescriptionId = CR.WorkDescriptionId";

                return connection.Query<CustomerVsWorkRateItem>(sql, new
                {
                    CustomerId = CustomerId
                }).ToList();
            }
        }

    }
}
