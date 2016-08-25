using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class FinancialYearRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<FinancialYear> GetFinancialYear()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<FinancialYear>("select * from FinancialYear");
            }
        }

        public DateTime  GetFinStartDate(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            
              string qry = @"	
                 
                       DECLARE @FIN_ID INT;
                       SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                       select FyStartDate from FinancialYear  WHERE FyId = @FIN_ID";
              return connection.Query<DateTime>(qry, new { OrganizationId = OrganizationId }).First();
            }
        }
        public DateTime GetFinEndDate(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                 
                       DECLARE @FIN_ID INT;
                       SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                       select FyEndDate from FinancialYear  WHERE FyId = @FIN_ID";
                return connection.Query<DateTime>(qry, new { OrganizationId = OrganizationId }).First();
            }
        }
    }
}
