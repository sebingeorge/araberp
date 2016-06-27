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
                return connection.Query<Dropdown>("SELECT SupCategoryId Id ,SupCategoryName Name FROM SupplierCategory").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCountryDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CountryId Id,CountryName Name FROM Country").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCurrencyDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency").ToList();
            }
        }

        public List<Dropdown> FillSupplier()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SupplierId Id,SupplierName Name from Supplier").ToList();
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

       
        public int InsertSupplier(Supplier objSupplier)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into Supplier(SupplierRefNo,SupplierName,PurchaseTypeId,SupplierPrintName,
                                                    SupCategoryId,ContractDate,ContactPerson,Active,
                                                    DoorNo,City,State,CountryId,
                                                    PostBoxNo,Phone,Fax,Email,
                                                    Bank,Branch,AccountDetails,SwiftCode,
                                                    RtgsNo,AccountNo,DiscountTermsId,DiscountRate,
                                                    CurrencyId,SupRefAccNo,PanNo,TinNo,
                                                    CreatedBy,CreatedDate,OrganizationId) 
                                             Values (@SupplierRefNo,@SupplierName,@PurchaseTypeId,@SupplierPrName,
                                                    @CategoryId,@ContractDate,@ContactPerson,@Active,
                                                    @DoorNo,@City,@State,@Country,
                                                    @PostBoxNo,@Phone,@Fax,@Email,
                                                    @Bank,@Branch,@AccountDetails,@SwiftCode,
                                                    @RtgsNo,@AccountNo,@DiscountTermsId,@DiscountRate,
                                                    @CurrencyId,@SupRefAccNo,@PanNo,@TinNo,
                                                    @CreatedBy,@CreatedDate,0);
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
             
                string sql = @" SELECT SupplierId,SupplierRefNo,SupplierName,SupCategoryName,PurchaseTypeName FROM Supplier S
                                INNER JOIN SupplierCategory SC ON SC.SupCategoryId=S.SupCategoryId
                                INNER JOIN PurchaseType P ON P.PurchaseTypeId=S.PurchaseTypeId
                                WHERE S.isActive=1";

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
        public IEnumerable<Dropdown> FillCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //var sym = new Currency();
                //sym = connection.Query<Currency>("select SymbolId,SymbolName from Symbol").ToList();

                return connection.Query<Dropdown>("select SupCategoryId Id,SupCategoryName Name from SupplierCategory").ToList();
            }
        }
        public IEnumerable<Dropdown> FillCdategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //var sym = new Currency();
                //sym = connection.Query<Currency>("select SymbolId,SymbolName from Symbol").ToList();

                return connection.Query<Dropdown>("select SupCategoryId Id,SupCategoryName Name from SupplierCategory").ToList();
            }
        }



    }
}