using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
   public  class CompanyRepository:BaseRepository
    {

        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public CompanyRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public Company InsertCompany(Company objCompany)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Company();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"insert  into mstAccCompany(cmpUsercode,cmpName,cmpShrtName,cmpRemarks,cmpDoorNo,cmpStreet,
                                                     cmpArea,cmpState,cmpPhNo,cmpFax,cmpEmail,cmpWeb,cmpPAN,cmpSSN,
                                                     cmpVATNo,cmpCSTNo,cmpIECode,cmpWorkSheetNo,cmpFormPrefix,CreatedBy,CreatedDate,IsActive) 
                                                     Values (@cmpUsercode,@cmpName,@cmpShrtName,@cmpRemarks,@cmpDoorNo,
                                                     @cmpStreet,@cmpArea,@cmpState,@cmpPhNo,@cmpFax,@cmpEmail,@cmpWeb,@cmpPAN,
                                                     @cmpSSN,@cmpVATNo,@cmpCSTNo,@cmpIECode,@cmpWorkSheetNo,@cmpFormPrefix,@CreatedBy,@CreatedDate,1);
                                                     SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Company).Name, "0", 1);
                  //  objCompany.CustomerRefNo = "CUS/" + internalid;

                    int id = connection.Query<int>(sql, objCompany, trn).Single();
                    objCompany.cmpCode = id;
                    InsertLoginHistory(dataConnection, objCompany.CreatedBy, "Create", "mstAccCompany", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objCompany.cmpCode = 0;
                    //objCustomer.CustomerRefNo = null;

                }
                return objCompany;
            }
        }
        public IEnumerable<Company> CompanyList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Company>(" select cmpCode,cmpUsercode,cmpName,cmpShrtName,cmpDoorNo,cmpStreet,cmpArea from mstAccCompany where IsActive=1 ").ToList();
            }
        }

        public Company UpdateCompany(Company objCompany)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE mstAccCompany SET cmpUsercode=@cmpUsercode ,cmpName=@cmpName,cmpShrtName=@cmpShrtName,cmpRemarks=@cmpRemarks,cmpDoorNo=@cmpDoorNo,cmpStreet=@cmpStreet,cmpArea=@cmpArea,
                                cmpState=@cmpState,cmpPhNo=@cmpPhNo,cmpFax=@cmpFax,cmpEmail=@cmpEmail,cmpWeb=@cmpWeb,cmpPAN=@cmpPAN,cmpSSN=@cmpSSN,
                                cmpVATNo=@cmpVATNo,cmpCSTNo=@cmpCSTNo,cmpIECode=@cmpIECode,cmpWorkSheetNo=@cmpWorkSheetNo,cmpFormPrefix=@cmpFormPrefix,CreatedBy=@CreatedBy,CreatedDate =@CreatedDate
                                WHERE cmpcode = @cmpcode";

                var id = connection.Execute(sql, objCompany);
                InsertLoginHistory(dataConnection, objCompany.CreatedBy, "Update", "mstAccCompany", id.ToString(), "0");
                return objCompany;
            }
        }
        public Company GetCompany(int cmpcode)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from mstAccCompany
                        where cmpcode=@cmpcode ";

                var objCompany = connection.Query<Company>(sql, new
                {
                    cmpcode = cmpcode
                }).First<Company>();

                return objCompany;
            }
        }

        public int DeleteCustomer(Company objCompany)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE mstAccCompany SET isActive=0  WHERE cmpcode=@cmpcode";
                try
                {

                    var id = connection.Execute(sql, objCompany);
                    objCompany.cmpCode = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objCompany.CreatedBy, "Delete", "mstAccCompany", id.ToString(), "0");
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
