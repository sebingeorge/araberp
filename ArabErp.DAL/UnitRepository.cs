using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class UnitRepository : BaseRepository
    {

        public int InsertUnit(Unit objUnit)
        {
            string sql = @"INSERT INTO Unit (UnitRefNo,UnitName,CreatedBy,CreatedDate,OrganizationId,isActive) VALUES(@UnitRefNo,@UnitName,@CreatedBy,getDate(),@OrganizationId,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";



            var id = connection.Query<int>(sql, objUnit).Single();
            return id;
        }

        public IEnumerable<Unit> FillUnitList()
        {

            return connection.Query<Unit>("SELECT UnitId,UnitRefNo,UnitName FROM Unit").ToList();
        }
        

        public Unit GetUnit(int UnitId)
        {

            string sql = @"select * from Unit
                        where UnitId=@UnitId";

            var objUnit= connection.Query<Unit>(sql, new
            {
                UnitId = UnitId
            }).First<Unit>();

            return objUnit;
        }

        public List<Unit> GetUnits()
        {
            string sql = @"select * from Unit
                        where isActive=1";

            var objUnits = connection.Query<Unit>(sql).ToList<Unit>();

            return objUnits;
        }

        public int UpdateUnit(Unit objUnit)
        {
            string sql = @"Update Unit Set UnitRefNo=@UnitRefNo,UnitName=@UnitName OUTPUT INSERTED.UnitId WHERE UnitId=@UnitId";


            var id = connection.Execute(sql, objUnit);
            return id;
        }

        public int DeleteUnit(Unit objUnit)
        {
            string sql = @"Delete Unit  OUTPUT DELETED.UnitId WHERE UnitId=@UnitId";


            var id = connection.Execute(sql, objUnit);
            return id;
        }

    }
}
