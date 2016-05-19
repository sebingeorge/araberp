using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class PurchaseRequestRepository : BaseRepository
    {

        public int InsertPurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            string sql = @"insert  into PurchaseRequest(PurchaseRequestNo,PurchaseRequestDate,WorkShopRequestId,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@WorkShopRequestId,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objPurchaseRequest).Single();
            return id;
        }


        public PurchaseRequest GetPurchaseRequest(int PurchaseRequestId)
        {

            string sql = @"select * from PurchaseRequest
                        where PurchaseRequestId=@PurchaseRequestId";

            var objPurchaseRequest = connection.Query<PurchaseRequest>(sql, new
            {
                PurchaseRequestId = PurchaseRequestId
            }).First<PurchaseRequest>();

            return objPurchaseRequest;
        }

        public List<PurchaseRequest> GetPurchaseRequests()
        {
            string sql = @"select * from PurchaseRequest
                        where isActive=1";

            var objPurchaseRequests = connection.Query<PurchaseRequest>(sql).ToList<PurchaseRequest>();

            return objPurchaseRequests;
        }

        public int UpdatePurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            string sql = @"UPDATE PurchaseRequest SET PurchaseRequestNo = @PurchaseRequestNo ,PurchaseRequestDate = @PurchaseRequestDate ,WorkShopRequestId = @WorkShopRequestId ,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate  OUTPUT INSERTED.PurchaseRequestId  WHERE PurchaseRequestId = @PurchaseRequestId";


            var id = connection.Execute(sql, objPurchaseRequest);
            return id;
        }

        public int DeletePurchaseRequest(Unit objPurchaseRequest)
        {
            string sql = @"Delete PurchaseRequest  OUTPUT DELETED.PurchaseRequestId WHERE PurchaseRequestId=@PurchaseRequestId";


            var id = connection.Execute(sql, objPurchaseRequest);
            return id;
        }


    }
}
