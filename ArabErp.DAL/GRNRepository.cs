using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class GRNRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertGRN(GRN objGRN)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into GRN(GRNNo,GRNDate,SupplierId,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@GRNNo,@GRNDate,@SupplierId,@WareHouseId,@SupplierDCNoAndDate,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objGRN).Single();
                return id;
            }
        }


        public GRN GetGRN(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from GRN
                        where GRNId=@GRNId";

                var objGRN = connection.Query<GRN>(sql, new
                {
                    GRNId = GRNId
                }).First<GRN>();

                return objGRN;
            }
        }

        public List<GRN> GetGRNs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from GRN
                        where isActive=1";

                var objGRNs = connection.Query<GRN>(sql).ToList<GRN>();

                return objGRNs;
            }
        }

        public int UpdateGRN(GRN objGRN)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE GRN SET GRNNo = @GRNNo ,GRNDate = @GRNDate ,SupplierId = @SupplierId ,WareHouseId = @WareHouseId,SupplierDCNoAndDate = @SupplierDCNoAndDate  OUTPUT INSERTED.GRNId  WHERE GRNId = @GRNId";


                var id = connection.Execute(sql, objGRN);
                return id;
            }
        }

        public int DeleteGRN(Unit objGRN)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete GRN  OUTPUT DELETED.GRNId WHERE GRNId=@GRNId";


                var id = connection.Execute(sql, objGRN);
                return id;
            }
        }

        public IEnumerable<Stockpoint> GetWarehouseList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Stockpoint>("select * from Stockpoint");
            }
        }

        public List<GRNItem> GetGRNData(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT SELECT I.ItemName,I.ItemId,I.PartNo,OrderedQty,UnitName,Rate,Discount,Amount FROM SupplyOrderItem INNER JOIN Item I ON PurchaseRequestItemId=I.ItemId INNER JOIN Unit ON UnitId =I.ItemUnitId WHERE SupplyOrderId @SupplyOrderId";

                return connection.Query<GRNItem>(query,
                new { SupplyOrderId = SupplyOrderId }).ToList();


            }
        }

    }
}