using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
 public class WorkshopRequestRegisterRepository : BaseRepository
    {

        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<WorkshopRequestRegister> GetWorkshopRegisterData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//              
                string qry = @"select WorkShopRequestRefNo,WorkShopRequestDate,ItemRefNo,I.PartNo,ItemName,Quantity,UnitName from WorkShopRequestItem WI 
                                inner join WorkShopRequest W on W.WorkShopRequestId=WI.WorkShopRequestId
                                inner join Item I ON I.ItemId=WI.ItemId 
                                inner join Unit U ON U.UnitId=I.ItemUnitId
                                WHERE W.isActive=1  AND W.WorkShopRequestDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())  and I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) AND  W.WorkShopRequestId = ISNULL(NULLIF(@id, 0), W.WorkShopRequestId)
                                order by WorkShopRequestDate";
                return connection.Query<WorkshopRequestRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }
    }
}
