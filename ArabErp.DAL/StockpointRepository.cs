using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class StockpointRepository : BaseRepository
    {

        public int InsertStockpoint(Stockpoint objStockpoint)
        {
            string sql = @"insert  into StockPoint(StockPointRefNo,StockPointName,StockPointShrtName,StockPointDoorNo,StockPointZip,StockPointArea,StockPointPhone,StockPointCity,StockPointFax,CreatedBy,CreatedDate,OrganizationId) Values (@StockPointRefNo,@StockPointName,@StockPointShrtName,@StockPointDoorNo,@StockPointZip,@StockPointArea,@StockPointPhone,@StockPointCity,@StockPointFax,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objStockpoint).Single();
            return id;
        }

        public IEnumerable<Stockpoint> FillStockpointList()
        {
            return connection.Query<Stockpoint>("SELECT StockPointRefNo,StockPointName,StockPointShrtName FROM Stockpoint").ToList();
        }
        public Stockpoint GetStockpoint(int StockpointId)
        {

            string sql = @"select * from Stockpoint
                        where StockpointId=@StockpointId";

            var objStockpoint = connection.Query<Stockpoint>(sql, new
            {
                StockpointId = StockpointId
            }).First<Stockpoint>();

            return objStockpoint;
        }

        public List<Stockpoint> GetStockpoints()
        {
            string sql = @"select * from Stockpoint
                        where isActive=1";

            var objStockpoints = connection.Query<Stockpoint>(sql).ToList<Stockpoint>();

            return objStockpoints;
        }

        public int UpdateStockpoint(Stockpoint objStockpoint)
        {
            string sql = @"UPDATE StockPoint SET StockPointRefNo = @StockPointRefNo ,StockPointName = @StockPointName ,StockPointShrtName = @StockPointShrtName ,StockPointDoorNo = @StockPointDoorNo,StockPointZip = @StockPointZip,StockPointArea = @StockPointArea,StockPointPhone = @StockPointPhone,StockPointCity = @StockPointCity,StockPointFax = @StockPointFax  OUTPUT INSERTED.StockPointId  WHERE StockPointId = @StockPointId";


            var id = connection.Execute(sql, objStockpoint);
            return id;
        }

        public int DeleteStockpoint(Unit objStockpoint)
        {
            string sql = @"Delete Stockpoint  OUTPUT DELETED.StockpointId WHERE StockpointId=@StockpointId";


            var id = connection.Execute(sql, objStockpoint);
            return id;
        }


    }
}