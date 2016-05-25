using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class VehicleModelRepository : BaseRepository
    {
         //private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");

        // public VehicleModelRepository()
        //{
        //    if (connection == null)
        //    {
        //        connection = ConnectionManager.connection;
        //    }
        //}

        public int InsertVehicleModel(VehicleModel objVehicleModel)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO VehicleModel(VehicleModelRefNo,VehicleModelName,VehicleModelDescription,CreatedBy,CreatedDate,OrganizationId) VALUES(@VehicleModelRefNo,@VehicleModelName,@VehicleModelDescription,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
                var id = connection.Query<int>(sql, objVehicleModel).Single();
                return id;
            }
        }

        public IEnumerable<VehicleModel> FillVehicleModelList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<VehicleModel>("SELECT VehicleModelRefNo,VehicleModelName,VehicleModelDescription  FROM VehicleModel").ToList();
            }
        }
        public VehicleModel GetVehicleModel(int VehicleModelId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleModel
                                    where VehicleModelId=@VehicleModelId";

                var objVehicleModel = connection.Query<VehicleModel>(sql, new
    {
        VehicleModelId = VehicleModelId
    }).First<VehicleModel>();

                return objVehicleModel;
            }
        }

        public List<VehicleModel> GetVehicleModel()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleModel
                        where OrganizationId>0";

                var objVehicleModel = connection.Query<VehicleModel>(sql).ToList<VehicleModel>();

                return objVehicleModel;
            }
        }

        //public void Dispose()
        //{
        //    connection.Dispose();
        //}

    }
   
}
