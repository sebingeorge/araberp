using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class TransactionHistoryRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertTransactionHistory(TransactionHistory objTransactionHistory)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into TransactionHistory(UserId,TransTime,Mode,Form,FormTransCode,IPAddress,OrganizationId) Values (@UserId,@TransTime,@Mode,@Form,@FormTransCode,@IPAddress,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objTransactionHistory).Single();
                return id;
            }
        }


        public TransactionHistory GetTransactionHistory(int TransactionHistoryId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select U.UserName, Form, Mode, TransTime, FormTransCode
                                from TransactionHistory T
                                inner join [User] U on T.UserId = U.UserId
                                where FormTransCode is not null";

                var objTransactionHistory = connection.Query<TransactionHistory>(sql, new
                {
                    TransactionHistoryId = TransactionHistoryId
                }).First<TransactionHistory>();

                return objTransactionHistory;
            }
        }

        public List<TransactionHistory> GetTransactionHistorys(string From, string To)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select U.UserName, Form, Mode, TransTime, FormTransCode
                                from TransactionHistory T
                                inner join [User] U on T.UserId = U.UserId
                                where FormTransCode is not null";
                if(From != string.Empty && To != string.Empty)
                {
                    sql += " and TransTime >= '"+ From +"' and TransTime <= '"+ To +"'";
                }
                var objTransactionHistorys = connection.Query<TransactionHistory>(sql).ToList<TransactionHistory>();

                return objTransactionHistorys;
            }
        }



        public int DeleteTransactionHistory(Unit objTransactionHistory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete TransactionHistory  OUTPUT DELETED.TransactionHistoryId WHERE TransactionHistoryId=@TransactionHistoryId";


                var id = connection.Execute(sql, objTransactionHistory);
                return id;
            }
        }


    }
}