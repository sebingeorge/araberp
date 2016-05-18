using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SalesExecutiveRepository : BaseRepository
    {

        public int InsertSalesExecutive(SalesExecutive objSalesExecutive)
        {
            string sql = @"INSERT INTO SalesExecutive (SalesExecutiveName,CreatedBy,CreatedDate,OrganizationId) VALUES(@SalesExecutiveName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSalesExecutive).Single();
            return id;
        }


        public SalesExecutive GetSalesExecutive(int SalesExecutiveId)
        {

            string sql = @"select * from SalesExecutive
                        where SalesExecutiveId=@SalesExecutiveId";

            var objSalesExecutive = connection.Query<SalesExecutive>(sql, new
            {
                SalesExecutiveId = SalesExecutiveId
            }).First<SalesExecutive>();

            return objSalesExecutive;
        }

        public List<SalesExecutive> GetSalesExecutives()
        {
            string sql = @"select * from SalesExecutive
                        where isActive=1";

            var objSalesExecutives = connection.Query<SalesExecutive>(sql).ToList<SalesExecutive>();

            return objSalesExecutives;
        }

        public int UpdateSalesExecutive(SalesExecutive objSalesExecutive)
        {
            string sql = @"Update SalesExecutive Set SalesExecutiveName=@SalesExecutiveName OUTPUT INSERTED.SalesExecutiveId WHERE SalesExecutiveId=@SalesExecutiveId";


            var id = connection.Execute(sql, objSalesExecutive);
            return id;
        }

        public int DeleteSalesExecutive(Unit objSalesExecutive)
        {
            string sql = @"Delete SalesExecutive  OUTPUT DELETED.SalesExecutiveId WHERE SalesExecutiveId=@SalesExecutiveId";


            var id = connection.Execute(sql, objSalesExecutive);
            return id;
        }


    }
}