using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class VehicleOutPassRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertVehicleOutPass(VehicleOutPass objVehicleOutPass)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into VehicleOutPass(JobCardId,RegistrationNo,VehicleInPassDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardId,@RegistrationNo,@VehicleInPassDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objVehicleOutPass).Single();
                return id;
            }
        }


        public VehicleOutPass GetVehicleOutPass(int VehicleOutPassId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleOutPass
                        where VehicleOutPassId=@VehicleOutPassId";

                var objVehicleOutPass = connection.Query<VehicleOutPass>(sql, new
                {
                    VehicleOutPassId = VehicleOutPassId
                }).First<VehicleOutPass>();

                return objVehicleOutPass;
            }
        }

        public List<VehicleOutPass> GetVehicleOutPasss()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleOutPass
                        where isActive=1";

                var objVehicleOutPasss = connection.Query<VehicleOutPass>(sql).ToList<VehicleOutPass>();

                return objVehicleOutPasss;
            }
        }



        public int DeleteVehicleOutPass(Unit objVehicleOutPass)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete VehicleOutPass  OUTPUT DELETED.VehicleOutPassId WHERE VehicleOutPassId=@VehicleOutPassId";


                var id = connection.Execute(sql, objVehicleOutPass);
                return id;
            }
        }


    }
}
