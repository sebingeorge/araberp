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

        public VehicleModel InsertVehicleModel(VehicleModel model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"INSERT INTO VehicleModel(VehicleModelRefNo,VehicleModelName,VehicleModelDescription,CreatedBy,CreatedDate,OrganizationId) VALUES(@VehicleModelRefNo,@VehicleModelName,@VehicleModelDescription,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
                int id = 0;
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(VehicleModel).Name, "0", 1);
                    model.VehicleModelRefNo = "VM/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.VehicleModelId = id;

                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.VehicleModelId = 0;
                    model.VehicleModelRefNo = null;
                }
                return model;
            }
        }

        public IEnumerable<VehicleModel> FillVehicleModelList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<VehicleModel>("SELECT VehicleModelId,VehicleModelRefNo,VehicleModelName,VehicleModelDescription  FROM VehicleModel where isActive=1").ToList();
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

        public VehicleModel UpdateVehicleModel(VehicleModel model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE VehicleModel SET VehicleModelName= @VehicleModelName,VehicleModelDescription = @VehicleModelDescription, CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.VehicleModelId  WHERE VehicleModelId = @VehicleModelId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.VehicleModelId = id;
                }
                catch (Exception ex)
                {

                    model.VehicleModelId = 0;

                }
                return model;
            }
        }
        public VehicleModel DeleteVehicleModel(VehicleModel model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE VehicleModel SET isActive = 0 OUTPUT INSERTED.VehicleModelId  WHERE VehicleModelId = @VehicleModelId";
                
                try
                {
                    var id = connection.Execute(sql, model);
                    model.VehicleModelId = id;
                }
                catch (Exception ex)
                {

                    model.VehicleModelId = 0;

                }
                return model;
            }
        }


    }
   
}
