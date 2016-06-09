using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class UserRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertUser(User user)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = "INSERT INTO [dbo].[User](UserName, UserEmail, UserPassword, UserSalt)";
                    sql += " VALUES(@UserName, @UserEmail, @UserPassword, @UserSalt);";
                    sql += " SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = connection.Query<int>(sql, user).Single();
                    return id;
                }
                catch(Exception ex)
                {
                    return 0;
                }                
            }
        }
    }
}
