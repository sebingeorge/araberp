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

                    string sql = @"insert into DeliveryChallan(JobCardId,DeliveryChallanRefNo,DeliveryChallanDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId,isActive, TransportWarrantyExpiryDate, QuotationRefNo) 
                                   Values (@JobCardId,@DeliveryChallanRefNo,@DeliveryChallanDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId,1, @TransportWarrantyExpiryDate, @QuotationRefNo);
                                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = connection.Query<int>(sql, objDeliveryChallan, txn).Single();

                    #region update customer order ref to [SaleOrder]
                    //if (objDeliveryChallan.isService == 1)
                    //{
                        sql = @"UPDATE SaleOrder SET CustomerOrderRef = '" + objDeliveryChallan.CustomerOrderRef + @"'
                                WHERE SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = " + objDeliveryChallan.JobCardId + @")";
                        if (connection.Execute(sql, transaction: txn) <= 0) throw new Exception();
                    //} 
                    #endregion

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
                    #region inserting print description
                    try
                    {
                        sql = @"INSERT INTO PrintDescription (DeliveryChallanId, Description, UoM, Quantity, CreatedBy, CreatedDate, OrganizationId)
                                VALUES (@DeliveryChallanId, @Description, @UoM, @Quantity, @CreatedBy, GETDATE(), @OrganizationId)";
                        foreach (var item in objDeliveryChallan.PrintDescriptions)
                        {
                            if (item.Description == null) continue;
                            item.DeliveryChallanId = id;
                            item.CreatedBy = int.Parse(objDeliveryChallan.CreatedBy);
                            item.OrganizationId = objDeliveryChallan.OrganizationId;
                            if (connection.Execute(sql, item, txn) <= 0) throw new Exception();
                        }
                    }
                    catch (Exception) { throw new Exception(); }
                    #endregion

                    #region update Registration No. to [Vehicle Inpass]
                    if (objDeliveryChallan.isService == 0)
                    {
                        sql = @"UPDATE VehicleInpass SET RegistrationNo = '" + objDeliveryChallan.RegistrationNo + @"'
                                WHERE VehicleInPassId = (SELECT InPassId FROM JobCard WHERE JobCardId = " + objDeliveryChallan.JobCardId + @")";
                    if (connection.Execute(sql, transaction: txn) <= 0) throw new Exception();
                    } 
                    #endregion
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
                    WHERE CUS.CustomerId = @customerId AND ISNULL(SOI.isActive, 1) = 1 /* AND ISNULL(VM.isActive, 1) = 1*/ and SO.OrganizationId = @OrganizationId AND SO.isProjectBased = 0;

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
                    VM.VehicleModelName VehicleModel, WD.WorkDescr, CUS.CustomerName, SOI.SaleOrderItemId,SO.CustomerOrderRef,
					SO.PaymentTerms
					INTO #TEMP 
                    FROM SaleOrderItem SOI
                    INNER JOIN SaleOrder SO ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                    INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                    WHERE ISNULL(SOI.isActive, 1) = 1 /* AND ISNULL(VM.isActive, 1) = 1 */;

                    SELECT ISNULL(J.JobCardNo, '')+' - '+CONVERT(VARCHAR, J.JobCardDate, 106) JobCardNoDate, T.SaleOrderNoDate,T.CustomerOrderRef,
                    T.VehicleModel, T.WorkDescr, T.CustomerName, VI.RegistrationNo, VI.ChassisNo, T.PaymentTerms
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

        public IEnumerable<DeliveryChallan> GetAllDeliveryChallan(int id, int cusid, int OrganizationId, DateTime? from, DateTime? to, string RegNo = "", string Customer="")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"select DeliveryChallanId,DeliveryChallanRefNo,DeliveryChallanDate,JobCardNo,JobCardDate,E.EmployeeName,C.CustomerName,
                               V.RegistrationNo,v.ChassisNo from DeliveryChallan D
                               INNER JOIN Employee E ON E.EmployeeId=D.EmployeeId
                               INNER JOIN JobCard J ON J.JobCardId =D.JobCardId 
	                           inner join vehicleinpass V ON  V.VehicleInPassId=J.InPassId
							   LEFT JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
							   LEFT JOIN Customer C ON C.CustomerId=S.CustomerId
                               where D.isActive=1 AND D.OrganizationId = 1 and  
							    D.DeliveryChallanDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) 
                               AND ISNULL(@to, GETDATE()) AND
							     D.DeliveryChallanId = ISNULL(NULLIF(@id, 0), D.DeliveryChallanId) 
                               AND isnull(C.CustomerName,'') LIKE '%'+@Customer+'%'
                               AND (ISNULL(V.RegistrationNo, '') LIKE '%'+@RegNo+'%' OR ISNULL(V.ChassisNo, '') LIKE '%'+@RegNo+'%')
                               ORDER BY D.DeliveryChallanDate DESC, D.CreatedDate DESC";
                return connection.Query<DeliveryChallan>(qry, new { OrganizationId = OrganizationId, id = id, from = from, to = to, RegNo = RegNo, Customer = Customer }).ToList();

            }
        }

        public DeliveryChallan ViewDeliveryChallanHD(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT DISTINCT DC.DeliveryChallanId,DeliveryChallanRefNo,DeliveryChallanDate,C.CustomerName Customer,SO.CustomerOrderRef,
                                ISNULL(SO.SaleOrderRefNo,'')+ ' - '  +CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
                                ISNULL(JC.JobCardNo,'') + ' - ' +CONVERT(varchar,JC.JobCardDate,106)JobCardNo,VI.RegistrationNo, VI.ChassisNo,
                                WI.WorkDescr,VM.VehicleModelName VehicleModel,E.EmployeeId,SO.PaymentTerms,DC.Remarks,ISNULL(SQ.DeliveryChallanId,0)IsUsed, TransportWarrantyExpiryDate,
                                DC.PrintDescription, DC.QuotationRefNo, JC.isService, JC.JobCardId, QC.PunchingNo
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
                                LEFT JOIN JobCardQC QC ON JC.JobCardId = QC.JobCardId
                                WHERE  DC.DeliveryChallanId=@DeliveryChallanId";

                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
                {
                    DeliveryChallanId = DeliveryChallanId
                }).First<DeliveryChallan>();

                return objDeliveryChallan;
            }
        }

        //        public DeliveryChallan GetDeliveryChallanHD(int DeliveryChallanId, int OrganizationId,int isService)
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                if (isService==0)
        //                {
        //                    string sql = @"
        //								
        //								SELECT DISTINCT O.*,
        //								ORR.CountryName,
        //								DC.DeliveryChallanId,
        //								DeliveryChallanRefNo,
        //								DeliveryChallanDate,
        //								C.CustomerName Customer,
        //								C.DoorNo CDoorNo,
        //								C.Street CStreet,
        //								CCC.CountryName CCountry,
        //								ISNULL(SO.SaleOrderRefNo,'')SaleRefNo,CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
        //								ISNULL(JC.JobCardNo,'')JobcardNo,CONVERT(varchar,JC.JobCardDate,106)JobCardDate,
        //								VI. RegistrationNo,VI.ChassisNo,
        //								WI.WorkDescr,
        //								VM.VehicleModelName VehicleModel,
        //								E.EmployeeName,
        //								SO.PaymentTerms,
        //								DC.Remarks,
        //								SQ.QuotationRefNo,
        //								I.[ItemName] FreezerName,I.ItemId FreezerId,
        //								ISNULL(I.PartNo,'') FreezerPartNo,
        //								II.ItemName BoxName, II.ItemId BoxId,
        //								ISNULL(II.PartNo, '') BoxPartNo,
        //								LPO.SupplyOrderNo,
        //								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig,
        //								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate,
        //                                DC.PrintDescription printdes
        //	                            FROM DeliveryChallan DC
        //                                left JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
        //                                left JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
        //								left JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
        //                                left JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
        //                                left JOIN Customer C ON C.CustomerId=SO.CustomerId 
        //								left join Country CCC ON CCC.CountryId=c.Country
        //                                left JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
        //                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
        //                                left JOIN Employee E ON E.EmployeeId=DC.EmployeeId
        //                                left join Organization O ON  DC.OrganizationId=O.OrganizationId
        //								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
        //                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
        //								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
        //								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
        //								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
        //								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
        //								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
        //								left join [User] U ON U.UserId=DC.CreatedBy
        //							    left join Item I ON I.ItemId=JC.[FreezerUnitId]
        //								left join Item II ON II.ItemId=JC.[BoxId]
        //								WHERE DC.DeliveryChallanId=@DeliveryChallanId";


        //                    var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
        //                    {
        //                        DeliveryChallanId = DeliveryChallanId,
        //                        OrganizationId = OrganizationId,
        //                        isService=isService


        //                    }).First<DeliveryChallan>();

        //                    return objDeliveryChallan;
        //                }
        //                else
        //                {
        //                    string sql = @"  SELECT DISTINCT O.*,SO.SaleOrderId,
        //								ORR.CountryName,
        //								DC.DeliveryChallanId,
        //								DeliveryChallanRefNo,
        //								DeliveryChallanDate,
        //								C.CustomerName Customer,
        //								C.DoorNo CDoorNo,
        //								C.Street CStreet,
        //								CCC.CountryName CCountry,
        //								ISNULL(SO.SaleOrderRefNo,'')SaleRefNo,CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
        //								ISNULL(JC.JobCardNo,'')JobcardNo,CONVERT(varchar,JC.JobCardDate,106)JobCardDate,
        //								VI. RegistrationNo,VI.ChassisNo,
        //								WI.WorkDescr,
        //								VM.VehicleModelName VehicleModel,
        //								E.EmployeeName,
        //								SO.PaymentTerms,
        //								DC.Remarks,
        //								SQ.QuotationRefNo,
        //								LPO.SupplyOrderNo,
        //								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig,
        //								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate,
        //                                DC.PrintDescription printdes,
        //								JC.isService,SE.TailLiftModel FreezerName,SE.TailLiftSerialNo FreezerId,SE.BoxNo BoxId,SE.BoxMake BoxName
        //	                            FROM DeliveryChallan DC
        //                                left JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
        //                                left JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
        //								left JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
        //                                left JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
        //                                left JOIN Customer C ON C.CustomerId=SO.CustomerId 
        //								left join Country CCC ON CCC.CountryId=c.Country
        //                                left JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
        //                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
        //                                left JOIN Employee E ON E.EmployeeId=DC.EmployeeId
        //                                left join Organization O ON  DC.OrganizationId=O.OrganizationId
        //								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
        //                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
        //								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
        //								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
        //								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
        //								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
        //								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
        //								left join [User] U ON U.UserId=DC.CreatedBy
        //								left join ServiceEnquiry SE On SE.ServiceEnquiryId=SO.ServiceEnquiryId
        //								WHERE DC.DeliveryChallanId@DeliveryChallanId";


        //                    var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
        //                    {
        //                        DeliveryChallanId = DeliveryChallanId,
        //                        OrganizationId = OrganizationId,


        //                    }).First<DeliveryChallan>();

        //                    return objDeliveryChallan;

        //                }
        //            }
        //        }

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
                                CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId, TransportWarrantyExpiryDate = @TransportWarrantyExpiryDate,
                                PrintDescription = @PrintDescription, QuotationRefNo = @QuotationRefNo
                                WHERE DeliveryChallanId = @DeliveryChallanId;";
                    var id = connection.Execute(sql, objDeliveryChallan, txn);

                    sql = @"UPDATE ItemBatch SET DeliveryChallanId = NULL WHERE DeliveryChallanId = @DeliveryChallanId";
                    id = connection.Execute(sql, objDeliveryChallan, txn);

                    #region update customer order ref to [SaleOrder]
                    //if (objDeliveryChallan.isService == 1)
                    //{
                        sql = @"UPDATE SaleOrder SET CustomerOrderRef = '" + objDeliveryChallan.CustomerOrderRef + @"'
                                WHERE SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = " + objDeliveryChallan.JobCardId + @")";
                        if (connection.Execute(sql, transaction: txn) <= 0) throw new Exception();
                    //}
                    #endregion

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

                    #region delete and insert print description
                    try
                    {
                        sql = @"DELETE FROM PrintDescription WHERE DeliveryChallanId = @id";
                        connection.Execute(sql, new { @id = objDeliveryChallan.DeliveryChallanId }, txn);
                        sql = @"INSERT INTO PrintDescription (DeliveryChallanId, Description, UoM, Quantity, CreatedBy, CreatedDate, OrganizationId)
                                VALUES (@DeliveryChallanId, @Description, @UoM, @Quantity, @CreatedBy, GETDATE(), @OrganizationId)";
                        foreach (var item in objDeliveryChallan.PrintDescriptions)
                        {
                            if (item.Description == null) continue;
                            item.DeliveryChallanId = objDeliveryChallan.DeliveryChallanId;
                            item.CreatedBy = int.Parse(objDeliveryChallan.CreatedBy);
                            item.OrganizationId = objDeliveryChallan.OrganizationId;
                            if (connection.Execute(sql, item, txn) <= 0) throw new Exception();
                        }
                    }
                    catch (Exception) { throw new Exception(); }
                    #endregion
                    #region update Registration No. to [Vehicle Inpass]
                    if (objDeliveryChallan.isService == 0)
                    {
                        sql = @"UPDATE VehicleInpass SET RegistrationNo = '" + objDeliveryChallan.RegistrationNo + @"'
                                WHERE VehicleInPassId = (SELECT InPassId FROM JobCard WHERE JobCardId = " + objDeliveryChallan.JobCardId + @")";
                        if (connection.Execute(sql, transaction: txn) <= 0) throw new Exception();
                    }
                    #endregion
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
        public List<PrintDescription> GetDeliveryChallanDTPrint(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"  select [Description],UoM,cast(Quantity as int) Quantity from [dbo].[PrintDescription] where [DeliveryChallanId]=@Id";

                return connection.Query<PrintDescription>(sql, new { Id = Id }).ToList();
            }
        }
        public DeliveryChallan GetDeliveryChallanHD(int DeliveryChallanId, int OrganizationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sq;
                string qu;
                string sql = @"select isService from DeliveryChallan D inner join JobCard J ON J.JobCardId=D.JobCardId where D.DeliveryChallanId=@DeliveryChallanId";
                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new { OrganizationId = OrganizationId, DeliveryChallanId = DeliveryChallanId }).First<DeliveryChallan>();
                if (objDeliveryChallan.isService == 0)
                {
                    #region old query 27.12.2016 6.50p
                    //                    sq = @"SELECT DISTINCT O.*,
                    //								ORR.CountryName,
                    //								DC.DeliveryChallanId,
                    //								DeliveryChallanRefNo,
                    //								DeliveryChallanDate,
                    //								C.CustomerName Customer,
                    //								C.DoorNo CDoorNo,
                    //								C.Street CStreet,
                    //								CCC.CountryName CCountry,
                    //								ISNULL(SO.SaleOrderRefNo,'')SaleRefNo,CONVERT(varchar,SO.SaleOrderDate,106) SONODATE,
                    //								ISNULL(JC.JobCardNo,'')JobcardNo,CONVERT(varchar,JC.JobCardDate,106)JobCardDate,
                    //								VI. RegistrationNo,VI.ChassisNo,
                    //								WI.WorkDescr,
                    //								VM.VehicleModelName VehicleModel,
                    //								E.EmployeeName,
                    //								SO.PaymentTerms,
                    //								DC.Remarks,
                    //								SQ.QuotationRefNo,
                    //								I.[ItemName] FreezerName,I.ItemId ReeferId,
                    //								IB.SerialNo FreezerPartNo,
                    //								--ISNULL(I.PartNo,'') FreezerPartNo,
                    //                                QC.PunchingNo Box,
                    //								II.ItemName Box, II.ItemId Box,
                    //								IB1.SerialNo BoxPartNo,
                    //								--ISNULL(II.PartNo, '') BoxPartNo,
                    //								LPO.SupplyOrderNo,
                    //								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig, U1.Signature ApprovedUsersig,
                    //								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate,
                    //                                DC.PrintDescription printdes,
                    //                                JC.JobCardId
                    //	                            FROM DeliveryChallan DC
                    //                                left JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                    //                                left JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
                    //								left JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
                    //                                left JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
                    //                                left JOIN Customer C ON C.CustomerId=SO.CustomerId 
                    //								left join Country CCC ON CCC.CountryId=c.Country
                    //                                left JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                    //                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                    //                                left JOIN Employee E ON E.EmployeeId=DC.EmployeeId
                    //                                left join Organization O ON  DC.OrganizationId=O.OrganizationId
                    //								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
                    //                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
                    //								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
                    //								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
                    //								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
                    //								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
                    //								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
                    //								left join [User] U ON U.UserId=DC.CreatedBy
                    //								left join [User] U1 ON U1.UserId=SOI.PaymentApprovedForDeliveryCreatedBy
                    //							    left join Item I ON I.ItemId=JC.[FreezerUnitId]
                    //								left join Item II ON II.ItemId=JC.[BoxId]
                    //                                LEFT JOIN JobCardQC QC ON JC.JobCardId = QC.JobCardId
                    //								LEFT JOIN ItemBatch IB ON DC.DeliveryChallanId = IB.DeliveryChallanId 
                    //								LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND I.ItemId = GI.ItemId
                    //								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId AND I.ItemId = OS.ItemId
                    //
                    //								LEFT JOIN ItemBatch IB1 ON DC.DeliveryChallanId = IB1.DeliveryChallanId 
                    //								LEFT JOIN GRNItem GI1 ON IB1.GRNItemId = GI1.GRNItemId AND II.ItemId = GI1.ItemId
                    //								LEFT JOIN OpeningStock OS1 ON IB1.OpeningStockId = OS1.OpeningStockId AND II.ItemId = OS1.ItemId
                    //								WHERE DC.DeliveryChallanId = @DeliveryChallanId
                    //								AND ISNULL(GI.ItemId, OS.ItemId) = JC.FreezerUnitId
                    //								AND ISNULL(GI1.ItemId, OS1.ItemId) = JC.BoxId"; 
                    #endregion

                    sq = @"DECLARE @FreezerSerialNo VARCHAR(MAX), @BoxSerialNo VARCHAR(MAX), @TailLiftSerialNo VARCHAR(MAX), @TailLiftName VARCHAR(MAX);
                            SELECT
	                            @FreezerSerialNo = IB.SerialNo
                            FROM DeliveryChallan DC
                            LEFT JOIN JobCard JC ON DC.JobCardId = JC.JobCardId
                            LEFT JOIN Item I ON JC.FreezerUnitId = I.ItemId
                            LEFT JOIN ItemBatch IB ON DC.DeliveryChallanId = IB.DeliveryChallanId 
                            LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND I.ItemId = GI.ItemId
                            LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId AND I.ItemId = OS.ItemId
                            WHERE DC.DeliveryChallanId = @DeliveryChallanId
                            AND ISNULL(GI.ItemId, OS.ItemId) = JC.FreezerUnitId;
                            SELECT
	                            @BoxSerialNo = IB.SerialNo
                            FROM DeliveryChallan DC
                            LEFT JOIN JobCard JC ON DC.JobCardId = JC.JobCardId
                            LEFT JOIN Item I ON JC.BoxId = I.ItemId
                            LEFT JOIN ItemBatch IB ON DC.DeliveryChallanId = IB.DeliveryChallanId 
                            LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND I.ItemId = GI.ItemId
                            LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId AND I.ItemId = OS.ItemId
                            WHERE DC.DeliveryChallanId = @DeliveryChallanId
                            AND ISNULL(GI.ItemId, OS.ItemId) = JC.BoxId
                            SELECT
	                            @TailLiftSerialNo = IB.SerialNo,
								@TailLiftName = I.ItemName
                            FROM DeliveryChallan DC
							INNER JOIN JobCard JC ON DC.JobCardId = JC.JobCardId
                            INNER JOIN ItemBatch IB ON DC.DeliveryChallanId = IB.DeliveryChallanId
                            LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId --AND I.ItemId = GI.ItemId
                            LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId --AND I.ItemId = OS.ItemId
							LEFT JOIN Item I ON GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId
							INNER JOIN SaleOrderMaterial SOM ON SOM.SaleOrderId = JC.SaleOrderId 
								AND (GI.ItemId = SOM.ItemId OR OS.ItemId = SOM.ItemId)
                            WHERE DC.DeliveryChallanId = @DeliveryChallanId AND ISNULL(I.BatchRequired, 0) = 1

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
								ISNULL(I.[ItemName], '') + (CASE WHEN (ISNULL(I.[ItemName], '') = '' OR ISNULL(@TailLiftName, '') = '')
									THEN '' ELSE ' / ' END)  + ISNULL(@TailLiftName, '') FreezerName,
								I.ItemId ReeferId,
								(ISNULL(@FreezerSerialNo, '') + 
									(CASE WHEN (ISNULL(@FreezerSerialNo, '') = '' OR ISNULL(@TailLiftSerialNo, '') = '')
									THEN '' ELSE ' / ' END)
									+ ISNULL(@TailLiftSerialNo, '')) FreezerPartNo,
								--IB.SerialNo FreezerPartNo,
								--ISNULL(I.PartNo,'') FreezerPartNo,
                                QC.PunchingNo Box,
								--II.ItemName Box, II.ItemId Box,
								@BoxSerialNo BoxPartNo,
								--IB1.SerialNo BoxPartNo,
								--ISNULL(II.PartNo, '') BoxPartNo,
								LPO.SupplyOrderNo,
								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig, U1.Signature ApprovedUsersig,
								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate,
                                DC.PrintDescription printdes,
                                JC.JobCardId
	                            FROM DeliveryChallan DC
                                left JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                                left JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
								left JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
                                left JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
                                left JOIN Customer C ON C.CustomerId=SO.CustomerId 
								left join Country CCC ON CCC.CountryId=c.Country
                                left JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                                left JOIN Employee E ON E.EmployeeId=DC.EmployeeId
                                left join Organization O ON  DC.OrganizationId=O.OrganizationId
								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
								left join [User] U ON U.UserId=DC.CreatedBy
								left join [User] U1 ON U1.UserId=SOI.PaymentApprovedForDeliveryCreatedBy
							    left join Item I ON I.ItemId=JC.[FreezerUnitId]
								left join Item II ON II.ItemId=JC.[BoxId]
                                LEFT JOIN JobCardQC QC ON JC.JobCardId = QC.JobCardId
								WHERE DC.DeliveryChallanId=@DeliveryChallanId";
                    var objDeliveryChalla = connection.Query<DeliveryChallan>(sq, new
                    {
                        DeliveryChallanId = DeliveryChallanId,
                        OrganizationId = OrganizationId,


                    }).First<DeliveryChallan>();

                    return objDeliveryChalla;
                }
                else
                {
                    qu = @"    SELECT DISTINCT O.*,SO.SaleOrderId,
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
								--VI. RegistrationNo,VI.ChassisNo,
                                SE.VehicleRegNo RegistrationNo,SE.VehicleChassisNo ChassisNo,
								WI.WorkDescr,
								VM.VehicleModelName VehicleModel,
								E.EmployeeName,
								SO.PaymentTerms,
								DC.Remarks,
								SQ.QuotationRefNo,
								LPO.SupplyOrderNo,
								LPO.SupplyOrderDate,U.UserName CreatedUser,U.Signature CreatedUsersig, U1.Signature ApprovedUsersig,
								SO.CustomerOrderRef LPONo,SO.SaleOrderDate LPODate,
                                DC.PrintDescription printdes,
								JC.isService,
								concat(SE.FreezerModel,CASE WHEN (ISNULL(LTRIM(RTRIM(SE.FreezerModel)),'') = '' OR ISNULL(LTRIM(RTRIM(SE.TailLiftModel)), '') = '') THEN '' ELSE ' / ' END,SE.TailLiftModel)FreezerName,
								concat(SE.FreezerSerialNo,CASE WHEN (ISNULL(LTRIM(RTRIM(SE.FreezerSerialNo)),'') = '' OR ISNULL(LTRIM(RTRIM(SE.TailLiftSerialNo)), '') = '') THEN '' ELSE ' / ' END,SE.TailLiftSerialNo)FreezerPartNo,
								SE.BoxNo Box,SE.BoxMake BoxPartNo, DC.QuotationRefNo
	                            FROM DeliveryChallan DC
                                left JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                                left JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
								left JOIN SalesQuotation SQ ON SQ.SalesQuotationId=SO.SalesQuotationId
                                left JOIN SaleOrderItem SOI ON JC.SaleOrderItemId=SOI.SaleOrderItemId
                                left JOIN Customer C ON C.CustomerId=SO.CustomerId 
								left join Country CCC ON CCC.CountryId=c.Country
                                left JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                                LEFT JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                                left JOIN Employee E ON E.EmployeeId=DC.EmployeeId
                                left join Organization O ON  DC.OrganizationId=O.OrganizationId
								LEFT  JOIN Country ORR ON ORR.CountryId=O.Country
                                LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
								LEFT join WorkShopRequest W ON W.SaleOrderId=SO.SaleOrderId
								LEFT join PurchaseRequest PR ON PR.WorkShopRequestId=w.WorkShopRequestId
								LEFT join PurchaseRequestItem PRI ON pRI.PurchaseRequestId=PR.PurchaseRequestId
								left join SupplyOrderItem LPOI ON PRI.PurchaseRequestItemId = LPOI.PurchaseRequestItemId
								LEFT JOIN SupplyOrder LPO ON LPOI.SupplyOrderId = LPO.SupplyOrderId
								left join [User] U ON U.UserId=DC.CreatedBy
								left join [User] U1 ON U1.UserId=SOI.PaymentApprovedForDeliveryCreatedBy
								left join ServiceEnquiry SE On SE.ServiceEnquiryId=SO.ServiceEnquiryId
								WHERE DC.DeliveryChallanId=@DeliveryChallanId";
                }

                var objDeliveryChal = connection.Query<DeliveryChallan>(qu, new
                {
                    DeliveryChallanId = DeliveryChallanId,
                    OrganizationId = OrganizationId

                }).First<DeliveryChallan>();

                return objDeliveryChal;
            }
        }


        public List<PrintDescription> GetPrintDescriptions(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"SELECT * FROM PrintDescription WHERE DeliveryChallanId = @id";
                    return connection.Query<PrintDescription>(sql, new { id = id }).ToList();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
                catch (Exception)
                {
                    throw new Exception("Error occurred while fetching print descriptions");
                }
            }
        }
        /// <summary>
        /// Get quotation number, customer order reference and isService from jobcard id
        /// </summary>
        /// <param name="id">JobCardId</param>
        /// <param name="OrganizationId">OrganizationId</param>
        /// <returns></returns>
        public DeliveryChallan GetDetailsFromJobCard(int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"SELECT
	                                    JC.isService,
	                                    SQ.QuotationRefNo,
	                                    SO.CustomerOrderRef,
                                        QC.PunchingNo,VI.RegistrationNo
                                    FROM JobCard JC
                                    LEFT JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
                                    LEFT JOIN SalesQuotation SQ ON SO.SalesQuotationId = SQ.SalesQuotationId
									LEFT JOIN JobCardQC QC ON JC.JobCardId = QC.JobCardId
                                    LEFT JOIN VehicleInPass VI ON VI.VehicleInPassId=JC.InPassId
                                    WHERE JC.JobCardId = " + id + @" AND JC.OrganizationId = " + OrganizationId;
                    var deliverychallan = connection.Query<DeliveryChallan>(sql).FirstOrDefault();
                  
                   string query = @"SELECT
	                        WRI.ItemId,
	                        SUM(WRI.Quantity) Quantity
                        INTO #WORK
                        FROM WorkShopRequest WR
                        INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                        INNER JOIN Item I ON WRI.ItemId = I.ItemId
                        WHERE ISNULL(I.isConsumable, 0) = 0 AND (WR.JobCardId = @JobCardId OR 
                        WR.SaleOrderItemId = (SELECT SaleOrderItemId FROM JobCard WHERE JobCardId = @JobCardId)
                        OR (WR.SaleOrderItemId = 0 AND ISNULL(WR.JobCardId, 0) = 0 AND WR.SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = @JobCardId)))
                        GROUP BY WRI.ItemId

                        SELECT
	                        WRI.ItemId,
	                        SUM(SII.IssuedQuantity) Quantity
                        INTO #ISSUE
                        FROM WorkShopRequest WR
                        INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                        INNER JOIN Item I ON WRI.ItemId = I.ItemId
                        LEFT JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                        WHERE ISNULL(I.isConsumable, 0) = 0 AND (WR.JobCardId = @JobCardId OR 
                        WR.SaleOrderItemId = (SELECT SaleOrderItemId FROM JobCard WHERE JobCardId = @JobCardId)
                        OR (WR.SaleOrderItemId = 0 AND ISNULL(WR.JobCardId, 0) = 0 AND WR.SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = @JobCardId)))
                        GROUP BY WRI.ItemId

                        SELECT
	                        COUNT(#WORK.ItemId)
                        FROM #WORK
	                        LEFT JOIN #ISSUE ON #WORK.ItemId = #ISSUE.ItemId
                        WHERE #WORK.Quantity > ISNULL(#ISSUE.Quantity, 0)

                        DROP TABLE #ISSUE;
                        DROP TABLE #WORK;";
                   int val = connection.Query<int>(query, new { JobCardId = id }).First();
                    deliverychallan.StoreIssued = val == 0 ? true : false;
                    //jobcard.JobCardTask = new List<JobCardCompletionTask>();

                    //foreach (JobCardCompletionTask item in tasks)
                    //{
                    //    jobcard.JobCardTask.Add(item);
                    //}
                    return deliverychallan;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public IEnumerable<DeliveryChallan> GetWarrantyExtensionRegister()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select c.CustomerName,J.JobCardId,J.JobCardNo,J.JobCardDate,V.ChassisNo,V.RegistrationNo,VM.VehicleModelName,W.WorkDescr,D.DeliveryChallanDate,I.SerialNo,I.WarrantyExpireDate from [dbo].[JobCard] J
                                INNER JOIN [dbo].[VehicleInPass] V ON J.InPassId=V.VehicleInPassId
                                INNER JOIN SaleOrderItem SI ON SI.SaleOrderItemId=J.SaleOrderItemId
                                INNER JOIN SaleOrder S ON S.SaleOrderId=SI.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
                                INNER JOIN VehicleModel VM ON VM.VehicleModelId=SI.VehicleModelId
                                INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                                LEFT JOIN DeliveryChallan D ON D.JobCardId=J.JobCardId
                                LEFT JOIN [dbo].[ItemBatch] I ON D.DeliveryChallanId= I.DeliveryChallanId
                                LEFT JOIN GRNItem G ON G.GRNItemId=I.GRNItemId
                                LEFT JOIN OpeningStock O ON O.OpeningStockId=I.OpeningStockId
                                WHERE (G.ItemId =J.FreezerUnitId OR O.ItemId=J.FreezerUnitId )";
                return connection.Query<DeliveryChallan>(sql, new { }).ToList();
            }
        }
    }
}
