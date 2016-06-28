using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class LoginHistoryRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<LoginHistory> GetLoginHistory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select L.LogIn LoginDate, U.UserName, convert(varchar(8), L.LogIn, 108) LoginTime,";
                sql += " convert(varchar(8), L.LogoutTime, 108) LogoutTime";
                sql += " from LoginHistory L inner join [User] U on L.UserId = U.UserId";
                sql += " order by L.LogIn,U.UserName";

                return connection.Query<LoginHistory>(sql);
            }
        }
    }
}
