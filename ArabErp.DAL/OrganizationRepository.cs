using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class OrganizationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public IEnumerable<Organization> GetOrganizations()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select OrganizationId, OrganizationName from Organization order by OrganizationName;";
                return connection.Query<Organization>(sql);
            }
        }
        public IEnumerable<Organization> GetOrganizationWithFY()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select OrganizationId, OrganizationName = OrganizationName + ' - ' + ISNULL(FyName,'') from Organization 
                left join FinancialYear on Organization.FyId = FinancialYear.FyId where isActive = 1 order by OrganizationName;";
                return connection.Query<Organization>(sql);
            }
        }
        public Organization InsertOrganization(Organization objOrganization)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Organization();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Organization (OrganizationRefNo,OrganizationName,DoorNo,Street,State,Country,CurrencyId,Zip,Phone,Fax,Email,ContactPerson,Image1,isActive,cmpCode, FyId) 
                               VALUES(@OrganizationRefNo,@OrganizationName,@DoorNo,@Street,@State,@Country,@CurrencyId,@Zip,@Phone,@Fax,@Email,@ContactPerson,@Image1,1,@cmpCode,@FyId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                try

                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Organization).Name, "0", 1);
                    objOrganization.OrganizationRefNo = "ORG/" + internalid;

                    int id = connection.Query<int>(sql, objOrganization, trn).Single();
                    objOrganization.OrganizationId = id;
                    //connection.Dispose();
                    InsertLoginHistory(dataConnection, objOrganization.CreatedBy, "Create", "Organization", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objOrganization.OrganizationId = 0;
                    objOrganization.OrganizationRefNo = null;

                }
                return objOrganization;
            }
        }

        public IEnumerable<Dropdown> FillCurrency()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency WHERE isActive=1").ToList();
            }
        }
        public IEnumerable<Dropdown> FillCompanyDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select cmpCode Id,cmpName Name from mstAccCompany").ToList();
            }
        }
        public IEnumerable<Dropdown> FillCountryDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT CountryId Id,CountryName Name FROM Country").ToList();
            }
        }
        public IEnumerable<Organization> FillOrganizationList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Organization>(" SELECT O.OrganizationId,OrganizationRefNo,OrganizationName,CurrencyName,CountryName,COM.cmpName CompanyName From Organization O INNER JOIN Currency C ON C.CurrencyId=O.CurrencyId " +
                                                      " left join Country  CO on CO.CountryId=O.Country "+
                                                      " inner join mstAccCompany COM on COM.cmpCode=O.cmpCode "+
                                                      " WHERE O.isActive=1 ").ToList();
            }
        }

        public Organization GetOrganization(int OrganizationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT O.*,c.CountryName FROM Organization O
                               inner join Country C ON C.CountryId=O.Country
                               WHERE O.OrganizationId=@OrganizationId";

                var objOrganization = connection.Query<Organization>(sql, new
                {
                    OrganizationId = OrganizationId
                }).First<Organization>();

                return objOrganization;
            }
        }

        public Organization UpdateOrganization(Organization objOrganization)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Organization Set OrganizationRefNo=@OrganizationRefNo,OrganizationName=@OrganizationName,DoorNo=@DoorNo,Street=@Street,State=@State,CurrencyId=@CurrencyId,Zip=@Zip,Phone=@Phone,Fax=@Fax,Email=@Email,ContactPerson=@ContactPerson,Image1=@Image1,FyId=@FyId,Country=@Country,cmpCode=@cmpCode OUTPUT INSERTED.OrganizationId WHERE OrganizationId=@OrganizationId";

                var id = connection.Execute(sql, objOrganization);
                InsertLoginHistory(dataConnection, objOrganization.CreatedBy, "Update", "Organization", id.ToString(), "0");
                return objOrganization;
            }
        }

        public int DeleteOrganization(Organization objOrganization)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Organization Set isActive=0 WHERE OrganizationId=@OrganizationId";
                try
                {

                    var id = connection.Execute(sql, objOrganization);
                    objOrganization.OrganizationId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objOrganization.CreatedBy, "Delete", "Organization", id.ToString(), "0");
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

        public void UpdateImageName(string fileName, int? id, int? index)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"UPDATE Organization SET Image" + index + " = @fileName WHERE OrganizationId = @id";
                connection.Execute(query, new { fileName = fileName, id = id });
            }
        }

       
    }
}
