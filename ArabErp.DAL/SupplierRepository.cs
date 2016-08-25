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
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public SupplierRepository()
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
                return connection.Query<Dropdown>("SELECT SupCategoryId Id ,SupCategoryName Name FROM SupplierCategory WHERE isActive=1").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCountryDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CountryId Id,CountryName Name FROM Country WHERE isActive=1").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCurrencyDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency WHERE isActive=1").ToList();
            }
        }

        public List<Dropdown> FillSupplier()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SupplierId Id,SupplierName Name from Supplier WHERE isActive=1").ToList();
            }
        }

        public List<Dropdown> FillPurchaseType()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select PurchaseTypeId Id,PurchaseTypeName Name from PurchaseType").ToList();
            }
        }


        public Supplier InsertSupplier(Supplier objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Supplier();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"insert  into Supplier(SupplierRefNo,SupplierName,PurchaseTypeId,SupplierPrintName,
                                                    SupCategoryId,ContractDate,ContactPerson,Active,
                                                    DoorNo,City,State,CountryId,
                                                    PostBoxNo,Phone,Fax,Email,
                                                    Bank,Branch,AccountDetails,SwiftCode,
                                                    RtgsNo,AccountNo,DiscountTermsId,DiscountRate,
                                                    CurrencyId,SupRefAccNo,PanNo,TinNo,
                                                    CreditPeriod,CreditLimit,PaymentTerms,
                                                    CreatedBy,CreatedDate,OrganizationId) 
                                             Values (@SupplierRefNo,@SupplierName,@PurchaseTypeId,@SupplierPrintName,
                                                    @SupCategoryId,@ContractDate,@ContactPerson,@Active,
                                                    @DoorNo,@City,@State,@CountryId,
                                                    @PostBoxNo,@Phone,@Fax,@Email,
                                                    @Bank,@Branch,@AccountDetails,@SwiftCode,
                                                    @RtgsNo,@AccountNo,@DiscountTermsId,@DiscountRate,
                                                    @CurrencyId,@SupRefAccNo,@PanNo,@TinNo,
                                                    @CreditPeriod,@CreditLimit,@PaymentTerms,
                                                    @CreatedBy,@CreatedDate,@OrganizationId);
                                                    SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Supplier).Name, "0", 1);
                    objSupplier.SupplierRefNo = "SUP/" + internalid;

                    int id = connection.Query<int>(sql, objSupplier, trn).Single();
                    objSupplier.SupplierId = id;
                    InsertLoginHistory(dataConnection, objSupplier.CreatedBy, "Create", "Supplier", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objSupplier.SupplierId = 0;
                    objSupplier.SupplierRefNo = null;

                }
                return objSupplier;
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

                string sql = @" SELECT SupplierId,SupplierRefNo,SupplierName,SupCategoryName,PurchaseTypeName FROM Supplier S
                                INNER JOIN SupplierCategory SC ON SC.SupCategoryId=S.SupCategoryId
                                INNER JOIN PurchaseType P ON P.PurchaseTypeId=S.PurchaseTypeId
                                WHERE S.isActive=1";

                var objSuppliers = connection.Query<Supplier>(sql).ToList<Supplier>();

                return objSuppliers;
            }
        }

        public Supplier UpdateSupplier(Supplier objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE Supplier SET SupplierRefNo = @SupplierRefNo ,SupplierName=@SupplierName,PurchaseTypeId=@PurchaseTypeId,
                                SupplierPrintName=@SupplierPrintName,SupCategoryId=@SupCategoryId,ContractDate=@ContractDate,ContactPerson=@ContactPerson,
                                Active=@Active,DoorNo=@DoorNo,City=@City,State=@State,CountryId=@CountryId,PostBoxNo=@PostBoxNo,Phone=@Phone,Fax=@Fax,
                                Email=@Email,Bank=@Bank,Branch=@Branch,AccountDetails=@AccountDetails,SwiftCode=@SwiftCode,RtgsNo=@RtgsNo,AccountNo=@AccountNo,
                                DiscountTermsId=@DiscountTermsId,DiscountRate=@DiscountRate,CurrencyId=@CurrencyId,SupRefAccNo=@SupRefAccNo,PanNo=@PanNo,TinNo=@TinNo,
                                CreditPeriod=@CreditPeriod,CreditLimit=@CreditLimit,PaymentTerms=@PaymentTerms,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,
                                OrganizationId = @OrganizationId
                                WHERE SupplierId = @SupplierId";


                var id = connection.Execute(sql, objSupplier);
                InsertLoginHistory(dataConnection, objSupplier.CreatedBy, "Update", "Supplier", id.ToString(), "0");
                return objSupplier;
            }
        }

        public int DeleteSupplier(Supplier objSupplier)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE Supplier SET isActive=0 WHERE SupplierId=@SupplierId";
                try
                {

                    var id = connection.Execute(sql, objSupplier);
                    objSupplier.SupplierId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objSupplier.CreatedBy, "Delete", "Supplier", id.ToString(), "0");
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


        public IEnumerable<Dropdown> FillCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select SupCategoryId Id,SupCategoryName Name from SupplierCategory ").ToList();
            }
        }
        public IEnumerable<Dropdown> FillCdategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select SupCategoryId Id,SupCategoryName Name from SupplierCategory").ToList();
            }
        }


        public string GetRefNo(Supplier objSupplier)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Supplier();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Supplier).Name, "0", 0);
                    RefNo = "SUP/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

        /// <summary>
        /// Get Supplier Currency by SupplierId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Supplier GetSupplierCurrency(int id)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT 
	                                S.CurrencyId,
	                                C.CurrencyName
                                FROM Supplier S
	                                INNER JOIN Currency C ON S.CurrencyId = C.CurrencyId
                                WHERE S.SupplierId = @id";
                    return connection.Query<Supplier>(query, new { id = id }).First();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}