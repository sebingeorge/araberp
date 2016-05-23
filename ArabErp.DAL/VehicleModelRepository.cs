using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class VehicleModelRepository : IDisposable
    {
         private SqlConnection connection;

         public VehicleModelRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public int InsertVehicleModel(VehicleModel objVehicleModel)
        {
            string sql = @"INSERT INTO VehicleModel(VehicleModelRefNo,VehicleModelName,VehicleModelDescription,CreatedBy,CreatedDate,OrganizationId) VALUES(@VehicleModelRefNo,@VehicleModelName,@VehicleModelDescription,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = connection.Query<int>(sql, objVehicleModel).Single();
            return id;
        }

        public IEnumerable<VehicleModel> FillVehicleModelList()
        {
            return connection.Query<VehicleModel>("SELECT VehicleModelRefNo,VehicleModelName,VehicleModelDescription  FROM VehicleModel").ToList();
        }

        public VehicleModel GetVehicleModel(int VehicleModelId)
        {

            string sql = @"select * from VehicleModel
                                    where VehicleModelId=@VehicleModelId";

            var objVehicleModel = connection.Query<VehicleModel>(sql, new
{
             VehicleModelId = VehicleModelId
                          }).First<VehicleModel>();

            return objVehicleModel;
        }

        public List<VehicleModel> GetVehicleModel()
        {
            string sql = @"select * from VehicleModel
                        where OrganizationId>0";

            var objVehicleModel = connection.Query<VehicleModel>(sql).ToList<VehicleModel>();

            return objVehicleModel;
        }

        public void Dispose()
        {
            connection.Dispose();
        }

    }
   
}
