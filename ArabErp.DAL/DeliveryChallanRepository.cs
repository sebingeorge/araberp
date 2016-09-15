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
                    objDeliveryChallan.DeliveryChallanRefNo = DatabaseCommonRepository.GetNewDocNo(connection, objDeliveryChallan.OrganizationId, 18, true,txn);

                    string sql = @"insert into DeliveryChallan(JobCardId,DeliveryChallanRefNo,DeliveryChallanDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId,isActive) Values (@JobCardId,@DeliveryChallanRefNo,@DeliveryChallanDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId,1);
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

        public List<DeliveryChallan> GetDeliveryChallans()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from DeliveryChallan
                        where isActive=1";

                var objDeliveryChallans = connection.Query<DeliveryChallan>(sql).ToList<DeliveryChallan>();

                return objDeliveryChallans;
            }
        }



        public int DeleteDeliveryChallan(Unit objDeliveryChallan)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete DeliveryChallan  OUTPUT DELETED.DeliveryChallanId WHERE DeliveryChallanId=@DeliveryChallanId";


                var id = connection.Execute(sql, objDeliveryChallan);
                InsertLoginHistory(dataConnection, objDeliveryChallan.CreatedBy, "Delete", "Delivery Challan", id.ToString(), "0");
                return id;
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
                    INNER JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
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
                    INNER JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
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
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                INNER JOIN Item I ON GI.ItemId = I.ItemId
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

        public DeliveryChallan GetDeliveryChallanHD(int DeliveryChallanId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT DISTINCT DeliveryChallanId,DeliveryChallanRefNo,DeliveryChallanDate,C.CustomerName,
                                ISNULL(SO.SaleOrderRefNo,'')+ ' - '  +CONVERT(varchar,SO.SaleOrderDate,106) SaleOrderNoDate,
                                ISNULL(JC.JobCardNo,'') + ' - ' +CONVERT(varchar,JC.JobCardDate,106)JobCardNoDate,VI. RegistrationNo,
                                WI.WorkDescr,VM.VehicleModelName,E.EmployeeName,SO.PaymentTerms,DC.Remarks
                                FROM DeliveryChallan DC
                                INNER JOIN JobCard JC ON JC.JobCardId=DC.JobCardId
                                INNER JOIN SaleOrder SO ON SO.SaleOrderId=JC.SaleOrderId
                                INNER JOIN SaleOrderItem SOI ON SOI.SaleOrderId=SOI.SaleOrderId AND JC.SaleOrderItemId=SOI.SaleOrderItemId
                                INNER JOIN Customer C ON C.CustomerId=SO.CustomerId 
                                INNER JOIN VehicleInPass VI ON VI.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN WorkDescription WI ON WI.WorkDescriptionId = SOI.WorkDescriptionId
                                INNER JOIN VehicleModel VM ON VM.VehicleModelId=SOI.VehicleModelId
                                INNER JOIN Employee E ON E.EmployeeId=DC.[EmployeeId]
                                WHERE  DeliveryChallanId=@DeliveryChallanId";

                var objDeliveryChallan = connection.Query<DeliveryChallan>(sql, new
                {
                    DeliveryChallanId = DeliveryChallanId
                }).First<DeliveryChallan>();

                return objDeliveryChallan;
            }
        }

    }
}
