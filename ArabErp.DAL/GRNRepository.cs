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

        /// <summary>
        /// Insert GRN
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        public int InsertGRN(GRN model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into GRN(GRNNo,GRNDate,SupplyOrderId,SupplierId,SONoAndDate,CurrencyId,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,
                                                   Addition,AdditionRemarks,Deduction,DeductionRemarks,CreatedBy,CreatedDate,OrganizationId) 
                                            Values (@GRNNo,@GRNDate,@SupplyOrderId,@SupplierId,@SONODATE,@CurrencyId,@StockPointId,@SupplierDCNoAndDate,@SpecialRemarks,
                                                   @Addition,@AdditionRemarks,@Deduction,@DeductionRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, model, trn).Single();
                    var saleorderitemrepo = new GRNItemRepository();
                    foreach (var item in model.Items)
                    {
                        item.GRNId = id;
                        new GRNItemRepository().InsertGRNItem(item, connection, trn);
                        //new GRNItemRepository().InsertStockUpdate(item, connection, trn);
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

        public int InsertStockUpdate(GRN model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                int id = 0;

                foreach (var item in model.Items)
                {
                    //item.GRNId = id;

                    string sql = @"insert  into StockUpdate(StockPointId,StocktrnId,StockUserId,stocktrnDate,
                                 ItemId,Quantity,StockType,StockInOut,StockDescription,
                                 CreatedBy,CreatedDate,OrganizationId) 
                                Values (@stockpointId,@GRNId,@GRNNo,@GRNDate,
                                @ItemId,@Quantity,'GRN','IN',@Supplier,
                                @CreatedBy,@CreatedDate,@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";

                    //id = connection.Query<int>(sql, objOpeningStock).Single();

                    id = connection.Query<int>(sql, new
                    {
                        stockpointId = model.StockPointId,
                        GRNId = item.GRNId,
                        GRNNo = model.GRNNo,
                        GRNDate = model.GRNDate,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        Supplier = model.Supplier,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate,
                        OrganizationId = model.OrganizationId
                    }).Single();

                }


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
         
        public IEnumerable<GRN> GetGRNPreviousList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT GRNId,GRNNo,GRNDate,S.SupplierName Supplier,SupplierDCNoAndDate";
                query += " FROM GRN G INNER JOIN Supplier S ON S.SupplierId=G.SupplierId";
                return connection.Query<GRN>(query);
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

        public List<Dropdown> FillCurrency()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CurrencyId Id,CurrencyName Name from Currency").ToList();
            }
        }

        /// <summary>
        /// Returns All Pending GRN
        /// </summary>
        /// <returns></returns>
        /// 
        public IEnumerable<PendingSupplyOrder> GetGRNPendingList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"SELECT
	                            SO.SupplyOrderId,
                            CONCAT(SO.SupplyOrderId,' - ',CONVERT(VARCHAR(15),SupplyOrderDate,106))SoNoWithDate,
                            QuotaionNoAndDate
                            FROM SupplyOrder SO 
	                            INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId AND SO.SupplierId = 3
	                            LEFT JOIN GRN G ON G.SupplyOrderId=SO.SupplyOrderId
                            WHERE SO.isActive=1 and G.SupplyOrderId is null";

                return connection.Query<PendingSupplyOrder>(qry);
            }
        }

        /// <summary>
        /// Return details of a GRN such as Supplier Id, Supplier Name, Quotaion No And Date
        /// </summary>
        /// <returns></returns>

        public GRN GetGRNDetails(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "SELECT SO.SupplyOrderId,S.SupplierId,S.SupplierName Supplier,CONCAT(SupplyOrderId,'/',CONVERT(VARCHAR(15),SupplyOrderDate,104))SONODATE,QuotaionNoAndDate,GETDATE() GRNDate";
                qry += " FROM SupplyOrder SO";
                qry += " INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId";
                qry += " where SO.SupplyOrderId = " + SupplyOrderId.ToString();

                GRN workshoprequest = connection.Query<GRN>(qry).FirstOrDefault();
                return workshoprequest;
            }
        }

     

        /// <summary>
        /// Returns all pending GRNs against Supply Order
        /// </summary>
        /// <returns></returns>
        /// 
        public List<GRNItem> GetGRNItem(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,So.OrderedQty-isnull(sum(GI.Quantity),0) Quantity,";
                query += " So.OrderedQty-isnull(sum(GI.Quantity),0) PendingQuantity,UnitName Unit,SO.Rate,SO.Discount,SO.Amount";
                query += " FROM SupplyOrderItem SO";
                query += " INNER JOIN PurchaseRequestItem PR ON PR.PurchaseRequestItemId=SO.PurchaseRequestItemId";
                query += " INNER JOIN Item I ON I.ItemId=PR.ItemId";
                query += " INNER JOIN Unit ON UnitId =I.ItemUnitId";
                query += " LEFT JOIN GRN G ON G.SupplyOrderId=SO.SupplyOrderId";
                query += " LEFT JOIN GRNItem GI ON G.GRNId=GI.GRNId and GI.SupplyOrderItemId=SO.SupplyOrderItemId";
                query += " WHERE SO.SupplyOrderId=@SupplyOrderId";
                query += " GROUP BY SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,So.OrderedQty,UnitName,SO.Rate,SO.Discount,SO.Amount ";
                //query += " HAVING So.OrderedQty-isnull(sum(GI.Quantity),0)>0";

                return connection.Query<GRNItem>(query, new { SupplyOrderId = SupplyOrderId }).ToList();


            }
        }


        public GRN GetGRNDISPLAYDetails(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = " SELECT GRNId,GRNNo,GRNDate,S.SupplierName Supplier,SONoAndDate SONODATE,";
                qry += " SpecialRemarks,WareHouseId StockPointId,StockPointName,SupplierDCNoAndDate,";
                qry += " SupplyOrderId SupplierId,AdditionRemarks,Addition Addition,";
                qry += " DeductionRemarks,Deduction Deduction,G.CurrencyId CurrencyId,C.CurrencyName";
                qry += " FROM GRN G";
                qry += " INNER JOIN Supplier S ON S.SupplierId=G.SupplierId";
                qry += " INNER JOIN Stockpoint SP On SP.StockPointId=G.WareHouseId";
                qry += " INNER JOIN Currency C On C.CurrencyId=G.CurrencyId";
                qry += " WHERE G.GRNId = " + GRNId.ToString();

                GRN workshoprequest = connection.Query<GRN>(qry).FirstOrDefault();
                return workshoprequest;
            }
        }


        public List<GRNItem> GetGRNDISPLAYItem(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = " SELECT GRNId,SupplyOrderItemId,I.ItemId,I.ItemName,G.PartNo,Quantity,Quantity PendingQuantity,Unit,Rate,Discount,Amount,Remarks";
                query += " FROM GRNItem G";
                query += " INNER JOIN Item I ON I.ItemId=G.ItemId";
                query += " WHERE G.GRNId = " + GRNId.ToString();

                return connection.Query<GRNItem>(query, new { GRNId = GRNId }).ToList();


            }
        }

      }
}