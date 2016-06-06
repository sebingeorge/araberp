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

        public int InsertGRN(GRN model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into GRN(GRNNo,GRNDate,SupplierId,SONoAndDate,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) 
                                            Values (@GRNNo,@GRNDate,@SupplierId,@SONODATE,@StockPointId,@SupplierDCNoAndDate,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, model, trn).Single();
                    var saleorderitemrepo = new GRNItemRepository();
                    foreach (var item in model.Items)
                    {
                        item.GRNId = id;
                        new GRNItemRepository().InsertGRNItem(item, connection, trn);

                    }

                    trn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return 0;
                }


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

      
        public IEnumerable<PendingSupplyOrder> GetGRNPendingList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"SELECT SupplyOrderId,S.SupplierId,S.SupplierName,CONCAT(SupplyOrderId,'/',CONVERT(VARCHAR(15),SupplyOrderDate,104))SoNoWithDate,QuotaionNoAndDate";
                qry += " FROM SupplyOrder SO ";
                qry += " INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId ";
                qry += " WHERE SO.isActive=1 ";

                return connection.Query<PendingSupplyOrder>(qry);
            }
        }


        public GRN GetGRNDetails(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "SELECT S.SupplierId,S.SupplierName Supplier,CONCAT(SupplyOrderId,'/',CONVERT(VARCHAR(15),SupplyOrderDate,104))SONODATE,QuotaionNoAndDate,GETDATE() GRNDate";
                qry += " FROM SupplyOrder SO";
                qry += " INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId";
                qry += " where SO.SupplyOrderId = " + SupplyOrderId.ToString();

                GRN workshoprequest = connection.Query<GRN>(qry).FirstOrDefault();
                return workshoprequest;
            }
        }


        public List<GRNItem> GetGRNItem(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT I.ItemName,I.ItemId,I.PartNo,OrderedQty Quantity,UnitName Unit,Rate,Discount,Amount FROM SupplyOrderItem SO";
                query += " INNER JOIN PurchaseRequestItem PR ON PR.PurchaseRequestItemId=SO.PurchaseRequestItemId";
                query += " INNER JOIN Item I ON I.ItemId=PR.ItemId";
                query += " INNER JOIN Unit ON UnitId =I.ItemUnitId WHERE SupplyOrderId=@SupplyOrderId";
                return connection.Query<GRNItem>(query, new { SupplyOrderId = SupplyOrderId }).ToList();


            }
        }


      }
}