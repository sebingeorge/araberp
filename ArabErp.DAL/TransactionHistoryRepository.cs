using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class TransactionHistoryRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertTransactionHistory(TransactionHistory objTransactionHistory)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into TransactionHistory(UserId,TransTime,Mode,Form,FormTransCode,IPAddress,OrganizationId) Values (@UserId,@TransTime,@Mode,@Form,@FormTransCode,@IPAddress,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objTransactionHistory).Single();
                return id;
            }
        }


//        public TransactionHistory GetTransactionHistory(int TransactionHistoryId)
//        {

//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                string sql = @"select U.UserName, Form, Mode, TransTime, FormTransCode
//                                from TransactionHistory T
//                                inner join [User] U on T.UserId = U.UserId
//                                where FormTransCode is not null";

//                var objTransactionHistory = connection.Query<TransactionHistory>(sql, new
//                {
//                    TransactionHistoryId = TransactionHistoryId
//                }).First<TransactionHistory>();

//                return objTransactionHistory;
//            }
//        }

        public IEnumerable<TransactionHistory> GetTransactionHistorys(DateTime? from, DateTime? to,string user, string form, string mode)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT U.UserName, Form, Mode, TransTime, FormTransCode
                                FROM TransactionHistory T
                                INNER JOIN [User] U ON T.UserId = U.UserId
                                WHERE FormTransCode is not null  
                                AND U.UserName LIKE '%'+@user+'%'
                                AND Form LIKE '%'+@form+'%'   
                                AND Mode LIKE '%'+@mode+'%'
                               AND TransTime BETWEEN @from AND @to ";

                return connection.Query<TransactionHistory>(query, new { from = from, to = to,user = user, form = form, mode = mode }).ToList();
            }
        }

      

        public IEnumerable<ItemBatch> GetMaterialList(string serialno, int item, int type, int saleorder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT SO.SaleOrderId,IB.ItemBatchId,
                                    CONCAT('Opening Stock',' - ',ISNULL(CONVERT(VARCHAR,OS.CreatedDate,106),'-')) OSDATE,
	                                G.GRNNo,CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
	                                SO.SaleOrderRefNo, CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                I.ItemName,IB.SerialNo,U.EmployeeName AS CreatedBy,
                                    ISNULL(IB.DeliveryChallanId, 0) DeliveryChallanId,
									ISNULL(DC.DeliveryChallanRefNo, '-') DeliveryChallanRefNo,
									CONVERT(VARCHAR, IB.WarrantyExpireDate, 106) WarrantyExpireDate,
									DATEDIFF(MONTH, GETDATE(), IB.WarrantyExpireDate) WarrantyLeft,
									C.CustomerName
                                    FROM ItemBatch IB
                                    LEFT JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                    LEFT JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                    LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                    LEFT JOIN GRN G ON GI.GRNId = G.GRNId
                                    LEFT JOIN Item I ON GI.ItemId = I.ItemId
                                    LEFT JOIN Employee U ON IB.CreatedBy = U.EmployeeId
                                    LEFT JOIN Customer C ON SO.CustomerId = C.CustomerId
                                    LEFT JOIN DeliveryChallan DC ON IB.DeliveryChallanId = DC.DeliveryChallanId
                                    LEFT JOIN OpeningStock OS ON OS.ItemId=I.ItemId
                                    WHERE ISNULL(IB.isActive, 1) = 1
								    AND IB.SerialNo LIKE '%'+@serialno+'%'
								    AND I.ItemId = ISNULL(NULLIF(@item, 0), I.ItemId)
								    AND ISNULL(SO.SaleOrderId, 0) = ISNULL(NULLIF(ISNULL(@saleorder, 0), 0), ISNULL(SO.SaleOrderId, 0))
                                    ORDER BY G.GRNDate DESC, IB.CreatedDate DESC;";
                if (type == 1)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NOT NULL AND IB.DeliveryChallanId IS NULL ");
                }
                else if (type == 2)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NULL ");
                }
                else if (type == 3)
                {
                    //query = query.Insert(query.IndexOf("WHERE"), "LEFT JOIN ItemBatch IB1 ON SOI.SaleOrderItemId = SI.SaleOrderItemId ");
                    //query = query.Insert(query.IndexOf("ORDER BY"), "AND SI.SaleOrderItemId IS NOT NULL ");
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND IB.DeliveryChallanId IS NOT NULL ");
                }
                return connection.Query<ItemBatch>(query, new { serialno = serialno, item = item, saleorder = saleorder }).ToList();
            }
        }


        public int DeleteTransactionHistory(Unit objTransactionHistory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete TransactionHistory  OUTPUT DELETED.TransactionHistoryId WHERE TransactionHistoryId=@TransactionHistoryId";


                var id = connection.Execute(sql, objTransactionHistory);
                return id;
            }
        }


    }
}