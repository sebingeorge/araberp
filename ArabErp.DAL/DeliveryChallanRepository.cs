using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class DeliveryChallanRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string InsertDeliveryChallan(DeliveryChallan objDeliveryChallan)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    objDeliveryChallan.DeliveryChallanRefNo = DatabaseCommonRepository.GetNewDocNo(connection, objDeliveryChallan.OrganizationId, 18, true, txn);

                    string sql = @"insert into DeliveryChallan(JobCardId,DeliveryChallanRefNo,DeliveryChallanDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId,isActive, TransportWarrantyExpiryDate) 
                                   Values (@JobCardId,@DeliveryChallanRefNo,@DeliveryChallanDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId,1, @TransportWarrantyExpiryDate);
                                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = connection.Query<int>(sql, objDeliveryChallan, txn).Single();

                    try
                    {
                        foreach (var item in objDeliveryChallan.ItemBatches)
                        {
                            item.DeliveryChallanId = id;
                            sql = @"UPDATE ItemBatch SET DeliveryChallanId = @DeliveryChallanId, WarrantyStartDate = @WarrantyStartDate, WarrantyExpireDate = @WarrantyExpireDate
                                WHERE ItemBatchId = @ItemBatchId";
                            connection.Execute(sql, item, txn);
                        }
                    }
                    catch (NullReferenceException) { }
                    InsertLoginHistory(dataConnection, objDeliveryChallan.CreatedBy, "Create", "Delivery Challan", id.ToString(), "0");
                    txn.Commit();
                    return objDeliveryChallan.DeliveryChallanRefNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "";
                }
            }
        }


        public DeliveryChallan GetDeliveryChallan(int DeliveryChallanId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from DeliveryChallan
                        where DeliveryChallanId=@DeliveryChallanId";

                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
                {
                    DeliveryChallanId = DeliveryChallanId
                }).First<DeliveryChallan>();

                return objDeliveryChallan;
            }
        }

        public List<DeliveryChallan> GetDeliveryChallans(int OrganizationId, string challan = "", string saleorder = "", string customer = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT        
	                                DeliveryChallan.DeliveryChallanId, 
	                                DeliveryChallan.DeliveryChallanRefNo, 
	                                SaleOrder.SaleOrderRefNo, 
	                                SaleOrder.SaleOrderDate, 
	                                Customer.CustomerName Customer, 
	                                DeliveryChallan.DeliveryChallanDate,
									TransportWarrantyExpiryDate
                                FROM DeliveryChallan INNER JOIN
                                    JobCard ON DeliveryChallan.JobCardId = JobCard.JobCardId INNER JOIN
                                    SaleOrder ON JobCard.SaleOrderId = SaleOrder.SaleOrderId INNER JOIN
                                    Customer ON SaleOrder.CustomerId = Customer.CustomerId
                                WHERE DeliveryChallan.OrganizationId = @OrganizationId";

                var objDeliveryChallans = connection.Query<DeliveryChallan>(sql, new { OrganizationId = OrganizationId }).ToList<DeliveryChallan>();

                return objDeliveryChallans;
            }
        }

        public string DeleteDeliveryChallanItemBatch(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {

                string query = @"Delete ItemBatch  OUTPUT DELETED.DeliveryChallanId WHERE DeliveryChallanId=@DeliveryChallanId";

                string output = connection.Query<string>(query, new { DeliveryChallanId = DeliveryChallanId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Return all completed jobcards that are not in vehicle out pass
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<PendingJC> PendingDeliveryChallan(int customerId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingJC>(@"SELECT ISNULL(SO.SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderNoDate, VM.VehicleModelName+' - '+VM.VehicleModelDescription VehicleModel, WD.WorkDescr, CUS.CustomerName, SOI.SaleOrderItemId,SOI.IsPaymentApprovedForDelivery INTO #TEMP FROM SaleOrderItem SOI
                    INNER JOIN SaleOrder SO ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                    INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                    WHERE CUS.CustomerId = @customerId AND ISNULL(SOI.isActive, 1) = 1 AND ISNULL(VM.isActive, 1) = 1 and SO.OrganizationId = @OrganizationId AND SO.isProjectBased = 0;

                    SELECT J.JobCardId, ISNULL(J.JobCardNo, '')+' - '+CONVERT(VARCHAR, J.JobCardDate, 106) JobCardNoDate, T.SaleOrderNoDate, T.VehicleModel, T.WorkDescr, T.CustomerName, ISNULL(VI.RegistrationNo, '-') RegistrationNo,T.IsPaymentApprovedForDelivery FROM JobCard J 
                    LEFT JOIN DeliveryChallan VO ON J.JobCardId = VO.JobCardId
                    INNER JOIN #TEMP T ON J.SaleOrderItemId = T.SaleOrderItemId
                    LEFT JOIN VehicleInPass VI ON T.SaleOrderItemId = VI.SaleOrderItemId
                    WHERE ISNULL(J.JodCardCompleteStatus, 0) = 1 AND VO.JobCardId IS NULL ;

                    DROP TABLE #TEMP;", new { customerId = customerId, OrganizationId = OrganizationId }).ToList();
            }
        }
        /// <summary>
        /// Get Job Card details such as Customer, Sale Order No. & Date, Job Card No. & Date, Registration No., Vehicle Model & Description, Work Description.
        /// </summary>
        /// <param name="jobCardId"></param>
        /// <returns></returns>
        public PendingJC GetJobCardDetails(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    return connection.Query<PendingJC>(@"SELECT ISNULL(SO.SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderNoDate, 
                    VM.VehicleModelName+' - '+VM.VehicleModelDescription VehicleModel, WD.WorkDescr, CUS.CustomerName, SOI.SaleOrderItemId,SO.CustomerOrderRef,
					SO.PaymentTerms
					INTO #TEMP 
                    FROM SaleOrderItem SOI
                    INNER JOIN SaleOrder SO ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                    INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                    WHERE ISNULL(SOI.isActive, 1) = 1 AND ISNULL(VM.isActive, 1) = 1;

                    SELECT ISNULL(J.JobCardNo, '')+' - '+CONVERT(VARCHAR, J.JobCardDate, 106) JobCardNoDate, T.SaleOrderNoDate,T.CustomerOrderRef,
                    T.VehicleModel, T.WorkDescr, T.CustomerName, ISNULL(VI.RegistrationNo, '-') RegistrationNo, T.PaymentTerms
                    FROM JobCard J 
                    LEFT JOIN DeliveryChallan VO ON J.JobCardId = VO.JobCardId
                    INNER JOIN #TEMP T ON J.SaleOrderItemId = T.SaleOrderItemId
                    LEFT JOIN VehicleInPass VI ON T.SaleOrderItemId = VI.SaleOrderItemId
                    WHERE J.JobCardId = @jobCardId AND ISNULL(J.JodCardCompleteStatus, 0) = 1 AND VO.JobCardId IS NULL;

                    DROP TABLE #TEMP;", new { jobCardId = jobCardId }).Single();
                }
                catch (InvalidOperationException)
                {
                    return new PendingJC();
                }
            }
        }

        public IEnumerable<ItemBatch> GetSerialNos(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                IB.ItemBatchId,
	                                I.ItemName,
	                                SerialNo
                                FROM JobCard JC
                                INNER JOIN ItemBatch IB ON JC.SaleOrderItemId = IB.SaleOrderItemId
                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                                INNER JOIN Item I ON (GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId)
                                WHERE JC.JobCardId = @id";


                return connection.Query<ItemBatch>(query, new { id = id }).ToList();
            }
        }

        public IEnumerable<DeliveryChallan> GetAllDeliveryChallan(int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"select DeliveryChallanId,DeliveryChallanRefNo,DeliveryChallanDate,JobCardNo,JobCardDate,E.EmployeeName from DeliveryChallan D
                               INNER JOIN Employee E ON E.EmployeeId=D.EmployeeId
                               INNER JOIN JobCard J ON J.JobCardId =D.JobCardId where D.isActive=1 AND D.OrganizationId = @OrganizationId and   D.DeliveryChallanDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) AND  D.DeliveryChallanId = ISNULL(NULLIF(@id, 0), D.DeliveryChallanId) 
                               ORDER BY D.DeliveryChallanDate DESC, D.CreatedDate DESC";
                return connection.Query<DeliveryChallan>(qry, new { OrganizationId = OrganizationId, id = id, from = from, to = to }).ToList();

            }
        }

        public DeliveryChallan ViewDeliveryChallanHD(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT DISTINCT DC.DeliveryChallanId,DeliveryChallanRefNo,DeliveryChallanDate,C.CustomerName Customer,SO.CustomerOrderRef,
                                ISNULL(SO.SaleOrderRefNo,'')+ ' - '  +CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
                                ISNULL(JC.JobCardNo,'') + ' - ' +CONVERT(varchar,JC.JobCardDate,106)JobCardNo,VI. RegistrationNo,
                                WI.WorkDescr,VM.VehicleModelName VehicleModel,E.EmployeeId,SO.PaymentTerms,DC.Remarks,ISNULL(SQ.DeliveryChallanId,0)IsUsed, TransportWarrantyExpiryDate
                                FROM DeliveryChallan DC
                                INNER JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                                INNER JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
                                INNER JOIN SaleOrderItem SOI ON SOI.SaleOrderId=SOI.SaleOrderId AND JC.SaleOrderItemId=SOI.SaleOrderItemId
                                INNER JOIN Customer C ON C.CustomerId=SO.CustomerId 
                                INNER JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                                INNER JOIN Employee E ON E.EmployeeId=DC.EmployeeId
                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
                                LEFT JOIN SalesQuotation SQ ON SQ.DeliveryChallanId=DC.DeliveryChallanId
                                WHERE  DC.DeliveryChallanId=@DeliveryChallanId";

                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
                {
                    DeliveryChallanId = DeliveryChallanId
                }).First<DeliveryChallan>();

                return objDeliveryChallan;
            }
        }

        public DeliveryChallan GetDeliveryChallanHD(int DeliveryChallanId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"
								
								SELECT DISTINCT O.*,
								ORR.CountryName,
								DC.DeliveryChallanId,
								DeliveryChallanRefNo,
								DeliveryChallanDate,
								C.CustomerName Customer,
								C.DoorNo CDoorNo,
								C.Street CStreet,
								CCC.CountryName CCountry,
								ISNULL(SO.SaleOrderRefNo,'')SaleRefNo,CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
								ISNULL(JC.JobCardNo,'')JobcardNo,CONVERT(varchar,JC.JobCardDate,106)JobCardDate,
								VI. RegistrationNo,VI.ChassisNo,
								WI.WorkDescr,
								VM.VehicleModelName VehicleModel,
								E.EmployeeName,
								SO.PaymentTerms,
								DC.Remarks,
								SQ.QuotationRefNo,
								I.[ItemName] FreezerName,I.ItemId FreezerId,
								ISNULL(I.PartNo,'') FreezerPartNo,
								II.ItemName BoxName, II.ItemId BoxId,
								ISNULL(II.PartNo, '') BoxPartNo,
								LPO.SupplyOrderNo,
								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig,
								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate
	                            FROM DeliveryChallan DC
                                INNER JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                                INNER JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
								LEFT JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
                                INNER JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
                                INNER JOIN Customer C ON C.CustomerId=SO.CustomerId 
								left join Country CCC ON CCC.CountryId=c.Country
                                INNER JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                                INNER JOIN Employee E ON E.EmployeeId=DC.EmployeeId
                                inner join Organization O ON  DC.OrganizationId=O.OrganizationId
								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
							    inner join Item I ON I.ItemId=JC.[FreezerUnitId]
								inner join Item II ON II.ItemId=JC.[BoxId]
								left join [User] U ON U.UserId=DC.CreatedBy
								WHERE DC.DeliveryChallanId=@DeliveryChallanId";

                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
                {
                    DeliveryChallanId = DeliveryChallanId,
                    OrganizationId = OrganizationId,
                   
                    
                }).First<DeliveryChallan>();

                return objDeliveryChallan;
            }
        }

        public List<ItemBatch> GetDeliveryChallanDT(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT DISTINCT ItemBatchId,SerialNo,ItemName, DATEDIFF(MONTH,WarrantyStartDate,WarrantyExpireDate) AS WarrantyPeriodInMonths
                                     FROM ItemBatch IB 
                                     LEFT JOIN GRNItem GI ON GI.GRNItemId=IB.GRNItemId
                LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                                     INNER JOIN Item I ON (I.ItemId=GI.ItemId OR OS.ItemId = I.ItemId)
                                     WHERE DeliveryChallanId = @DeliveryChallanId";
               
                return connection.Query<ItemBatch>(sql, new { DeliveryChallanId = DeliveryChallanId }).ToList();
            }
        }

        public int UpdateDeliveryChallan(DeliveryChallan objDeliveryChallan)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                string sql = @"UPDATE
                                DeliveryChallan SET DeliveryChallanRefNo=@DeliveryChallanRefNo,
                                DeliveryChallanDate=@DeliveryChallanDate,EmployeeId=@EmployeeId,Remarks=@Remarks,
                                CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId, TransportWarrantyExpiryDate = @TransportWarrantyExpiryDate
                                WHERE DeliveryChallanId = @DeliveryChallanId;";
                    var id = connection.Execute(sql, objDeliveryChallan, txn);

                    sql = @"UPDATE ItemBatch SET DeliveryChallanId = NULL WHERE DeliveryChallanId = @DeliveryChallanId";
                    id = connection.Execute(sql, objDeliveryChallan, txn);

                    try
                    {
                        if (objDeliveryChallan.ItemBatches != null && objDeliveryChallan.ItemBatches.Count > 0)
                        {
                            foreach (var item in objDeliveryChallan.ItemBatches)
                            {
                                item.DeliveryChallanId = objDeliveryChallan.DeliveryChallanId;
                                item.WarrantyStartDate = objDeliveryChallan.DeliveryChallanDate;
                                item.WarrantyExpireDate = objDeliveryChallan.DeliveryChallanDate.AddMonths(item.WarrantyPeriodInMonths ?? 0).AddDays(-1);

                                sql = @"UPDATE ItemBatch SET DeliveryChallanId = @DeliveryChallanId, WarrantyStartDate = @WarrantyStartDate, WarrantyExpireDate = @WarrantyExpireDate
                                WHERE ItemBatchId = @ItemBatchId";
                                connection.Execute(sql, item, txn);
                            }
                        }
                    }
                    catch (NullReferenceException) { }

                    InsertLoginHistory(dataConnection, objDeliveryChallan.CreatedBy, "Update", "Delivery Challan", objDeliveryChallan.DeliveryChallanId.ToString(), objDeliveryChallan.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public string DeleteDeliveryChallan(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM ItemBatch WHERE DeliveryChallanId = @DeliveryChallanId;
                                     DELETE FROM DeliveryChallan OUTPUT deleted.DeliveryChallanRefNo WHERE DeliveryChallanId = @DeliveryChallanId;";
                    string output = connection.Query<string>(query, new { DeliveryChallanId = DeliveryChallanId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }
        public List<ItemVsBom> GetDeliveryChallanDTPrint(int FreezerId,int BoxId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select I.itemname ItemName, 'Nos' UnitName, 1 Quantity from item I where itemid=@BoxId
						union All 
						select I.itemname, 'Nos', 1 from Item I where ItemId=@FreezerId
						union All				
						select I.[ItemName],U.UnitName ,IB.Quantity from [dbo].[ItemVsBom] IB
						inner join Item I ON I.ItemId=IB.BomItemId 
						inner join unit U ON U.UnitId=I.ItemUnitId
						where IB.itemId=@FreezerId
						UNION ALL
						select I.[ItemName],U.UnitName,IB.Quantity from [dbo].[ItemVsBom] IB
						inner join Item I ON I.ItemId=IB.BomItemId 
						inner join unit U ON U.UnitId=I.ItemUnitId
						where IB.itemId=@BoxId";

                return connection.Query<ItemVsBom>(sql, new { FreezerId = FreezerId, BoxId = BoxId }).ToList();
            }
        }

    }
}
