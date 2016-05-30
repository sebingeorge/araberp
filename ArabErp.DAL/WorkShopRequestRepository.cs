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

    }
}