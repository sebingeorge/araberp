﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockMovementRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ClosingStock> GetStockMovementData(DateTime? from, DateTime? to, int stkid, int itmcatid, int itmid, int OrganizationId, int ItemGroupId = 0, int ItemSubGroupId = 0,string PartNo="")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select I.ItemId,I.ItemRefNo,I.ItemName,U.UnitName,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='IN' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) INQTY,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='OUT' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid )*(-1) OUTQTY ,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE  StockType='OpeningStock' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) OPENINGSTOCK
                            from StockUpdate S  inner join Item I on I.ItemId=S.ItemId INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                            where S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid
	                        AND S.StockPointId=ISNULL(NULLIF(@stkid,0),S.StockPointId)
							AND I.ItemGroupId=ISNULL(NULLIF(@ItemGroupId,0),I.ItemGroupId)
							AND I.ItemSubGroupId=ISNULL(NULLIF(@ItemSubGroupId, 0), I.ItemSubGroupId)
                            and isnull(I.PartNo,'') like '%'+@PartNo+'%' 
							GROUP BY  I.ItemId,I.ItemRefNo,I.ItemName,ItemCategoryId,U.UnitName";


                return connection.Query<ClosingStock>(qry, new { itmcatid = itmcatid, stkid = stkid, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to, ItemGroupId = ItemGroupId, ItemSubGroupId = ItemSubGroupId, PartNo = PartNo }).ToList();

               
                
            }
        }
    }
}
