﻿using System;
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

                    foreach(var item in user.Module)
                    {
                        if(item.isPermission == 1)
                        {
                            item.UserId = id;
                            sql = "insert into ModuleVsUser(ModuleId, UserId) values(@ModuleId, @UserId);";
                            connection.Query(sql, item);
                        }                        
                    }
                    return id;
                }
                catch(Exception ex)
                {
                    return 0;
                }                
            }
        }
        public int UpdateUser(User user)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = "Update [User] set UserEmail = @UserEmail";
                    if (user.UserPassword != null && user.UserPassword != "")
                    {
                        sql += ", UserPassword = @UserPassword, @UserSalt = UserSalt";
                    }
                    sql += " where UserId = @UserId;";
                    connection.Query(sql, user);

                    sql = "delete from ModuleVsUser where UserId = " + user.UserId.ToString();
                    connection.Query(sql);
                    foreach (var item in user.Module)
                    {
                        if(item.isPermission == 1)
                        {
                            item.UserId = user.UserId ?? 0;
                            sql = "insert into ModuleVsUser(ModuleId, UserId) values(@ModuleId, @UserId);";
                            connection.Query(sql, item);
                        }                        
                    }
                }
                catch
                {
                    return 0;
                }
            }
            return user.UserId ?? 0;
        }
        public User GetUserByUserNameAndPassword(string username, string password)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<User>("select * from [User] where UserName='"+ username +"' and UserPassword='"+ password +"'").Single();
            }
        }
        public bool IsValidUser(int UserId, string Username, string ControllName, string ActionName, int OrganizationId, string SessionID)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                if(ActionName != "LoadQuickView" && ControllName != "Home")
                {
                    string sql = @"insert into TransactionHistory(UserId, TransTime, Mode, Form, OrganizationId, SessionID)
                               values('" + UserId.ToString() + "', GetDate(), '" + ActionName + "', '" + ControllName + "', '" + OrganizationId.ToString() + "','" + SessionID + "')";
                    connection.Query(sql);
                }               
            }
            return true;
        }
        public void InsertLoginHistory(User user, string sessionId, string ipAddress, string OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "Insert into LoginHistory(UserId, LogIn, SessionID, IpAddress, OrganizationId)";
                sql += " values('" + user.UserId.ToString() + "', GetDate(), '" + sessionId + "','" + ipAddress + "','" + OrganizationId + "')";
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
        public IEnumerable<ModuleVsUser> GetModules(int? UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "";
                if (UserId != null)
                {
                    sql = @"select M.ModuleId, M.ModuleName, isPermission =  case when RowId is null then 0 else 1 end 
                        from Module M left join ModuleVsUser MU on M.ModuleId = MU.ModuleId and MU.UserId = " + UserId.ToString();
                }
                else
                {
                    sql = @"select M.ModuleId, M.ModuleName,0 as isPermission from Module M";
                }
                return connection.Query<ModuleVsUser>(sql);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select * from [User]";
                return connection.Query<User>(sql);
            }
        }
        public IEnumerable<ModuleVsUser> GetModulePermissions(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select M.* from Module M inner join ModuleVsUser MU on M.ModuleId = MU.ModuleId where MU.UserId = " + UserId.ToString();
                return connection.Query<ModuleVsUser>(sql);
            }
        }
    }
}
