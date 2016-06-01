using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class VehicleInPassRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertVehicleInPass(VehicleInPass objVehicleInPass)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into VehicleInPass(SaleOrderId,RegistrationNo,VehicleInPassDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId) Values (@SaleOrderId,@RegistrationNo,@VehicleInPassDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objVehicleInPass).Single();
                return id;
            }
        }


        public VehicleInPass GetVehicleInPass(int VehicleInPassId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where VehicleInPassId=@VehicleInPassId";

                var objVehicleInPass = connection.Query<VehicleInPass>(sql, new
                {
                    VehicleInPassId = VehicleInPassId
                }).First<VehicleInPass>();

                return objVehicleInPass;
            }
        }

        public List<VehicleInPass> GetVehicleInPasss()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where isActive=1";

                var objVehicleInPasss = connection.Query<VehicleInPass>(sql).ToList<VehicleInPass>();

                return objVehicleInPasss;
            }
        }



        public int DeleteVehicleInPass(Unit objVehicleInPass)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete VehicleInPass  OUTPUT DELETED.VehicleInPassId WHERE VehicleInPassId=@VehicleInPassId";


                var id = connection.Execute(sql, objVehicleInPass);
                return id;
            }
        }


    }
}