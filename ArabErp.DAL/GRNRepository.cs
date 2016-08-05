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
        public string InsertGRN(GRN model)
        {

            model.Items = model.Items.Where(m => m.AcceptedQuantity > 0).ToList<GRNItem>();
            if (model.Items != null && model.Items.Count > 0)
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    IDbTransaction trn = connection.BeginTransaction();
                    try
                    {
                        int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(GRN).Name, "0", 1);
                        model.GRNNo = "GRN/0/" + internalId;
                        model.GrandTotal = model.Items.Sum(m => ((m.AcceptedQuantity * m.Rate) - m.Discount)) + model.Addition - model.Deduction;

                        int id = 0;

                        string sql = @"INSERT INTO GRN(GRNNo,GRNDate,SupplierId,CurrencyId,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,
                                                   Addition,AdditionId,Deduction,DeductionId,CreatedBy,CreatedDate,OrganizationId,isDirectPurchaseGRN, GrandTotal, VehicleNo, GatePassNo, ReceivedBy) 
                                            VALUES (@GRNNo,@GRNDate,@SupplierId,@CurrencyId,@StockPointId,@SupplierDCNoAndDate,@SpecialRemarks,
                                                   @Addition,@AdditionId,@Deduction,@DeductionId,@CreatedBy,@CreatedDate,@OrganizationId,@isDirectPurchaseGRN, @GrandTotal, @VehicleNo, @GatePassNo, @ReceivedBy);
                                            SELECT CAST(SCOPE_IDENTITY() AS INT)";

                        id = connection.Query<int>(sql, model, trn).Single();
                        foreach (var item in model.Items)
                        {
                            item.GRNId = id;
                            item.Amount = item.AcceptedQuantity * item.Rate;
                            new GRNItemRepository().InsertGRNItem(item, connection, trn);
                            new StockUpdateRepository().InsertStockUpdate(
                                new StockUpdate
                                {
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = model.CreatedDate,
                                    OrganizationId = model.OrganizationId,
                                    ItemId = item.ItemId,
                                    Quantity = item.AcceptedQuantity,
                                    StockInOut = "IN",
                                    StockPointId = model.StockPointId,
                                    StockType = model.isDirectPurchaseGRN ? "DirectGRN" : "GRN",
                                    StocktrnId = id,
                                    StockUserId = model.GRNNo,
                                    stocktrnDate = model.GRNDate
                                }, connection, trn);
                        }

                        trn.Commit();
                        return model.GRNNo;
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        throw ex;
                    }
                }
            }
            else
                throw new NullReferenceException("1You have to fill the quantity of atleast one material");
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
                        Quantity = item.AcceptedQuantity,
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
                string query = @"SELECT 
	                                GRNId,
	                                GRNNo+' - '+CONVERT(VARCHAR, GRNDate, 106) GRNNo,
	                                ISNULL(S.SupplierName, '-') Supplier,
	                                ISNULL(SupplierDCNoAndDate, '-') SupplierDCNoAndDate,
	                                EMP.EmployeeName ReceivedByName,
	                                ST.StockPointName,
	                                ISNULL(G.GrandTotal, 0.00) GrandTotal,
	                                G.isDirectPurchaseGRN
                                FROM GRN G INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                                INNER JOIN Employee EMP ON G.ReceivedBy = EMP.EmployeeId
                                INNER JOIN Stockpoint ST ON G.WareHouseId = ST.StockPointId
                                WHERE ISNULL(G.isActive, 1) = 1
                                ORDER BY GRNDate DESC, G.CreatedDate DESC;";
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
                return connection.Query<Stockpoint>("select * from Stockpoint where isActive=1");
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
        public IEnumerable<PendingForGRN> GetGRNPendingList(int supplierId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    //                    string qry = @"SELECT
                    //	                            SO.SupplyOrderId,
                    //                            CONCAT(SO.SupplyOrderId,' - ',CONVERT(VARCHAR(15),SupplyOrderDate,106))SoNoWithDate,
                    //                            QuotaionNoAndDate
                    //                            FROM SupplyOrder SO 
                    //	                            INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId AND SO.SupplierId = @supplierId
                    //	                            LEFT JOIN GRN G ON G.SupplyOrderId=SO.SupplyOrderId
                    //                            WHERE SO.isActive=1 and G.SupplyOrderId is null";

                    string qry = @"SELECT
	                                    DISTINCT SO.SupplyOrderId,
	                                    SO.SupplyOrderDate,
	                                    SO.CreatedDate,
	                                    CONCAT(SO.SupplyOrderNo,' - ',ISNULL(CONVERT(VARCHAR(15),SupplyOrderDate,106), ''))SoNoWithDate,
	                                    ISNULL(QuotaionNoAndDate, '-')QuotaionNoAndDate,
	                                    DATEDIFF(day, SupplyOrderDate, GETDATE()) Age,
	                                    ISNULL(SpecialRemarks, '-') SpecialRemarks,
	                                    ISNULL(CONVERT(VARCHAR(15),RequiredDate,106), '-') RequiredDate,
										S.SupplierId,
										S.SupplierName
                                    FROM SupplyOrder SO 
	                                    INNER JOIN SupplyOrderItem SOI ON SO.SupplyOrderId = SOI.SupplyOrderId
	                                    INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId AND SO.SupplierId = ISNULL(NULLIF(@supplierId, 0), SO.SupplierId)
	                                    LEFT JOIN GRNItem GI ON SOI.SupplyOrderItemId = GI.SupplyOrderItemId
                                    WHERE SO.isActive=1 and 
                                    (GI.SupplyOrderItemId IS NULL OR ISNULL(GI.Quantity, 0) < ISNULL(SOI.OrderedQty, 0))
                                    ORDER BY SO.SupplyOrderDate DESC, CreatedDate DESC";

                    return connection.Query<PendingForGRN>(qry, new { supplierId = supplierId });
                }
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Return details of a GRN such as Supplier Id
        /// </summary>
        /// <returns></returns>

        public GRN GetGRNDetails(int? supplierId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    return connection.Query<GRN>(@"SELECT Supplierid, SupplierName Supplier FROM Supplier WHERE SupplierId = @supplierId",
                        new { supplierId = supplierId }
                        ).FirstOrDefault();
                }
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all supply order items against multiple Supply Order ids
        /// </summary>
        /// <returns></returns>
        /// 
        public List<GRNItem> GetGRNItem(List<int?> id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string query = "SELECT SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,So.OrderedQty-isnull(sum(GI.Quantity),0) Quantity,";
                //query += " So.OrderedQty-isnull(sum(GI.Quantity),0) PendingQuantity,UnitName Unit,SO.Rate,SO.Discount,SO.Amount";
                //query += " FROM SupplyOrderItem SO";
                //query += " INNER JOIN PurchaseRequestItem PR ON PR.PurchaseRequestItemId=SO.PurchaseRequestItemId";
                //query += " INNER JOIN Item I ON I.ItemId=PR.ItemId";
                //query += " INNER JOIN Unit ON UnitId =I.ItemUnitId";
                //query += " LEFT JOIN GRN G ON G.SupplyOrderId=SO.SupplyOrderId";
                //query += " LEFT JOIN GRNItem GI ON G.GRNId=GI.GRNId and GI.SupplyOrderItemId=SO.SupplyOrderItemId";
                //query += " WHERE SO.SupplyOrderId=@SupplyOrderId";
                //query += " GROUP BY SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,SO.SupplyOrderItemId,I.ItemName,I.ItemId,I.PartNo,So.OrderedQty,UnitName,SO.Rate,SO.Discount,SO.Amount ";
                //query += " HAVING So.OrderedQty-isnull(sum(GI.Quantity),0)>0";

                string query = @"SELECT
	                                SupplyOrderItemId,
	                                SUM(Quantity) Quantity
                                INTO #GRN
                                FROM GRNItem
                                WHERE SupplyOrderItemId IS NOT NULL
	                                AND ISNULL(isActive, 1) = 1
                                GROUP BY SupplyOrderItemId;
                           
                                SELECT
                                    ROW_NUMBER() OVER(ORDER BY SOI.SupplyOrderItemId) AS SlNo,
	                                SOI.SupplyOrderId,
	                                SOI.SupplyOrderItemId,
	                                I.ItemId,
	                                I.ItemName,
	                                I.PartNo,
	                                U.UnitName Unit,
	                                (ISNULL(SOI.OrderedQty, 0) - ISNULL(GRN.Quantity, 0)) PendingQuantity,
	                                (ISNULL(SOI.OrderedQty, 0) - ISNULL(GRN.Quantity, 0)) ReceivedQuantity,
	                                (ISNULL(SOI.OrderedQty, 0) - ISNULL(GRN.Quantity, 0)) AcceptedQuantity,
                                    0 AS RejectedQuantity,
	                                ISNULL(SOI.Rate, 0.00) Rate,
	                                ISNULL(SOI.Discount, 0.00) Discount,
	                                ISNULL(SOI.Amount, 0.00) Amount
                                FROM SupplyOrderItem SOI
                                INNER JOIN SupplyOrder SO ON SOI.SupplyOrderId = SO.SupplyOrderId
                                INNER JOIN PurchaseRequestItem PR ON SOI.PurchaseRequestItemId = PR.PurchaseRequestItemId
                                INNER JOIN Item I ON PR.ItemId = I.ItemId
                                INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
                                LEFT JOIN #GRN GRN ON SOI.SupplyOrderItemId = GRN.SupplyOrderItemId
                                WHERE (SOI.SupplyOrderItemId IS NULL OR ISNULL(GRN.Quantity, 0) < ISNULL(SOI.OrderedQty, 0))
                                AND SO.SupplyOrderId IN @id
                                AND ISNULL(SOI.isActive, 1) = 1 AND ISNULL(PR.isActive, 1) = 1;
                                DROP TABLE #GRN;";

                return connection.Query<GRNItem>(query, new { id = id }).ToList();
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

        /// <summary>
        /// Return all pending direct purchase requests (requests that are not in GRN)
        /// </summary>
        /// <returns></returns>
        public List<PendingForGRN> GetPendingDirectPurchase()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string query = @"SELECT
//	                                DISTINCT DP.DirectPurchaseRequestId,
//	                                ISNULL(DP.PurchaseRequestNo, '') +' - '+ CONVERT(VARCHAR, ISNULL(DP.PurchaseRequestDate, ''), 106) RequestNoAndDate,
//									DP.CreatedDate,
//                                    DP.PurchaseRequestDate,
//                                    ISNULL(DP.SpecialRemarks, '-') SpecialRemarks,
//	                                ISNULL(DP.TotalAmount, 0.00) TotalAmount,
//                                    DATEDIFF(day, DP.PurchaseRequestDate, GETDATE()) Age,
//									ISNULL(CONVERT(VARCHAR, RequiredDate, 106), '-')RequiredDate,
//                                    1 AS isDirectPurchase
//                                FROM DirectPurchaseRequestItem DPI
//                                INNER JOIN DirectPurchaseRequest DP ON DPI.DirectPurchaseRequestId = DP.DirectPurchaseRequestId
//                                LEFT JOIN GRNItem GRN ON DPI.DirectPurchaseRequestItemId = GRN.DirectPurchaseRequestItemId
//                                WHERE GRN.DirectPurchaseRequestItemId IS NULL AND ISNULL(DP.isApproved, 0) = 1
//                                AND ISNULL(DP.isActive, 1) = 1 AND ISNULL(DPI.isActive, 1) = 1 AND ISNULL(GRN.isActive, 1) = 1
//                                ORDER BY DP.PurchaseRequestDate DESC, DP.CreatedDate DESC";

                string query = @"SELECT
	                                DirectPurchaseRequestItemId,
	                                SUM(Quantity) Quantity
                                INTO #GRN
                                FROM GRNItem
                                WHERE DirectPurchaseRequestItemId IS NOT NULL
	                                AND ISNULL(isActive, 1) = 1
                                GROUP BY DirectPurchaseRequestItemId;
                                SELECT
	                                DISTINCT DP.DirectPurchaseRequestId,
	                                --GRN.quantity,DPI.Quantity,
	                                ISNULL(DP.PurchaseRequestNo, '') +' - '+ CONVERT(VARCHAR, ISNULL(DP.PurchaseRequestDate, ''), 106) RequestNoAndDate,
	                                DP.CreatedDate,
                                    DP.PurchaseRequestDate,
                                    ISNULL(DP.SpecialRemarks, '-') SpecialRemarks,
	                                ISNULL(DP.TotalAmount, 0.00) TotalAmount,
                                    DATEDIFF(day, DP.PurchaseRequestDate, GETDATE()) Age,
	                                ISNULL(CONVERT(VARCHAR, RequiredDate, 106), '-')RequiredDate,
                                    1 AS isDirectPurchase
	                                --(DPI.Quantity - ISNULL(GRN.Quantity,0)) PendingQuantity,
	                                --DPI.Quantity RequiredQuantity
                                FROM DirectPurchaseRequestItem DPI
                                INNER JOIN DirectPurchaseRequest DP ON DPI.DirectPurchaseRequestId = DP.DirectPurchaseRequestId
                                LEFT JOIN #GRN GRN ON DPI.DirectPurchaseRequestItemId = GRN.DirectPurchaseRequestItemId
                                WHERE (GRN.DirectPurchaseRequestItemId IS NULL OR ISNULL(GRN.Quantity, 0) < ISNULL(DPI.Quantity, 0))
                                AND ISNULL(DP.isApproved, 0) = 1
                                AND ISNULL(DP.isActive, 1) = 1 AND ISNULL(DPI.isActive, 1) = 1
                                ORDER BY DP.PurchaseRequestDate DESC, DP.CreatedDate DESC;
                                DROP TABLE #GRN;";

                return connection.Query<PendingForGRN>(query).ToList();
            }
        }

        public List<GRNItem> GetDirectGRNItems(List<int?> id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string query = @"SELECT
//                                    DPI.DirectPurchaseRequestId,
//	                                DPI.DirectPurchaseRequestItemId,
//	                                DPI.ItemId,
//	                                I.ItemName,
//	                                I.PartNo,
//	                                U.UnitName Unit,
//	                                ROW_NUMBER() OVER(ORDER BY DPI.DirectPurchaseRequestItemId) AS SlNo,
//	                                ISNULL(DPI.Remarks, '') Remarks,
//	                                ISNULL(DPI.Quantity, 0) PendingQuantity,
//	                                ISNULL(DPI.Quantity, 0) ReceivedQuantity,
//	                                ISNULL(DPI.Quantity, 0) AcceptedQuantity,
//	                                0 RejectedQuantity,
//	                                0.00 AS Discount,
//	                                ISNULL(DPI.Rate, 0.00) Rate,
//	                                CAST(ISNULL(DPI.Quantity, 0.00)*ISNULL(DPI.Rate, 0.00) AS DECIMAL(18, 2)) Amount
//                                FROM DirectPurchaseRequestItem DPI
//                                INNER JOIN Item I ON DPI.ItemId = I.ItemId
//                                INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
//                                WHERE DPI.DirectPurchaseRequestId IN @id
//                                AND ISNULL(DPI.isActive, 1) = 1";
                string query = @"SELECT
	                                G.DirectPurchaseRequestItemId,
	                                SUM(G.Quantity) Quantity
                                INTO #GRN
                                FROM GRNItem G
                                INNER JOIN DirectPurchaseRequestItem DPI ON G.DirectPurchaseRequestItemId = DPI.DirectPurchaseRequestItemId
                                WHERE DirectPurchaseRequestId IN @id
	                                AND ISNULL(DPI.isActive, 1) = 1
	                                AND ISNULL(G.isActive, 1) = 1
                                GROUP BY G.DirectPurchaseRequestItemId;
                                SELECT
                                    DPI.DirectPurchaseRequestId,
	                                DPI.DirectPurchaseRequestItemId,
	                                DPI.ItemId,
	                                I.ItemName,
	                                I.PartNo,
	                                U.UnitName Unit,
	                                ROW_NUMBER() OVER(ORDER BY DPI.DirectPurchaseRequestItemId) AS SlNo,
	                                ISNULL(DPI.Remarks, '') Remarks,
	                                (ISNULL(DPI.Quantity, 0) - ISNULL(G.Quantity, 0)) PendingQuantity,
	                                (ISNULL(DPI.Quantity, 0) - ISNULL(G.Quantity, 0)) ReceivedQuantity,
	                                (ISNULL(DPI.Quantity, 0) - ISNULL(G.Quantity, 0)) AcceptedQuantity,
	                                0 RejectedQuantity,
	                                0.00 AS Discount,
	                                ISNULL(DPI.Rate, 0.00) Rate,
	                                CAST(ISNULL(DPI.Quantity, 0.00)*ISNULL(DPI.Rate, 0.00) AS DECIMAL(18, 2)) Amount
                                FROM DirectPurchaseRequestItem DPI
                                INNER JOIN Item I ON DPI.ItemId = I.ItemId
                                INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
                                LEFT JOIN #GRN G ON DPI.DirectPurchaseRequestItemId = G.DirectPurchaseRequestItemId
                                WHERE DPI.DirectPurchaseRequestId IN @id
                                    AND (ISNULL(DPI.Quantity, 0) - ISNULL(G.Quantity, 0)) > 0
                                    AND ISNULL(DPI.isActive, 1) = 1;
                                DROP TABLE #GRN;";

                return connection.Query<GRNItem>(query, new { id = id }).ToList();
            }
        }
    }
}