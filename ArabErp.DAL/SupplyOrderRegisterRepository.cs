using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class SupplyOrderRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<SupplyOrderRegister> GetSupplyOrderRegisterData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount,
                                (select sum(SD.Amount) from SupplyOrderItem SD where  SD.SupplyOrderId=SI.SupplyOrderId 
                                GROUP BY  SD.SupplyOrderId)TotalAmount
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                WHERE S.isActive=1 AND S.SupplyOrderDate 
                                BETWEEN  @from  AND @to 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }
        public IEnumerable<SupplyOrderRegister> GetSupplyOrderSummaryData(DateTime? from, DateTime? to, int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName, sum(SI.Amount)TotalAmount
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                WHERE S.isActive=1 AND S.SupplyOrderDate 
                                BETWEEN @from AND @to
                                AND S.OrganizationId=@OrganizationId 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }

        public IEnumerable<SupplyOrderRegister> GetPendingSupplyOrderRegister(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,
                                SI.OrderedQty,UnitName,0 SettledQty,isnull(GI.Quantity,0)ReceviedQty,
                                (SI.OrderedQty-isnull(GI.Quantity,0))BalanceQty
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId
                                WHERE S.isActive=1 and (SI.OrderedQty-isnull(GI.Quantity,0))>0 AND S.SupplyOrderDate 
                                BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }

        public IEnumerable<SupplyOrderRegister> GetSOVarianceData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,
                                SI.OrderedQty,UnitName,isnull(GI.Quantity,0)ReceviedQty,
                                (SI.OrderedQty-isnull(GI.Quantity,0))BalanceQty,
                                CASE WHEN (SI.OrderedQty-isnull(GI.Quantity,0))>0 THEN 'PENDING' ELSE 'COMPLETED' END AS STATUS
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId
                                WHERE S.isActive=1 AND S.SupplyOrderDate 
                                BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }


//        public SupplyOrderRegister GetSupplyOrderRegisterHD(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
//        {
//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {

//                //              
//                string sql = @" select SUP.SupplierName,ItemName,O.OrganizationName from SupplyOrder S
//                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
//                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
//                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
//                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
//								inner join Organization O ON O.OrganizationId=S.OrganizationId";

//                var objSupplyOrderRegisterId = connection.Query<SupplyOrderRegister>(sql, new
//                {
//                    from=from,
//                    to=to,
//                    id=id,
//                    itmid=itmid,
//                    OrganizationId = OrganizationId
//                }).First<SupplyOrderRegister>();

//                return objSupplyOrderRegisterId;
//            }
//        }


        public List<SupplyOrderRegister> GetSupplyOrderRegisterDT(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string sql = @"SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount,
//                                (select sum(SD.Amount) from SupplyOrderItem SD where  SD.SupplyOrderId=SI.SupplyOrderId 
//                                GROUP BY  SD.SupplyOrderId)TotalAmount
//                                FROM SupplyOrder S
//                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
//                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
//                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
//                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
//                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
//                                WHERE S.isActive=1 AND S.SupplyOrderDate 
//                                BETWEEN  @from  AND @to AND S.OrganizationId=@OrganizationId 
//                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount
//                                ORDER BY SupplyOrderDate";
                string sql = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount,
                                (select sum(SD.Amount) from SupplyOrderItem SD where  SD.SupplyOrderId=SI.SupplyOrderId 
                                GROUP BY  SD.SupplyOrderId)TotalAmount
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                WHERE S.isActive=1 AND S.SupplyOrderDate 
                                BETWEEN  @from  AND @to 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount
                                ORDER BY SupplyOrderDate";
                return connection.Query<SupplyOrderRegister>(sql, new { OrganizationId = OrganizationId, from = from, to = to, id, itmid }).ToList();


            }
        }




        public SupplyOrderRegister GetSupplyOrderRegisterHD(DateTime? from, DateTime? to, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"	select O.Image1,O.OrganizationName from Organization O where OrganizationId=@OrganizationId";

                var objSupplyOrderRegisterId = connection.Query<SupplyOrderRegister>(qry, new
                {
                   
                    OrganizationId = OrganizationId,
                     from = from, 
                     to = to
                }).First<SupplyOrderRegister>();

                return objSupplyOrderRegisterId;
            }
        }

        public IEnumerable<SupplyOrderRegister> SupplyOrderSummaryDT(DateTime? from, DateTime? to, string id, int OrganizationId, int SupId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                        
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName, sum(SI.Amount)TotalAmount
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                WHERE S.isActive=1 AND S.SupplyOrderDate 
                                BETWEEN @from AND @to
                                AND S.OrganizationId=@OrganizationId 
                                and S.SupplierId=ISNULL(NULLIF(@SupId, 0), S.SupplierId) 
                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, OrganizationId = OrganizationId, from = from, to = to, SupId = SupId }).ToList();
            }
        }
        public SupplyOrderRegister SupplyOrderSummaryHD(DateTime? from, DateTime? to, int OrganizationId,string id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"	select O.Image1,O.OrganizationName from Organization O where OrganizationId=@OrganizationId";

                var objSupplyOrderRegisterId = connection.Query<SupplyOrderRegister>(qry, new
                {

                    OrganizationId = OrganizationId,
                    from = from,
                    to = to,
                    id = id
                }).First<SupplyOrderRegister>();

                return objSupplyOrderRegisterId;
            }
        }
        public SupplyOrderRegister Supplier(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"	SELECT distinct  S.SupplierId ,SupplierName  FROM Supplier S 
                                INNER JOIN SupplyOrder SO ON SO.SupplierId=S.SupplierId 
                                WHERE ISNULL(S.isActive, 1) = 1 and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId)  ";

                var objSupplyOrderRegisterId = connection.Query<SupplyOrderRegister>(qry, new
                {

                    Id = Id
                }).First<SupplyOrderRegister>();

                return objSupplyOrderRegisterId;
            }
        }

        public IEnumerable<SupplyOrderRegister> PendingSupplyOrderRegisterDT(DateTime? from, DateTime? to, int itmid = 0, string itmName = "", int SupId = 0, string SupName = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,
                                SI.OrderedQty,UnitName,0 SettledQty,isnull(GI.Quantity,0)ReceviedQty,
                                (SI.OrderedQty-isnull(GI.Quantity,0))BalanceQty
                                FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId 
                                INNER JOIN Supplier SUP ON SUP.SupplierId=S.SupplierId
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                                INNER JOIN Item I ON  I.ItemId=PI.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId
                                WHERE S.isActive=1 and (SI.OrderedQty-isnull(GI.Quantity,0))>0 AND S.SupplyOrderDate 
                                BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { itmName = itmName, itmid = itmid, SupId = SupId, from = from, to = to, SupName = SupName }).ToList();
            }
        }


    }
}
