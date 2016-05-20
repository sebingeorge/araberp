using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class CustomerRepository : BaseRepository
    {

        public int InsertCustomer(Customer objCustomer)
        {
            string sql = @"insert  into Customer(CustomerRefNo,CustomerName,CustomerPrintName,CategoryId,LeadSourceId,CurrencyId,DoorNo,Street,State,Country,Zip,Phone,Fax,Email,ContactPerson,CreatedBy,CreatedDate,OrganizationId) Values (@CustomerRefNo,@CustomerName,@CustomerPrintName,@CategoryId,@LeadSourceId,@CurrencyId,@DoorNo,@Street,@State,@Country,@Zip,@Phone,@Fax,@Email,@ContactPerson,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objCustomer).Single();
            return id;
        }


        public Customer GetCustomer(int CustomerId)
        {

            string sql = @"select * from Customer
                        where CustomerId=@CustomerId";

            var objCustomer = connection.Query<Customer>(sql, new
            {
                CustomerId = CustomerId
            }).First<Customer>();

            return objCustomer;
        }

        public List<Customer> GetCustomers()
        {
            string sql = @"select * from Customer
                        where isActive=1";

            var objCustomers = connection.Query<Customer>(sql).ToList<Customer>();

            return objCustomers;
        }

        public int UpdateCustomer(Customer objCustomer)
        {
            string sql = @"UPDATE Customer SET CustomerRefNo = @CustomerRefNo ,CustomerName = @CustomerName ,CustomerPrintName = @CustomerPrintName ,CategoryId = @CategoryId,LeadSourceId = @LeadSourceId,CurrencyId = @CurrencyId,DoorNo = @DoorNo,Street = @Street,State = @State,Country = @Country,Zip = @Zip,Phone = @Phone,Fax = @Fax,Email = @Email,ContactPerson = @ContactPerson,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.CustomerId  WHERE CustomerId = @CustomerId";


            var id = connection.Execute(sql, objCustomer);
            return id;
        }

        public int DeleteCustomer(Unit objCustomer)
        {
            string sql = @"Delete Customer  OUTPUT DELETED.CustomerId WHERE CustomerId=@CustomerId";


            var id = connection.Execute(sql, objCustomer);
            return id;
        }


    }
}