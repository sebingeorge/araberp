using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class IndentVsSOVsInpassStatusRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<IndentVsSOVsInpassStatus> GetIndentRegister(string supplier = "", string item = "", string indentno = "", string sono = "", string grnno = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;

                sql = @"SELECT S.SupplierName Supplier,IG.ItemGroupName ItemGroup,I.ItemName Item,
                        P.PurchaseRequestNo IndentNo,Convert(varchar(50),P.PurchaseRequestDate,106) IndenDate,PI.Quantity IndentQty,PRU.UserName IndentUser,
                        SO.SupplyOrderNo SONo,Convert(varchar(50),SO.SupplyOrderDate,106) SODate,SOI.OrderedQty SOQty,SOU.UserName SOUser,Convert(varchar(50),SO.RequiredDate,106) SODueDate,SOI.Rate SORate,SOI.Amount SOAmount,
                        G.GRNNo GRNNo,Convert(varchar(50),G.GRNDate,106) GRNDate,GI.Quantity GRNQty,GU.UserName GRNUser,
                        (SOI.OrderedQty-GI.Quantity)PendingQty
                        FROM PurchaseRequest P
                        INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestId=P.PurchaseRequestId
                        INNER JOIN Item I ON I.ItemId=PI.ItemId
                        INNER JOIN ItemGroup IG ON IG.ItemGroupId=I.ItemGroupId
                        INNER JOIN [User] PRU ON PRU.UserId=P.CreatedBy
                        LEFT JOIN SupplyOrderItem SOI ON SOI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                        LEFT JOIN SupplyOrder SO ON SO.SupplyOrderId=SOI.SupplyOrderId
                        LEFT JOIN [User] SOU ON SOU.UserId=SO.CreatedBy
                        LEFT JOIN Supplier S ON S.SupplierId=SO.SupplierId
                        LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SOI.SupplyOrderItemId AND GI.ItemId=I.ItemId
                        LEFT JOIN GRN G ON G.GRNId=GI.GRNId
                        LEFT JOIN [User] GU ON GU.UserId=G.CreatedBy
                        WHERE isnull(S.SupplierName,'')  LIKE '%'+@supplier+'%'
                        AND isnull(I.ItemName,'')  LIKE '%'+@item+'%'
                        AND isnull(P.PurchaseRequestNo,'')  LIKE '%'+@indentno+'%'
                        AND isnull(SO.SupplyOrderNo,'')  LIKE '%'+@sono+'%'
                        AND isnull(G.GRNNo,'')  LIKE '%'+@grnno+'%'";

                return connection.Query<IndentVsSOVsInpassStatus>(sql, new { supplier = supplier, item = item, indentno = indentno, sono = sono, grnno = grnno });
            }
        }
    }
}
