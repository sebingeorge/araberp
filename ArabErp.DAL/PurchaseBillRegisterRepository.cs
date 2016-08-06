using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class PurchaseBillRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<PurchaseBillRegister> GetPurchaseBillRegisterData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select PurchaseBillRefNo,PurchaseBillDate,PurchaseBillNoDate,S.SupplierName,ItemName,ItemRefNo,G.Quantity,PI.Rate,PI.Amount 
                               from PurchaseBill P,PurchaseBillItem PI,Item I,Supplier S,GRNItem G
                               where P.SupplierId=S.SupplierId AND P.PurchaseBillId=PI.PurchaseBillId AND PI.GRNItemId=G.GRNItemId AND G.ItemId =I.ItemId 
                               and P.isActive=1 AND P.PurchaseBillDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                               AND P.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                               ORDER BY PurchaseBillDate";



                return connection.Query<PurchaseBillRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }
    }
}
