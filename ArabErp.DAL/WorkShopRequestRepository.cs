using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkShopRequestRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

              //  var trn = connection.BeginTransaction();
                int id = 0;

                try
                {
                    string sql = @"insert  into WorkShopRequest(WorkShopRequestNo,WorkShopRequestDate,SaleOrderId,CustomerId,CustomerOrderRef,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@WorkShopRequestNo,@WorkShopRequestDate,@SaleOrderId,@CustomerId,@CustomerOrderRef,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                 id = connection.Query<int>(sql, objWorkShopRequest).Single();



               
                var workshopitemitemrepo = new WorkShopRequestItemRepository();
                foreach (var item in objWorkShopRequest.Items)
                {
                    item.WorkShopRequestId = id;
                    workshopitemitemrepo.InsertWorkShopRequestItem(item);
                }
                   // trn.Commit();
                }
                catch (Exception e)
                {
                   // trn.Rollback();
                    throw;
                }
                return id;
            }
        }

        public WorkShopRequest GetWorkShopRequest(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from WorkShopRequest
                        where WorkShopRequestId=@WorkShopRequestId";

                var objWorkShopRequest = connection.Query<WorkShopRequest>(sql, new
                {
                    WorkShopRequestId = WorkShopRequestId
                }).First<WorkShopRequest>();

                return objWorkShopRequest;
            }
        }

        public List<WorkShopRequest> GetWorkShopRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopRequest
                        where isActive=1";

                var objWorkShopRequests = connection.Query<WorkShopRequest>(sql).ToList<WorkShopRequest>();

                return objWorkShopRequests;
            }
        }
        public int UpdateWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE WorkShopRequest SET WorkShopRequestNo = @WorkShopRequestNo ,WorkShopRequestDate = @WorkShopRequestDate ,SaleOrderId = @SaleOrderId ,CustomerId = @CustomerId,CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkShopRequestId  WHERE WorkShopRequestId = @WorkShopRequestId";


                var id = connection.Execute(sql, objWorkShopRequest);
                return id;
            }
        }

        public int DeleteWorkShopRequest(Unit objWorkShopRequest)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkShopRequest  OUTPUT DELETED.WorkShopRequestId WHERE WorkShopRequestId=@WorkShopRequestId";


                var id = connection.Execute(sql, objWorkShopRequest);
                return id;
            }
        }
        /// <summary>
        /// Return details of a job card such as Sale Order No., Customer Name, Customer Order Ref. No.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WorkShopRequest GetJobCardDetails(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT SO.SaleOrderId, SO.SaleOrderRefNo, C.CustomerName, C.CustomerId, SO.CustomerOrderRef FROM JobCard JC INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId INNER JOIN Customer C ON C.CustomerId = SO.CustomerId WHERE JC.JobCardId = @JobCardId";
                return connection.Query<WorkShopRequest>(query,
                    new { JobCardId = jobCardId }).Single();
            }
        }
        /// <summary>
        /// Returns the part number of a given item
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string GetItemPartNo(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<string>("SELECT PartNo FROM Item WHERE ItemId = @ItemId",
                    new { ItemId = itemId }).First<string>();
            }
        }
        /// <summary>
        /// Insert additional workshop request head table (WorkShopRequest table)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertAdditionalWorkshopRequest(WorkShopRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO WorkShopRequest(
                                    WorkShopRequestNo, 
                                    WorkShopRequestDate, 
                                    SaleOrderId, 
                                    CustomerId, 
                                    CustomerOrderRef, 
                                    SpecialRemarks, 
                                    RequiredDate, 
                                    CreatedBy, 
                                    CreatedDate, 
                                    OrganizationId, 
                                    isActive, 
                                    isAdditionalRequest, 
                                    JobCardId)
                                VALUES(
                                    @WorkShopRequestNo,
                                    @WorkShopRequestDate, 
                                    @SaleOrderId, 
                                    @CustomerId, 
                                    @CustomerOrderRef, 
                                    @SpecialRemarks, 
                                    @RequiredDate, 
                                    @CreatedBy, 
                                    @CreatedDate, 
                                    @OrganizationId, 
                                    1, 
                                    1, 
                                    @JobCardId);

                                SELECT CAST(SCOPE_IDENTITY() as int)";
        public List<WorkShopRequest> GetWorkShopRequestPending()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopRequest";

                var objWrkOrders = connection.Query<WorkShopRequest>(sql).ToList<WorkShopRequest>();

                return objWrkOrders;
            }
        }
        

                    int id = connection.Query<int>(query, model, txn).First();
                    foreach (var item in model.Items)
                    {
                        item.WorkShopRequestId = id;
                        new WorkShopRequestItemRepository().InsertAdditionalWorkshopRequestItem(item, connection, txn);
                    }
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return 0;
                }
            }
        }
    }
}