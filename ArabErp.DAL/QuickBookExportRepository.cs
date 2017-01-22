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
        public List<PurchaseBillPostingList> GetPurchaseBillsForPosting(string Status)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select P.PurchaseBillId, P.PurchaseBillRefNo, P.PurchaseBillDate, S.SupplierName,  
                P.PurchaseBillNoDate, P.PurchaseBillAmount, C.CurrencyName, 0 IsSelected,
                PostStatus = case when isnull(P.PostStatus, 0) = 0 then 'No' else 'Yes' end
                from PurchaseBill P inner join Supplier S on P.SupplierId = S.SupplierId
                inner join Currency C on C.CurrencyId = P.CurrencyId
                where isnull(P.PostStatus, 0) = " + Status;

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
        public List<PurchaseBillPostingTransaction> GetPurchaseBillDetailsForExportExcel(PendingPurchaseBillsForPosting model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string Id = "0";
                foreach (var item in model.PurchaseBillPostingList)
                {
                    if(item.IsSelected == 1)
                    {
                        Id += ", " + item.PurchaseBillId.ToString();
                    }
                }
                string sql = @"select convert(varchar(50), PurchaseBillDate, 105)[Date],
                PurchaseBillNoDate Num, S.SupplierName [Name], 'credit' Terms,
				convert(varchar(50), PurchaseBillDueDate, 105)[DueDate],
				case when
				(select top 1 I.ItemCategoryId from PurchaseBillItem PB 
				inner join GRNItem G on PB.GRNItemId = G.GRNItemId
				inner join Item I on G.ItemId = I.ItemId
				where PB.PurchaseBillId = P.PurchaseBillId) = 2 then 'Inventory Asset' else 'Inventory Consumable' end Account, 
				cast(P.PurchaseBillAmount as varchar(25)) Amount,
				Remarks Memo
                from PurchaseBill P 
                inner join Supplier S on P.SupplierId = S.SupplierId
                where P.PurchaseBillId in ("+ Id +")";

                string query = "update PurchaseBill set PostStatus = 1 where PurchaseBillId in (" + Id + ")";
                connection.Query(query);

                return connection.Query<PurchaseBillPostingTransaction>(sql).ToList();
            }
        }
        public List<SalesInvoicePostingList> GetSalesInvoicePostingList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select SalesInvoiceId, SalesInvoiceRefNo, SalesInvoiceDate, C.CustomerName,
                S.PaymentTerms, S.TotalAmount, CR.CurrencyName
                from SalesInvoice S
                inner join SaleOrder SO on S.SaleOrderId = SO.SaleOrderId
                inner join Customer C on SO.CustomerId = C.CustomerId
                inner join Currency CR on CR.CurrencyId = SO.CurrencyId
                where isnull(S.PostStatus, 0) = 0";

                return connection.Query<SalesInvoicePostingList>(sql).ToList();
            }
        }
    }
}
