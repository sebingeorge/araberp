﻿
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
                                BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                AND S.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) 
                                and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                                GROUP BY SI.SupplyOrderId,SupplyOrderNo,SupplyOrderDate,SUP.SupplierName,ItemName,OrderedQty,UnitName,SI.Rate,SI.Amount
                                ORDER BY SupplyOrderDate";



                return connection.Query<SupplyOrderRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }


    }
}
