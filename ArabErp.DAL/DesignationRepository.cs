using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class DesignationRepository : BaseRepository
    {
        public int InsertDesignation(Designation objDesignation)
        {
            string sql = @"INSERT INTO Designation(DesignationRefNo,DesignationName,CreatedBy,CreatedDate,OrganizationId) VALUES(@DesignationRefNo,@DesignationName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = connection.Query<int>(sql, objDesignation).Single();
            return id;
        }
         
        public IEnumerable<Designation> FillDesignationList()
        {
            return connection.Query<Designation>("SELECT DesignationRefNo,DesignationName FROM Designation").ToList();
        }

        public Designation GetDesignation(int DesignationId)
        {

            string sql = @"select * from Designation
                        where DesignationId=@DesignationId";

            var objDesignation = connection.Query<Designation>(sql, new
            {
                DesignationId = DesignationId
            }).First<Designation>();

            return objDesignation;
        }

        public List<Designation> GetDesignation()
        {
            string sql = @"select * from Designation
                        where OrganizationId>0";

            var objDesignation = connection.Query<Designation>(sql).ToList<Designation>();

            return objDesignation;
        }

    }
  
}
