using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderFollowupRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSupplyOrderFollowup(IList<SupplyOrderFollowup> model)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                foreach (SupplyOrderFollowup item in model)
                {
                    if (item.ExpectedDate != null)
                    {
                        string checksql = @"DELETE from SupplyOrderFollowup where SupplyOrderItemId=@SupplyOrderItemId ";

                        connection.Query<int>(checksql, item);

                        string sql = @"insert  into SupplyOrderFollowup(SupplyOrderItemId,SupplyOrderFollowupDate,ExpectedDate,Remarks,CreatedBy,CreatedDate,OrganizationId) Values (@SupplyOrderItemId,@SupplyOrderFollowupDate,@ExpectedDate,@Remarks,@CreatedBy,@CreatedDate,@OrganizationId);
                              SELECT CAST(SCOPE_IDENTITY() as int)";

                       // InsertLoginHistory(dataConnection, item.CreatedBy, "Create", "SupplyOrderFollowup", "0", "0");
                        int objCustomerVsSalesExecutive = connection.Query<int>(sql, item).First();
                    }
                }

                return 1;
            }
        }
        public SupplyOrderFollowUpList GetSupplyOrderFollowup(int OrganizationId, string name, string SupplierName, int? batch)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                SupplyOrderFollowUpList model = new SupplyOrderFollowUpList();

                string query = @"
                            SELECT  distinct SI.SupplyOrderItemId,I.ItemName,case when I.CriticalItem = 1 then 'Critical' else '-' end CriticalItem,
                                SI.OrderedQty,Su.SupplierName,convert(varchar,SF.ExpectedDate,106) ExpectedDate,S.RequiredDate,SF.Remarks,I.BatchRequired,
                                CONCAT(S.SupplyOrderNo, ' - ' , convert(varchar,S.SupplyOrderDate,106) ) SupplyOrderDate
                                from SupplyOrder S inner join Supplier Su on  Su.SupplierId=S.SupplierId
                                inner join  SupplyOrderItem SI on S.SupplyOrderId=SI.SupplyOrderId
                                inner join PurchaseRequestItem PI on PI.PurchaseRequestItemId=SI.PurchaseRequestItemId 
                                inner join Item I on PI.ItemId=I.ItemId
								left join SupplyOrderFollowup SF on SF.SupplyOrderItemId=SI.SupplyOrderItemId where S.OrganizationId=@OrganizationId and ItemName LIKE '%'+@name+'%' 
                                and  SupplierName Like '%'+@SupplierName+'%' and BatchRequired = ISNULL(@batch,BatchRequired)
								AND ISNULL(S.isApproved, 0) = 1";

               // model.SupplyOrderFollowups = connection.Query<SupplyOrderFollowup>(query).ToList<SupplyOrderFollowup>();
                model.SupplyOrderFollowups = connection.Query<SupplyOrderFollowup>(query, new
                {
                    OrganizationId = OrganizationId,
                    name=name,
                    SupplierName=SupplierName,
                    batch = batch
                }).ToList<SupplyOrderFollowup>();
                return model;
            }
        }



        //        public SupplyOrderFollowup GetSupplyOrderFollowup(int SupplyOrderFollowupId)
        //        {

        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                string sql = @"select * from SupplyOrderFollowup
        //                        where SupplyOrderFollowupId=@SupplyOrderFollowupId";

        //                var objSupplyOrderFollowup = connection.Query<SupplyOrderFollowup>(sql, new
        //                {
        //                    SupplyOrderFollowupId = SupplyOrderFollowupId
        //                }).First<SupplyOrderFollowup>();

        //                return objSupplyOrderFollowup;
        //            }
        //        }

        //        public List<SupplyOrderFollowup> GetSupplyOrderFollowups()
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                string sql = @"select * from SupplyOrderFollowup
        //                        where isActive=1";

        //                var objSupplyOrderFollowups = connection.Query<SupplyOrderFollowup>(sql).ToList<SupplyOrderFollowup>();

        //                return objSupplyOrderFollowups;
        //            }
        //        }



        //public int DeleteSupplyOrderFollowup(Unit objSupplyOrderFollowup)
        //{
        //    using (IDbConnection connection = OpenConnection(dataConnection))
        //    {
        //        string sql = @"Delete SupplyOrderFollowup  OUTPUT DELETED.SupplyOrderFollowupId WHERE SupplyOrderFollowupId=@SupplyOrderFollowupId";


        //        var id = connection.Execute(sql, objSupplyOrderFollowup);
        //        return id;
        //    }
        //}


    }
}
