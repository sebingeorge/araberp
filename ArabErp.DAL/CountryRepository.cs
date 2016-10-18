using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
   public class CountryRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public IEnumerable<Country> FillCountryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Country>("SELECT CountryId,CountryRefNo,CountryName,CountryRemarks FROM Country WHERE isActive=1").ToList();
            }
        }
        public string GetRefNo(Country objCountry)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Country();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Country).Name, "0", 0);
                    RefNo = "C/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }
        public Country InsertCountry(Country objCountry)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Country();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Country(CountryRefNo,CountryName,CountryRemarks,CreatedBy,CreatedDate,OrganizationId,isActive) 
                               VALUES(@CountryRefNo,@CountryName,@CountryRemarks,@CreatedBy,@CreatedDate,@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Country).Name, "0", 1);
                    objCountry.CountryRefNo = "C/" + internalid;

                    int id = connection.Query<int>(sql, objCountry, trn).Single();
                    objCountry.CountryId = id;
                    //connection.Dispose();
                    InsertLoginHistory(dataConnection, objCountry.CreatedBy, "Create", "Country", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objCountry.CountryId = 0;
                    objCountry.CountryRefNo = null;

                }
                return objCountry;
            }
        }

        public Country GetCountry(int CountryId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Country
                        where CountryId=@CountryId";

                var objCountry = connection.Query<Country>(sql, new
                {
                    CountryId = CountryId
                }).First<Country>();

                return objCountry;
            }
        }
        public Country UpdateEmployeeLocation(Country objCountry)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Country Set CountryRefNo=@CountryRefNo,CountryName=@CountryName,CountryRemarks=@CountryRemarks OUTPUT INSERTED.CountryId WHERE CountryId=@CountryId";

                var id = connection.Execute(sql, objCountry);
                InsertLoginHistory(dataConnection, objCountry.CreatedBy, "Update", "Country", id.ToString(), "0");
                return objCountry;
            }
        }

        public Country GetEmployeeLocation(int CountryId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Country
                        where CountryId=@CountryId";

                var objCountry = connection.Query<Country>(sql, new
                {
                    CountryId = CountryId
                }).First<Country>();

                return objCountry;
            }
        }

        public int DeleteCountry(Country objCountry)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Country Set isActive=0 WHERE CountryId=@CountryId";
                try
                {

                    var id = connection.Execute(sql, objCountry);
                    objCountry.CountryId = id;
                    InsertLoginHistory(dataConnection, objCountry.CreatedBy, "Delete", "Country", id.ToString(), "0");
                    result = 0;

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

       
    }
}
