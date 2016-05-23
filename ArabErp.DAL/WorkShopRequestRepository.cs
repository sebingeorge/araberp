using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class WorkShopRequestRepository : BaseRepository
    {

        public int InsertWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {
            string sql = @"insert  into WorkShopRequest(WorkShopRequestNo,WorkShopRequestDate,SaleOrderId,CustomerId,CustomerOrderRef,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@WorkShopRequestNo,@WorkShopRequestDate,@SaleOrderId,@CustomerId,@CustomerOrderRef,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objWorkShopRequest).Single();
            return id;
        }


        public WorkShopRequest GetWorkShopRequest(int WorkShopRequestId)
        {

            string sql = @"select * from WorkShopRequest
                        where WorkShopRequestId=@WorkShopRequestId";

            var objWorkShopRequest = connection.Query<WorkShopRequest>(sql, new
            {
                WorkShopRequestId = WorkShopRequestId
            }).First<WorkShopRequest>();

            return objWorkShopRequest;
        }

        public List<WorkShopRequest> GetWorkShopRequests()
        {
            string sql = @"select * from WorkShopRequest
                        where isActive=1";

            var objWorkShopRequests = connection.Query<WorkShopRequest>(sql).ToList<WorkShopRequest>();

            return objWorkShopRequests;
        }

        public int UpdateWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {
            string sql = @"UPDATE WorkShopRequest SET WorkShopRequestNo = @WorkShopRequestNo ,WorkShopRequestDate = @WorkShopRequestDate ,SaleOrderId = @SaleOrderId ,CustomerId = @CustomerId,CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkShopRequestId  WHERE WorkShopRequestId = @WorkShopRequestId";


            var id = connection.Execute(sql, objWorkShopRequest);
            return id;
        }

        public int DeleteWorkShopRequest(Unit objWorkShopRequest)
        {
            string sql = @"Delete WorkShopRequest  OUTPUT DELETED.WorkShopRequestId WHERE WorkShopRequestId=@WorkShopRequestId";


            var id = connection.Execute(sql, objWorkShopRequest);
            return id;
        }


    }
}