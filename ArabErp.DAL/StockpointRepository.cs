using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockpointRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public Stockpoint InsertStockpoint(Stockpoint model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"insert  into StockPoint(StockPointRefNo,StockPointName,StockPointShrtName,StockPointDoorNo,StockPointZip,StockPointArea,
                                       StockPointPhone,StockPointCity,StockPointFax,CreatedBy,CreatedDate,OrganizationId) 
                                       Values (@StockPointRefNo,@StockPointName,@StockPointShrtName,@StockPointDoorNo,@StockPointZip,
                                       @StockPointArea,@StockPointPhone,@StockPointCity,@StockPointFax,@CreatedBy,getDate(),@OrganizationId);
                                       SELECT CAST(SCOPE_IDENTITY() as int)";
                int id = 0;
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Stockpoint).Name, "0", 1);
                    model.StockPointRefNo = "SP/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.StockPointId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Stock Point", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.StockPointId = 0;
                    model.StockPointRefNo = null;
                }
                return model;
        }
        }

        public IEnumerable<Stockpoint> FillStockpointList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Stockpoint>("SELECT StockPointId,StockPointRefNo,StockPointName,StockPointShrtName FROM Stockpoint where isActive=1").ToList();
        }
        }
        public Stockpoint GetStockpoint(int StockpointId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            string sql = @"select * from Stockpoint
                        where StockpointId=@StockpointId";

            var objStockpoint = connection.Query<Stockpoint>(sql, new
            {
                StockpointId = StockpointId
            }).First<Stockpoint>();

            return objStockpoint;
        }
        }

        public List<Stockpoint> GetStockpoints()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            string sql = @"select * from Stockpoint
                        where isActive=1";

            var objStockpoints = connection.Query<Stockpoint>(sql).ToList<Stockpoint>();

            return objStockpoints;
        }
        }

        public Stockpoint UpdateStockpoint(Stockpoint model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Stockpoint SET StockPointName = @StockPointName ,	StockPointShrtName = @StockPointShrtName,	StockPointDoorNo = @StockPointDoorNo,	StockPointZip = @StockPointZip 	,StockPointArea = @StockPointArea	,StockPointPhone = @StockPointPhone,	StockPointCity = @StockPointCity,	StockPointFax = @StockPointFax, CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.StockPointId  WHERE StockPointId = @StockPointId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.StockPointId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Update", "Stock Point", id.ToString(), "0");
                }
                catch (Exception ex)
                {

                    model.StockPointId = 0;

                }
                return model;
            }
        }

        public Stockpoint DeleteStockpoint(Stockpoint model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Stockpoint SET isActive = 0 OUTPUT INSERTED.StockPointId  WHERE StockPointId = @StockPointId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.StockPointId = id;
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Delete", "Stock Point", id.ToString(), "0");
                }
                catch (Exception ex)
                {

                    model.StockPointId = 0;

                }
                return model;
            }
        }



    }
}