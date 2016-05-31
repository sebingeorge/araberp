using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkDescriptionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertWorkDescription(WorkDescription objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into WorkDescription(VehicleModelId,FreezerUnitId,BoxId,WorkDescr,CreatedBy,CreatedDate,OrganizationId) Values (@VehicleModelId,@FreezerUnitId,@BoxId,@WorkDescr,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkDescription).Single();
                return id;
            }
        }


        public WorkDescription GetWorkDescription(int WorkDescriptionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkDescription
                        where WorkDescriptionId=@WorkDescriptionId";

                var objWorkDescription = connection.Query<WorkDescription>(sql, new
                {
                    WorkDescriptionId = WorkDescriptionId
                }).First<WorkDescription>();

                return objWorkDescription;
            }
        }

        public List<WorkDescription> GetWorkDescriptions()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkDescription
                        where isActive=1";

                var objWorkDescriptions = connection.Query<WorkDescription>(sql).ToList<WorkDescription>();

                return objWorkDescriptions;
            }
        }

        public int UpdateWorkDescription(WorkDescription objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE WorkDescription SET VehicleModelId = @VehicleModelId ,FreezerUnitId = @FreezerUnitId ,BoxId = @BoxId ,WorkDescr = @WorkDescr,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkDescriptionId  WHERE WorkDescriptionId = @WorkDescriptionId";


                var id = connection.Execute(sql, objWorkDescription);
                return id;
            }
        }

        public int DeleteWorkDescription(Unit objWorkDescription)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkDescription  OUTPUT DELETED.WorkDescriptionId WHERE WorkDescriptionId=@WorkDescriptionId";


                var id = connection.Execute(sql, objWorkDescription);
                return id;
            }
        }


    }
}