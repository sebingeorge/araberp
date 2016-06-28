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
        public User GetUserByUserNameAndPassword(string username, string password)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<User>("select * from [User] where UserName='"+ username +"' and UserPassword='"+ password +"'").Single();
            }
        }
        public bool IsValidUser(int UserId, string Username, string ControllName, string ActionName)
        {
            return true;
        }
        public void InsertLoginHistory(User user, string sessionId, string ipAddress)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "Insert into LoginHistory(UserId, LogIn, SessionID, IpAddress)";
                sql += " values('"+ user.UserId.ToString() +"', GetDate(), '"+ sessionId +"','"+ ipAddress +"')";
                connection.Query(sql);
            }
        }
        public void UpdateLoginHistory(string sessionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "update LoginHistory set LogoutTime = Getdate() where SessionID = " + sessionId;
            }
        }
    }
}
