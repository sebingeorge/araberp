﻿using System;
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
                    objDeliveryChallan.DeliveryChallanRefNo = "DEL/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection,
                        txn, typeof(DeliveryChallan).Name, "0", 1);

                    string sql = @"insert into DeliveryChallan(JobCardId,DeliveryChallanRefNo,DeliveryChallanDate,EmployeeId,Remarks,CreatedBy,CreatedDate,OrganizationId,isActive) Values (@JobCardId,@DeliveryChallanRefNo,@DeliveryChallanDate,@EmployeeId,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId,1);
                                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = connection.Query<int>(sql, objDeliveryChallan, txn).Single();

                    foreach (var item in objDeliveryChallan.ItemBatches)
                    {
                        item.DeliveryChallanId = id;
                        sql = @"UPDATE ItemBatch SET DeliveryChallanId = @DeliveryChallanId, WarrantyStartDate = @WarrantyStartDate, WarrantyExpireDate = @WarrantyExpireDate
                                WHERE ItemBatchId = @ItemBatchId";
                        connection.Execute(sql, item, txn);
                    }
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
                return id;
            }
        }
        /// <summary>
        /// Return all completed jobcards that are not in vehicle out pass
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<PendingJC> PendingDeliveryChallan(int customerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingJC>(@"SELECT ISNULL(SO.SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderNoDate, VM.VehicleModelName+' - '+VM.VehicleModelDescription VehicleModel, WD.WorkDescr, CUS.CustomerName, SOI.SaleOrderItemId,SOI.IsPaymentApprovedForDelivery INTO #TEMP FROM SaleOrderItem SOI
                    INNER JOIN SaleOrder SO ON SO.SaleOrderId = SOI.SaleOrderId
                    INNER JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                    INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                    WHERE CUS.CustomerId = @customerId AND ISNULL(SOI.isActive, 1) = 1 AND ISNULL(VM.isActive, 1) = 1;

                    SELECT J.JobCardId, ISNULL(J.JobCardNo, '')+' - '+CONVERT(VARCHAR, J.JobCardDate, 106) JobCardNoDate, T.SaleOrderNoDate, T.VehicleModel, T.WorkDescr, T.CustomerName, ISNULL(VI.RegistrationNo, '-') RegistrationNo,T.IsPaymentApprovedForDelivery FROM JobCard J 
                    LEFT JOIN DeliveryChallan VO ON J.JobCardId = VO.JobCardId
                    INNER JOIN #TEMP T ON J.SaleOrderItemId = T.SaleOrderItemId
                    LEFT JOIN VehicleInPass VI ON T.SaleOrderItemId = VI.SaleOrderItemId
                    WHERE ISNULL(J.JodCardCompleteStatus, 0) = 1 AND VO.JobCardId IS NULL;

                    DROP TABLE #TEMP;", new { customerId = customerId }).ToList();
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
                    return connection.Query<PendingJC>(@"SELECT ISNULL(SO.SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderNoDate, VM.VehicleModelName+' - '+VM.VehicleModelDescription VehicleModel, WD.WorkDescr, CUS.CustomerName, SOI.SaleOrderItemId INTO #TEMP FROM SaleOrderItem SOI
                    INNER JOIN SaleOrder SO ON SO.SaleOrderId = SOI.SaleOrderId
                    INNER JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                    INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                    WHERE ISNULL(SOI.isActive, 1) = 1 AND ISNULL(VM.isActive, 1) = 1;

                    SELECT ISNULL(J.JobCardNo, '')+' - '+CONVERT(VARCHAR, J.JobCardDate, 106) JobCardNoDate, T.SaleOrderNoDate, T.VehicleModel, T.WorkDescr, T.CustomerName, ISNULL(VI.RegistrationNo, '-') RegistrationNo FROM JobCard J 
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
    }
}