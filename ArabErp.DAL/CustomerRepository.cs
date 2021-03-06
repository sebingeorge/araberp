﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class CustomerRepository : BaseRepository
    {
       
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public CustomerRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public IEnumerable<Dropdown> FillCategoryDropdown()
        {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 return connection.Query<Dropdown>("SELECT CusCategoryId Id ,CusCategoryName Name FROM CustomerCategory WHERE isActive=1").ToList();
             }
        }

        public IEnumerable<Dropdown>FillCountryDropdown()
        {
            using (IDbConnection connection=OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CountryId Id,CountryName Name FROM Country WHERE isActive=1").ToList();
            }
        }

        public IEnumerable<Dropdown>FillCurrencyDropdown()
        {
            using (IDbConnection connection=OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency WHERE isActive=1").ToList();
            }
        }

        public Customer InsertCustomer(Customer objCustomer)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Customer();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"insert  into Customer(CustomerRefNo,CustomerName,CustomerPrintName,CategoryId,LeadSourceId,CurrencyId,
                                                     DoorNo,Street,State,Country,Zip,Phone,Fax,Email,ContactPerson,CreditPeriod,
                                                     CreditLimit,CreatedBy,CreatedDate,OrganizationId,approve) 
                                                     Values (@CustomerRefNo,@CustomerName,@CustomerPrintName,@CategoryId,@LeadSourceId,
                                                     @CurrencyId,@DoorNo,@Street,@State,@Country,@Zip,@Phone,@Fax,@Email,@ContactPerson,
                                                     @CreditPeriod,@CreditLimit,@CreatedBy,@CreatedDate,@OrganizationId,0);
                                                     SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Customer).Name, "0", 1);
                    objCustomer.CustomerRefNo = "CUS/" + internalid;

                    int id = connection.Query<int>(sql, objCustomer, trn).Single();
                    objCustomer.CustomerId = id;
                    InsertLoginHistory(dataConnection, objCustomer.CreatedBy, "Create", "Customer", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objCustomer.CustomerId = 0;
                    objCustomer.CustomerRefNo = null;

                }
                return objCustomer;
            }
        }



        public Customer GetCustomer(int CustomerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Customer
                        where CustomerId=@CustomerId";

                var objCustomer = connection.Query<Customer>(sql, new
                {
                    CustomerId = CustomerId
                }).First<Customer>();

                return objCustomer;
            }
        }

        public List<Customer> GetCustomers(string customer)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT CustomerId,CustomerRefNo,CustomerName,CusCategoryName,CountryName,isnull(approve,0)approve FROM Customer C
                                INNER JOIN CustomerCategory ON CusCategoryId=C.CategoryId
                                INNER JOIN Country ON CountryId=C.Country
                                WHERE C.isActive=1 and CustomerName LIKE @customer+'%'
                                order by CustomerId desc ";

                var objCustomers = connection.Query<Customer>(sql, new
                {
                    customer = customer
                    }).ToList<Customer>();

                return objCustomers;
            }
        }

        public Customer UpdateCustomer(Customer objCustomer)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE Customer SET CustomerRefNo = @CustomerRefNo ,CustomerName=@CustomerName,CustomerPrintName=@CustomerPrintName,
                                CategoryId=@CategoryId,LeadSourceId=@LeadSourceId,CurrencyId=@CurrencyId,DoorNo=@DoorNo,Street=@Street,State=@State,
                                Country=@Country,Zip=@Zip,Phone=@Phone,Fax=@Fax,Email=@Email,ContactPerson=@ContactPerson,CreditPeriod=@CreditPeriod,
                                CreditLimit=@CreditLimit,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId=@OrganizationId
                                WHERE CustomerId = @CustomerId";

                var id = connection.Execute(sql, objCustomer);
                InsertLoginHistory(dataConnection, objCustomer.CreatedBy, "Update", "Customer", id.ToString(), "0");
                return objCustomer;
            }
        }

        public int DeleteCustomer(Customer objCustomer)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE Customer SET isActive=0  WHERE CustomerId=@CustomerId";
                try
                {

                    var id = connection.Execute(sql, objCustomer);
                    objCustomer.CustomerId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objCustomer.CreatedBy, "Delete", "Customer", id.ToString(), "0");
                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                    {
                        switch (ex.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                result = 1;
                                break;

                            default:
                                result = 2;
                                break;
                        }
                    }

                }

                return result;
            }
        }


        /// <summary>
        /// Get the door, street, state, country address of a given customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public string GetCustomerAddress(int customerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    return connection.Query<string>(@"SELECT ISNULL(DoorNo,'')+', '+ISNULL(Street,'')+', '+ISNULL([State],'')+', '+ISNULL(CountryName, '') FROM Customer LEFT JOIN Country ON Country = CountryId WHERE CustomerId = @customerId",
                                new { customerId = customerId }).First();
                }
                catch (InvalidOperationException)
                {
                    return "";
                }
            }
        }

        public string GetRefNo(Customer objCustomer)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Customer();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Customer).Name, "0", 0);
                    RefNo = "CUS/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

        public int GetCustomerFromWarranty(int id, bool isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    return connection.Query<int>(@"SELECT CustomerId FROM SalesQuotation WHERE " + (isProjectBased ? "ProjectCompletionId" : "DeliveryChallanId") + " = @id",
                                new { id = id }).FirstOrDefault();
                }
                catch (InvalidOperationException)
                {
                    return 0;
                }
            }
        }

        public void ApproveCustomer(Customer objCustomer)
             {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Customer  SET approve=1  OUTPUT INSERTED.CustomerId WHERE CustomerId=@CustomerId";


                var id = connection.Query(sql, new { CustomerId = objCustomer.CustomerId });

            }
        }

    }
}
