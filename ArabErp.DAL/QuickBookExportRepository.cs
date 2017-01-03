using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Web;
using System.Collections;

namespace ArabErp.DAL
{
    public class QuickBookExportRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public List<PurchaseBillPostingList> GetPurchaseBillsForPosting()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select P.PurchaseBillId, P.PurchaseBillRefNo, P.PurchaseBillDate, S.SupplierName,  
                P.PurchaseBillNoDate, P.PurchaseBillAmount, C.CurrencyName, 0 IsSelected
                from PurchaseBill P inner join Supplier S on P.SupplierId = S.SupplierId
                inner join Currency C on C.CurrencyId = P.CurrencyId
                where isnull(P.PostStatus, 0) = 0";

                return connection.Query<PurchaseBillPostingList>(sql).ToList();
            }
        }
        public List<PurchaseBillPostingTransaction> GetPurchaseBillDetails(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select cast(PurchaseBillId as varchar(10)) Trans, 'Bill' [Type], convert(varchar(50), PurchaseBillDate, 105)[Date],
                PurchaseBillRefNo Num, S.SupplierName [Name], '' Memo,
                'Account Payable' Account, '' Class, '' Debit, 
                cast(P.PurchaseBillAmount as varchar(25)) Credit
                from PurchaseBill P 
                inner join Supplier S on P.SupplierId = S.SupplierId
                where PurchaseBillId = " + Id.ToString() + @"
                union all
                select '' Trans, '' [Type], '' [Date], '' Num,
                S.SupplierName [Name], I.ItemName Memo, 'Inventory Asset' Account,
                'Trading' Class, cast(PI.TotalAmount as varchar(25)) Debit, '' Credit
                from PurchaseBillItem PI
                inner join PurchaseBill P on PI.PurchaseBillId = P.PurchaseBillId
                inner join Supplier S on P.SupplierId = S.SupplierId
                inner join GRNItem G on PI.GRNItemId = G.GRNItemId
                inner join Item I on G.ItemId = I.ItemId	
                where P.PurchaseBillId = " + Id.ToString() + @"";

                return connection.Query<PurchaseBillPostingTransaction>(sql).ToList();
            }
        }
    }
}
