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
                                                     cmpVATNo,cmpCSTNo,cmpIECode,cmpWorkSheetNo,cmpFormPrefix,CreatedBy,CreatedDate) 
                                                     Values (@cmpUsercode,@cmpName,@cmpShrtName,@cmpRemarks,@cmpDoorNo,
                                                     @cmpStreet,@cmpArea,@cmpState,@cmpPhNo,@cmpFax,@cmpEmail,@cmpWeb,@cmpPAN,
                                                     @cmpSSN,@cmpVATNo,@cmpCSTNo,@cmpIECode,@cmpWorkSheetNo,@cmpFormPrefix,@CreatedBy,@CreatedDate);
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



    }
}
