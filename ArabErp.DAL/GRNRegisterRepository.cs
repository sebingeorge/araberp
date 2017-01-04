using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class GRNRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        
        public IEnumerable<GRNRegister> GetGRNRegister(DateTime? from, DateTime? to,int id, string material = "",string partno="", string supplier = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;

                sql = @"SELECT GRNNo,GRNDate,S.SupplierName,isnull(SupplyOrderNo,'-')SupplyOrderNo,G.SupplierDCNoAndDate DCNo,
                        I.ItemName,isnull (I.PartNo,'-')PartNo,GI.Quantity,GI.Rate,GI.Amount,U.UnitName 
                        FROM  GRN G
                        INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                        INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                        INNER JOIN Item I ON I.ItemId =GI.ItemId
                        INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                        LEFT JOIN SupplyOrderItem SOI ON SOI.SupplyOrderItemId=GI.SupplyOrderItemId
                        LEFT JOIN SupplyOrder SO ON SO.SupplyOrderId=SOI.SupplyOrderId 
                        WHERE  GRNDate >= @from AND GRNDate <= @to
                        AND I.ItemGroupId=ISNULL(NULLIF(@id, 0),I.ItemGroupId)
                        AND isnull(I.ItemName,'')  LIKE '%'+@material+'%'
                        AND isnull(I.PartNo,'')  LIKE '%'+@partno+'%'
                        AND isnull(S.SupplierName,'')  LIKE '%'+@supplier+'%'";

                return connection.Query<GRNRegister>(sql, new { from = from, to = to, id = id, material = material, partno = partno,supplier = supplier });
            }
        }
        public IEnumerable<GRNRegister> GRNSummary(DateTime? from, DateTime? to, string supplier = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;

                sql = @"SELECT DISTINCT G.GRNId,GRNNo,GRNDate,S.SupplierName,isnull(SupplyOrderNo,'-')SupplyOrderNo,
                        G.SupplierDCNoAndDate DCNo,G.GrandTotal Amount
                        FROM  GRN G
                        INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                        INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                        LEFT JOIN SupplyOrderItem SOI ON SOI.SupplyOrderItemId=GI.SupplyOrderItemId
                        LEFT JOIN SupplyOrder SO ON SO.SupplyOrderId=SOI.SupplyOrderId 
                        WHERE  GRNDate >= @from AND GRNDate <= @to
                        AND isnull(S.SupplierName,'')  LIKE '%'+@supplier+'%'
	                    ORDER BY G.GRNId";

                return connection.Query<GRNRegister>(sql, new { from = from, to = to, supplier = supplier });
            }
        }

        public IEnumerable<GRNRegister> GRNDetailed(DateTime? from, DateTime? to, int id, string material = "", string partno = "", string supplier = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;

                sql = @"SELECT G.GRNId,GRNNo,GRNDate,S.SupplierName,isnull(SupplyOrderNo,'-')SupplyOrderNo,G.SupplierDCNoAndDate DCNo,
                        I.ItemName,isnull (I.PartNo,'-')PartNo,GI.Quantity,GI.Rate,GI.Amount,U.UnitName 
                        FROM  GRN G
                        INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                        INNER JOIN Supplier S ON S.SupplierId=G.SupplierId
                        INNER JOIN Item I ON I.ItemId =GI.ItemId
                        INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                        LEFT JOIN SupplyOrderItem SOI ON SOI.SupplyOrderItemId=GI.SupplyOrderItemId
                        LEFT JOIN SupplyOrder SO ON SO.SupplyOrderId=SOI.SupplyOrderId 
                        WHERE  GRNDate >= @from AND GRNDate <= @to
                        AND I.ItemGroupId=ISNULL(NULLIF(@id, 0),I.ItemGroupId)
                        AND isnull(I.ItemName,'')  LIKE '%'+@material+'%'
                        AND isnull(I.PartNo,'')  LIKE '%'+@partno+'%'
                        AND isnull(S.SupplierName,'')  LIKE '%'+@supplier+'%'
	                    ORDER BY G.GRNId";

                return connection.Query<GRNRegister>(sql, new { from = from, to = to, id = id, material = material, partno = partno, supplier = supplier });
            }
        }

    }
}
