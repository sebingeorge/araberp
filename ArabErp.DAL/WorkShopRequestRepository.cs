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



        /// <summary>
        /// Insert WorkShopRequest
        /// </summary>
        /// <param name="model"></param>
        /// <returns>primary key of WorkShopRequest </returns>
        public int InsertWorkShopRequest(WorkShopRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into WorkShopRequest(WorkShopRequestNo,WorkShopRequestDate,SaleOrderId,CustomerId,CustomerOrderRef,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@WorkShopRequestNo,@WorkShopRequestDate,@SaleOrderId,@CustomerId,@CustomerOrderRef,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, model, trn).Single();
                    var saleorderitemrepo = new WorkShopRequestItemRepository();
                    foreach (var item in model.Items)
                    {
                        item.WorkShopRequestId = id;
                        new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, trn);
                      
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

        public List<WorkShopRequestItem> GetWorkShopRequestData(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                 
                string query = "SELECT I.ItemName,I.ItemId,I.PartNo,SUM(WI.Quantity)Quantity,UnitName from WorkDescription W INNER JOIN  WorkVsItem WI on W.WorkDescriptionId=WI.WorkDescriptionId";
                       query += " INNER JOIN Item I ON WI.ItemId=I.ItemId INNER JOIN Unit U on U.UnitId =I.ItemUnitId  INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId";
                       query += " WHERE SI.SaleOrderId=@SaleOrderId GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName ";

                return connection.Query<WorkShopRequestItem>(query,
                new { SaleOrderId = SaleOrderId }).ToList();

               
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
        /// <summary>
        /// Returns all pending workshop requests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WorkShopRequest> PendingWorkshopRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<WorkShopRequest>(@"SELECT WorkShopRequestId, SUM(Quantity) Quantity INTO #WORK FROM WorkShopRequestItem GROUP BY WorkShopRequestId;
                SELECT WorkShopRequestId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE FROM StoreIssueItem SII INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId GROUP BY WorkShopRequestId;
                SELECT CustomerId, CustomerName INTO #CUSTOMER FROM Customer;
				SELECT SaleOrderId, SaleOrderRefNo+', '+CONVERT(VARCHAR, SaleOrderDate, 106) SoNoWithDate INTO #SALE FROM SaleOrder;
                SELECT W.WorkShopRequestId, ISNULL(WR.WorkShopRequestNo, '-')+', '+CAST(CONVERT(VARCHAR, WR.WorkShopRequestDate, 106) AS VARCHAR) WorkShopRequestNo, ISNULL(CONVERT(DATETIME, WR.RequiredDate, 106), '01 Jan 1900') RequiredDate, C.CustomerName, S.SoNoWithDate FROM #WORK W LEFT JOIN #ISSUE I ON W.WorkShopRequestId = I.WorkShopRequestId INNER JOIN WorkShopRequest WR ON W.WorkShopRequestId = WR.WorkShopRequestId INNER JOIN #CUSTOMER C ON WR.CustomerId = C.CustomerId INNER JOIN #SALE S ON WR.SaleOrderId = S.SaleOrderId WHERE ISNULL(IssuedQuantity,0) < Quantity;
                DROP TABLE #ISSUE;
                DROP TABLE #WORK;
                DROP TABLE #CUSTOMER;
				DROP TABLE #SALE;").ToList();
            }
        }
    }
}