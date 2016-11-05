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
                        var internalId = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 11, true, trn);
                        model.GRNNo = internalId;
                        //model.GrandTotal = model.Items.Sum(m => ((m.AcceptedQuantity * m.Rate) - m.Discount)) + model.Addition - model.Deduction;
                        model.GrandTotal = model.Items.Sum(m => ((m.AcceptedQuantity * m.Rate) - m.Discount));
                        model.Addition = model.Additions
                            .Where(x => (x.AdditionId != null || x.AdditionId != 0) && x.Addition > 0)
                            .Sum(x => x.Addition);
                        model.Deduction = model.Deductions
                            .Where(x => (x.DeductionId != null || x.DeductionId != 0) && x.Deduction > 0)
                            .Sum(x => x.Deduction);
                        int id = 0;

                        string sql = @"INSERT INTO GRN(GRNNo,GRNDate,SupplierId,CurrencyId,WareHouseId,SupplierDCNoAndDate,SpecialRemarks,
                                                       CreatedBy,CreatedDate,OrganizationId,isDirectPurchaseGRN,
                                                       GrandTotal, VehicleNo, GatePassNo, ReceivedBy, Addition, Deduction) 
                                               VALUES (@GRNNo,@GRNDate,@SupplierId,@CurrencyId,@StockPointId,@SupplierDCNoAndDate,@SpecialRemarks,
                                                       @CreatedBy,@CreatedDate,@OrganizationId,@isDirectPurchaseGRN, 
                                                       @GrandTotal, @VehicleNo, @GatePassNo, @ReceivedBy, @Addition, @Deduction);
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

                        foreach (var Addition in model.Additions)
                        {
                            if ((Addition.AdditionId == null || Addition.AdditionId == 0) && Addition.Addition <= 0) continue;
                            new GRNItemRepository().InsertGRNAddition(new GRNAddition
                            {
                                GRNId = id,
                                AdditionId = Addition.AdditionId,
                                Addition = Addition.Addition
                            }, connection, trn);

                        }

                        foreach (var Deduction in model.Deductions)
                        {
                            if ((Deduction.DeductionId == null || Deduction.DeductionId == 0) && Deduction.Deduction <= 0) continue;
                            new GRNItemRepository().InsertGRNDeduction(new GRNDeduction
                            {
                                GRNId = id,
                                DeductionId = Deduction.DeductionId,
                                Deduction = Deduction.Deduction
                            }, connection, trn);

                        }

                        InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "GRN", id.ToString(), "0");
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

        public int InsertGRNDT(GRN model, IDbConnection connection, IDbTransaction txn)
        {

            model.Items = model.Items.Where(m => m.AcceptedQuantity > 0).ToList<GRNItem>();
            if (model.Items != null && model.Items.Count > 0)
            {
                try
                {
                    foreach (var item in model.Items)
                    {
                        item.GRNId = model.GRNId;
                        item.Amount = item.AcceptedQuantity * item.Rate;
                        new GRNItemRepository().InsertGRNItem(item, connection, txn);
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
                                StocktrnId = model.GRNId,
                                StockUserId = model.GRNNo,
                                stocktrnDate = model.GRNDate
                            }, connection, txn);
                    }


                    foreach (var Addition in model.Additions)
                    {
                        if ((Addition.AdditionId == null || Addition.AdditionId == 0) && Addition.Addition <= 0) continue;
                        new GRNItemRepository().InsertGRNAddition(new GRNAddition
                        {
                            GRNId = model.GRNId,
                            AdditionId = Addition.AdditionId,
                            Addition = Addition.Addition
                        }, connection, txn);

                    }

                    foreach (var Deduction in model.Deductions)
                    {
                        if ((Deduction.DeductionId == null || Deduction.DeductionId == 0) && Deduction.Deduction <= 0) continue;
                        new GRNItemRepository().InsertGRNDeduction(new GRNDeduction
                        {
                            GRNId = model.GRNId,
                            DeductionId = Deduction.DeductionId,
                            Deduction = Deduction.Deduction
                        }, connection, txn);

                    }
                    return 1;
                }
                catch (Exception ex)
                {
                    throw ex;
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

        public IEnumerable<GRN> GetGRNPreviousList(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @" SELECT Distinct
                                G.GRNId,GRNNo+' - '+CONVERT(VARCHAR, GRNDate, 106) GRNNo,
                                STUFF((SELECT Distinct ', ' + CAST(SO.SupplyOrderNo AS VARCHAR(MAX)) [text()]
                                FROM GRNItem GT1 
							    INNER  JOIN SupplyOrderItem SOT ON SOT.SupplyOrderItemId =GT1.SupplyOrderItemId
							    INNER  JOIN SupplyOrder SO on SO.SupplyOrderId=SOT.SupplyOrderId
                                WHERE GT1.GRNId = GT.GRNId
                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') SONoDATE,

                                ISNULL(S.SupplierName, '-') Supplier,
                                ISNULL(SupplierDCNoAndDate, '-') SupplierDCNoAndDate,
                                EMP.EmployeeName ReceivedByName,
                                ST.StockPointName,
                                ISNULL(G.GrandTotal, 0.00) + MAX(ISNULL(G.Addition, 0)) - MAX(ISNULL(G.Deduction, 0)) GrandTotal,
                                G.isDirectPurchaseGRN,
								G.GRNDate, G.CreatedDate
                                FROM GRN G 
                                INNER JOIN GRNItem GT ON GT.GRNId=G.GRNId
                                INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                                INNER JOIN Employee EMP ON G.ReceivedBy = EMP.EmployeeId
                                INNER JOIN Stockpoint ST ON G.WareHouseId = ST.StockPointId

                                WHERE ISNULL(G.isActive, 1) = 1 AND G.OrganizationId = @OrganizationId
                                GROUP BY G.GRNId,G.GRNNo,G.GRNDate,S.SupplierName,G.SupplierDCNoAndDate,
                                EMP.EmployeeName ,ST.StockPointName,G.GrandTotal,G.CreatedDate,
                                G.isDirectPurchaseGRN,GT.SupplyOrderItemId,GT.GRNId
								ORDER BY G.GRNDate DESC, G.CreatedDate DESC;";
                return connection.Query<GRN>(query, new { OrganizationId = OrganizationId });
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

        public GRN UpdateGRN(GRN model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.GrandTotal = model.Items.Sum(m => ((m.AcceptedQuantity * m.Rate) - m.Discount));
                    model.Addition = model.Additions
                        .Where(x => (x.AdditionId != null || x.AdditionId != 0) && x.Addition > 0)
                        .Sum(x => x.Addition);
                    model.Deduction = model.Deductions
                        .Where(x => (x.DeductionId != null || x.DeductionId != 0) && x.Deduction > 0)
                        .Sum(x => x.Deduction);

                    string sql = @"UPDATE GRN SET /*GRNNo = @GRNNo,*/GRNDate =@GRNDate ,SupplierId = @SupplierId,
                              WareHouseId = @StockPointId,SupplierDCNoAndDate = @SupplierDCNoAndDate,CurrencyId=@CurrencyId,SpecialRemarks=@SpecialRemarks,
                              CreatedBy=@CreatedBy,CreatedDate=@CreatedDate, GrandTotal=@GrandTotal, VehicleNo=@VehicleNo, GatePassNo=@GatePassNo, 
                              ReceivedBy=@ReceivedBy, Addition=@Addition, Deduction=@Deduction WHERE GRNId = @GRNId";
                    var id = connection.Execute(sql, model, txn);

                    int output = new GRNItemRepository().DeleteGRNADDDED(model.GRNId, connection, txn);

                    output = new GRNItemRepository().DeleteGRNItem(model.GRNId, connection, txn);

                    output = new StockUpdateRepository().DeleteGRNStockUpdate(model.GRNId, connection, txn);

                    output = InsertGRNDT(model, connection, txn);

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Update", "GRN", id.ToString(), "0");

                    txn.Commit();
                    return model;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
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
	                                    DATEDIFF(day, GETDATE(), RequiredDate) DaysLeft,
	                                    ISNULL(SpecialRemarks, '-') SpecialRemarks,
	                                    ISNULL(CONVERT(VARCHAR(15),RequiredDate,106), '-') RequiredDate,
										S.SupplierId,
										S.SupplierName,
										SO.RequiredDate
                                    FROM SupplyOrder SO 
	                                    INNER JOIN SupplyOrderItem SOI ON SO.SupplyOrderId = SOI.SupplyOrderId
	                                    INNER JOIN Supplier S ON S.SupplierId=SO.SupplierId AND SO.SupplierId = ISNULL(NULLIF(@supplierId, 0), SO.SupplierId)
	                                    LEFT JOIN GRNItem GI ON SOI.SupplyOrderItemId = GI.SupplyOrderItemId
                                    WHERE SO.isActive=1 and 
                                    (GI.SupplyOrderItemId IS NULL OR ISNULL(GI.Quantity, 0) < ISNULL(SOI.OrderedQty, 0))
                                    ORDER BY SO.RequiredDate, SO.SupplyOrderDate DESC";

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
                    return connection.Query<GRN>(@"SELECT Supplierid, SupplierName Supplier, S.CurrencyId CurrencyId, C.CurrencyName FROM Supplier S
                                                    LEFT JOIN Currency C ON S.CUrrencyId = C.CurrencyId WHERE SupplierId = @supplierId",
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
									((ISNULL(SOI.OrderedQty, 0) - ISNULL(GRN.Quantity, 0))*ISNULL(SOI.Rate, 0.00))-ISNULL(SOI.Discount, 0.00) Amount
	                                --ISNULL(SOI.Amount, 0.00) Amount
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


        public GRN GetGRNDetails(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"DECLARE @isUsed BIT = 0;
                                IF EXISTS(SELECT GRNItemId FROM GRNItem WHERE GRNId = @GRNId AND GRNItemId IN (SELECT GRNItemId FROM PurchaseBillItem))
	                                SET @isUsed = 1;
                                ELSE IF EXISTS(SELECT GRNItemId FROM GRNItem WHERE GRNId = @GRNId AND GRNItemId IN (SELECT GRNItemId FROM ItemBatch))
	                                SET @isUsed = 1;

                                SELECT GRNId,GRNNo,GRNDate,S.SupplierName Supplier,''SONODATE,VehicleNo,GatePassNo,
                                     GrandTotal,ReceivedBy,
                                     SpecialRemarks,WareHouseId StockPointId,StockPointName,SupplierDCNoAndDate,
                                     S.SupplierId SupplierId,G.CurrencyId CurrencyId,C.CurrencyName,
				                     @isUsed isUsed
                                     FROM GRN G
                                     INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                                     INNER JOIN Stockpoint SP On SP.StockPointId=G.WareHouseId
                                     INNER JOIN Currency C On C.CurrencyId=G.CurrencyId
                                     WHERE G.GRNId = @GRNId";

                GRN workshoprequest = connection.Query<GRN>(qry, new { GRNId = GRNId }).FirstOrDefault();
                return workshoprequest;
            }
        }


        public List<GRNItem> GetGRNItems(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "CREATE TABLE #TEMP(GRNId INT,SupplyOrderItemId INT,ItemId INT,";
                query += " ItemName VARCHAR(500),PartNo  VARCHAR(500),";
                query += " BALANCEQTY INT,AcceptedQuantity  INT,RejectedQuantity INT,";
                query += " Unit VARCHAR(500),Rate DECIMAL(18,3),Discount DECIMAL(18,3),";
                query += " Amount DECIMAL(18,3),Remarks VARCHAR(5000),GRNQTY INT,PendingQuantity INT,ReceivedQuantity INT)";

                query += " INSERT INTO #TEMP (GRNId,SupplyOrderItemId, ItemId,ItemName ,PartNo,";
                query += " BALANCEQTY,AcceptedQuantity,RejectedQuantity,";
                query += " Unit,Rate,Discount,Amount,Remarks,GRNQTY,PendingQuantity,ReceivedQuantity)";

                query += " SELECT GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                query += " sum(isnull(S.OrderedQty,0))-isnull((select sum(ISNULL(A.Quantity,0)) ";
                query += " FROM GRNItem A where S.SupplyOrderItemId=A.SupplyOrderItemId),0) BALANCEQTY,";
                query += " sum(Quantity) AcceptedQuantity,(isnull(G.ReceivedQty,0)-isnull(Quantity,0)) RejectedQuantity,";
                query += " U.UnitName Unit,G.Rate,G.Discount,G.Amount,Remarks,0 GRNQTY,0 PendingQuantity,sum(G.ReceivedQty)";
                query += " FROM GRNItem G";
                query += " INNER JOIN SupplyOrderItem S ON S.SupplyOrderItemId=G.SupplyOrderItemId";
                query += " INNER JOIN Item I ON I.ItemId=G.ItemId";
                query += " INNER JOIN Unit U ON U.UnitId=I.ItemUnitId";
                query += " WHERE G.GRNId = " + GRNId.ToString();
                query += " GROUP BY GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                query += " G.ReceivedQty,G.Quantity,U.UnitName,G.Rate,G.Discount,G.Amount,Remarks,S.SupplyOrderItemId";
                query += " UPDATE #TEMP SET GRNQTY=(SELECT (SUM(G1.Quantity)) ";
                query += " FROM GRNItem G1 where #TEMP.SupplyOrderItemId=G1.SupplyOrderItemId and G1.GRNId=" + GRNId.ToString();
                query += " GROUP BY G1.SupplyOrderItemId)";

                query += " UPDATE #TEMP SET PendingQuantity=BALANCEQTY+GRNQTY";

                query += " SELECT * FROM #TEMP";
                //string query = " SELECT GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                //      query += " (sum(S.OrderedQty-G.Quantity )+G.Quantity)PendingQuantity,";
                //      query += " sum(G.ReceivedQty) ReceivedQuantity,sum(Quantity) AcceptedQuantity,";
                //      query += " (isnull(G.ReceivedQty,0)-isnull(Quantity,0)) RejectedQuantity,";
                //      query += " U.UnitName Unit,G.Rate,G.Discount,G.Amount,Remarks";
                //      query += " FROM GRNItem G";
                //      query += " INNER JOIN SupplyOrderItem S ON S.SupplyOrderItemId=G.SupplyOrderItemId";
                //      query += " INNER JOIN Item I ON I.ItemId=G.ItemId";
                //      query += " INNER JOIN Unit U ON U.UnitId=I.ItemUnitId";
                //      query += " WHERE G.GRNId = " + GRNId.ToString();
                //      query += " GROUP BY GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                //      query += " G.ReceivedQty,G.Quantity,U.UnitName,G.Rate,G.Discount,G.Amount,Remarks";

                return connection.Query<GRNItem>(query).ToList();


            }
        }

        public List<GRNAddition> GetGRNAdditions(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = " SELECT GRNId,A.AddDedId AdditionId,AddDedAmt Addition FROM GRNAddDed G";
                query += " INNER JOIN AdditionDeduction A ON A.AddDedId=G.AddDedId";
                query += " WHERE AddDedType=1 AND GRNId= " + GRNId.ToString();
                return connection.Query<GRNAddition>(query).ToList();
            }
        }

        public List<GRNDeduction> GetGRNDeductions(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = " SELECT GRNId,D.AddDedId DeductionId,AddDedAmt Deduction FROM GRNAddDed G";
                query += " INNER JOIN AdditionDeduction D ON D.AddDedId=G.AddDedId";
                query += " WHERE AddDedType=2 AND GRNId= " + GRNId.ToString();
                return connection.Query<GRNDeduction>(query).ToList();
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


        public int CHECK(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT count(G.GRNId)count FROM GRNItem G 
                                INNER JOIN PurchaseBillItem P ON P.GRNItemId=G.GRNItemId
                                WHERE GRNId =@GRNId";

                var id = connection.Query<int>(sql, new { GRNId = GRNId }).FirstOrDefault();

                return id;

            }

        }

        public int DeleteGRNHD(int Id)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete FROM GRN  WHERE GRNId=@Id";

                {
                    var id = connection.Execute(sql, new { Id = Id });
                    return id;
                }

            }
        }

        public int DeleteGRN(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                int output = new GRNItemRepository().DeleteGRNADDDED(GRNId, connection, txn);

                output = new GRNItemRepository().DeleteGRNItem(GRNId, connection, txn);

                output = new StockUpdateRepository().DeleteGRNStockUpdate(GRNId, connection, txn);

                string sql = @"Delete FROM GRN  WHERE GRNId=@GRNId";
                output = connection.Execute(sql, new { GRNId = GRNId });
                return output;
            }

        }

        public GRN GetGRNDetailsHDPrint(int GRNId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @" DECLARE @isUsed BIT = 0;
                                IF EXISTS(SELECT GRNItemId FROM GRNItem WHERE GRNId = 8120 AND GRNItemId IN (SELECT GRNItemId FROM PurchaseBillItem))
	                                SET @isUsed = 1;
                                ELSE IF EXISTS(SELECT GRNItemId FROM GRNItem WHERE GRNId = 8120 AND GRNItemId IN (SELECT GRNItemId FROM ItemBatch))
	                                SET @isUsed = 1;

                                SELECT O.*,Cu.CurrencyName OrgCurrency,ORR.CountryName OrgCountryName,GRNNo,GRNDate,S.SupplierName Supplier,''SONODATE,VehicleNo,GatePassNo,
                                     GrandTotal,EmployeeName EmpReceivedBy,
                                     SpecialRemarks,WareHouseId StockPointId,StockPointName,SupplierDCNoAndDate,
                                     S.SupplierId SupplierId,G.CurrencyId CurrencyId,C.CurrencyName,
				                     @isUsed isUsed
                                     FROM GRN G
                                     INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                                     INNER JOIN Stockpoint SP On SP.StockPointId=G.WareHouseId
                                     INNER JOIN Currency C On C.CurrencyId=G.CurrencyId
                                    INNER JOIN Organization O ON O.OrganizationId=S.OrganizationId
		                            inner join employee on employeeid=ReceivedBy
                                    left  JOIN Country ORR ON ORR.CountryId=O.Country
							        Left Join Currency CU on CU.CurrencyId=O.CurrencyId
                                WHERE G.GRNId=" + GRNId.ToString();

                GRN workshoprequest = connection.Query<GRN>(qry, new { OrganizationId = OrganizationId }).FirstOrDefault();
                return workshoprequest;
            }
        }


        public List<GRNItem> GetGRNItemsPrintDT(int GRNId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "CREATE TABLE #TEMP(GRNId INT,SupplyOrderItemId INT,ItemId INT,";
                query += " ItemName VARCHAR(500),PartNo  VARCHAR(500),";
                query += " BALANCEQTY INT,AcceptedQuantity  INT,RejectedQuantity INT,";
                query += " Unit VARCHAR(500),Rate DECIMAL(18,3),Discount DECIMAL(18,3),";
                query += " Amount DECIMAL(18,3),Remarks VARCHAR(5000),GRNQTY INT,PendingQuantity INT,ReceivedQuantity INT)";

                query += " INSERT INTO #TEMP (GRNId,SupplyOrderItemId, ItemId,ItemName ,PartNo,";
                query += " BALANCEQTY,AcceptedQuantity,RejectedQuantity,";
                query += " Unit,Rate,Discount,Amount,Remarks,GRNQTY,PendingQuantity,ReceivedQuantity)";

                query += " SELECT GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                query += " sum(isnull(S.OrderedQty,0))-isnull((select sum(ISNULL(A.Quantity,0)) ";
                query += " FROM GRNItem A where S.SupplyOrderItemId=A.SupplyOrderItemId),0) BALANCEQTY,";
                query += " sum(Quantity) AcceptedQuantity,(isnull(G.ReceivedQty,0)-isnull(Quantity,0)) RejectedQuantity,";
                query += " U.UnitName Unit,G.Rate,G.Discount,G.Amount,Remarks,0 GRNQTY,0 PendingQuantity,sum(G.ReceivedQty)";
                query += " FROM GRNItem G";
                query += " INNER JOIN SupplyOrderItem S ON S.SupplyOrderItemId=G.SupplyOrderItemId";
                query += " INNER JOIN Item I ON I.ItemId=G.ItemId";
                query += " INNER JOIN Unit U ON U.UnitId=I.ItemUnitId";
                query += " WHERE G.GRNId = " + GRNId.ToString();
                query += " GROUP BY GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                query += " G.ReceivedQty,G.Quantity,U.UnitName,G.Rate,G.Discount,G.Amount,Remarks,S.SupplyOrderItemId";
                query += " UPDATE #TEMP SET GRNQTY=(SELECT (SUM(G1.Quantity)) ";
                query += " FROM GRNItem G1 where #TEMP.SupplyOrderItemId=G1.SupplyOrderItemId and G1.GRNId=" + GRNId.ToString();
                query += " GROUP BY G1.SupplyOrderItemId)";

                query += " UPDATE #TEMP SET PendingQuantity=BALANCEQTY+GRNQTY";

                query += " SELECT * FROM #TEMP";
                //string query = " SELECT GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                //      query += " (sum(S.OrderedQty-G.Quantity )+G.Quantity)PendingQuantity,";
                //      query += " sum(G.ReceivedQty) ReceivedQuantity,sum(Quantity) AcceptedQuantity,";
                //      query += " (isnull(G.ReceivedQty,0)-isnull(Quantity,0)) RejectedQuantity,";
                //      query += " U.UnitName Unit,G.Rate,G.Discount,G.Amount,Remarks";
                //      query += " FROM GRNItem G";
                //      query += " INNER JOIN SupplyOrderItem S ON S.SupplyOrderItemId=G.SupplyOrderItemId";
                //      query += " INNER JOIN Item I ON I.ItemId=G.ItemId";
                //      query += " INNER JOIN Unit U ON U.UnitId=I.ItemUnitId";
                //      query += " WHERE G.GRNId = " + GRNId.ToString();
                //      query += " GROUP BY GRNId,G.SupplyOrderItemId,I.ItemId,I.ItemName,I.PartNo,";
                //      query += " G.ReceivedQty,G.Quantity,U.UnitName,G.Rate,G.Discount,G.Amount,Remarks";

                return connection.Query<GRNItem>(query).ToList();


            }
        }
    }
}