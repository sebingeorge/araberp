using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class DesignationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertDesignation(Designation objDesignation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO Designation(DesignationRefNo,DesignationName,CreatedBy,CreatedDate,OrganizationId) VALUES(@DesignationRefNo,@DesignationName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
                var id = connection.Query<int>(sql, objDesignation).Single();
                return id;
            }
        }

        public IEnumerable<Designation> FillDesignationList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Designation>("SELECT DesignationRefNo,DesignationName FROM Designation").ToList();
            }
        }

        public Designation GetDesignation(int DesignationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Designation
                        where DesignationId=@DesignationId";

                var objDesignation = connection.Query<Designation>(sql, new
                {
                    DesignationId = DesignationId
                }).First<Designation>();

                return objDesignation;
            }
        }

        public List<Designation> GetDesignation()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Designation
                        where OrganizationId>0";

                var objDesignation = connection.Query<Designation>(sql).ToList<Designation>();

                return objDesignation;
            }
        }

    }
  
}
