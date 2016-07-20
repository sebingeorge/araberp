using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class QuotationApprovalRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<QuotationApprovalAmountSettings> GetApprovalAmountSettings()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select ApprovalCode, AmountFrom, AmountTo from QuotationApprovalAmountSettings";

                return connection.Query<QuotationApprovalAmountSettings>(sql);
            }
        }
        public IEnumerable<QuotationApprovalSettings> GetApprovalSettings()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select U.UserId, U.UserName, Q.Approval1, Q.Approval2, Q.Approval3,Q.Cancel from 
                            QuotationApprovalSettings Q
                            right join [User] U on Q.UserId = U.UserId";

                return connection.Query<QuotationApprovalSettings>(sql);
            }
        }
        public void UpdateSettings(QuotationApprovalViewModel model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                foreach(var item in model.QuotationApprovalAmountSettings)
                {
                    sql += @"update QuotationApprovalAmountSettings set AmountFrom = '" + item.AmountFrom.ToString("#0.#0") + "', AmountTo = '" + item.AmountTo.ToString("#0.#0") + "' where ApprovalCode = " + item.ApprovalCode + ";";
                }

                connection.Query(sql);

                sql = string.Empty;

                sql = "delete from QuotationApprovalSettings;";
                connection.Query(sql);
                foreach (var item in model.QuotationApprovalSettings)
                {
                    sql = @"insert into QuotationApprovalSettings(UserId, Approval1, Approval2, Approval3,Cancel) values
                           (@UserId, @Approval1, @Approval2, @Approval3,@Cancel);";
                    connection.Query(sql, item);
                }
            }
        }
        public QuotationApprovalAmountSettings GetUserApprovalAmountSettings(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"declare @approveType as int;
                            select @approveType = case when Approval3 = 1 then 3 when Approval2 = 1 then 2 when Approval1 = 1 then 1 else 0 end  
                            from QuotationApprovalSettings where UserId = " + UserId.ToString() + @"
                            if Exists(select ApprovalCode, AmountFrom, AmountTo from QuotationApprovalAmountSettings where ApprovalCode = @approveType)
							begin
                            select ApprovalCode, AmountFrom, AmountTo from QuotationApprovalAmountSettings where ApprovalCode = @approveType
							end
							else
							begin
								select 0 ApprovalCode, 0 AmountFrom, 0 AmountTo
							end";

                return connection.Query<QuotationApprovalAmountSettings>(sql).Single();
            }
        }
    }
}
