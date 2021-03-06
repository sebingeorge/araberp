﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class UserRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertUser(User user)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = "INSERT INTO [dbo].[User](UserName, UserEmail, UserPassword, UserSalt,DesignationId,Amount)";
                    sql += " VALUES(@UserName, @UserEmail, @UserPassword, @UserSalt,@DesignationId,@Amount);";
                    sql += " SELECT CAST(SCOPE_IDENTITY() as int);";

                    var id = connection.Query<int>(sql, user).Single();

                    foreach (var item in user.Module)
                    {
                        if (item.isPermission == 1)
                        {
                            item.UserId = id;
                            sql = "insert into ModuleVsUser(ModuleId, UserId) values(@ModuleId, @UserId);";
                            connection.Query(sql, item);
                        }
                    }
                    foreach (var item in user.ERPAlerts)
                    {
                        if (item.HasPermission == 1)
                        {
                            item.UserId = id;
                            sql = "insert into ERPAlertsVsUser(AlertId, UserId) values(@AlertId, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    foreach (var item in user.ERPGraphs)
                    {
                        if (item.HasPermission == 1)
                        {
                            item.UserId = id;
                            sql = "insert into ERPGraphsVsUser(GraphId, UserId) values(@GraphId, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    foreach (var item in user.Companies)
                    {
                        if (item.isPermission == 1)
                        {
                            item.UserId = id;
                            sql = "insert into CompanyVsUser(cmpCode, UserId) values(@cmpCode, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    InsertFormPermission(user.Forms, id);
                    InsertLoginHistory(dataConnection, user.CreatedBy, "Create", "Unit", id.ToString(), "0");
                    return id;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        private int InsertFormPermission(List<FormsVsUser> Forms, int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM UserVsForms WHERE UserId = @UserId";
                    connection.Execute(query, new { UserId = UserId }, txn);
                    query = @"INSERT INTO UserVsForms (UserId, FormId)
                              VALUES (@UserId, @FormId)";
                    foreach (var item in Forms)
                    {
                        connection.Execute(query, new { UserId = UserId, FormId = item.FormId }, txn);
                    }
                    txn.Commit();
                    return 1;
                }
                catch
                {
                    txn.Rollback();
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
                    string sql = "Update [User] set UserEmail = @UserEmail,DesignationId=@DesignationId,Amount=@Amount";
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
                        if (item.isPermission == 1)
                        {
                            item.UserId = user.UserId ?? 0;
                            sql = "insert into ModuleVsUser(ModuleId, UserId) values(@ModuleId, @UserId);";
                            connection.Query(sql, item);
                        }
                    }
                    sql = "delete from ERPAlertsVsUser where UserId = " + user.UserId.ToString();
                    connection.Query(sql);
                    foreach (var item in user.ERPAlerts)
                    {
                        if (item.HasPermission == 1)
                        {
                            item.UserId = user.UserId ?? 0;
                            sql = "insert into ERPAlertsVsUser(AlertId, UserId) values(@AlertId, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    sql = "delete from ERPGraphsVsUser where UserId = " + user.UserId.ToString();
                    connection.Query(sql);
                    foreach (var item in user.ERPGraphs)
                    {
                        if (item.HasPermission == 1)
                        {
                            item.UserId = user.UserId ?? 0;
                            sql = "insert into ERPGraphsVsUser(GraphId, UserId) values(@GraphId, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    sql = "delete from CompanyVsUser where UserId = " + user.UserId.ToString();
                    connection.Query(sql);
                    foreach (var item in user.Companies)
                    {
                        if (item.isPermission == 1)
                        {
                            item.UserId = user.UserId ?? 0;
                            sql = "insert into CompanyVsUser(cmpCode, UserId) values(@cmpCode, @UserId)";
                            connection.Query(sql, item);
                        }
                    }
                    InsertFormPermission(user.Forms, user.UserId ?? 0);
                }
                catch
                {
                    return 0;
                }
            }
            InsertLoginHistory(dataConnection, user.CreatedBy, "Update", "Unit", user.UserId.ToString(), "0");
            return user.UserId ?? 0;
        }
        public User GetUserByUserNameAndPassword(string username, string password)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select * from [User] where UserName='" + username + "' and UserPassword='" + password + "'";
                var user = new User();
                try
                {
                    user = connection.Query<User>(sql).Single();
                }
                catch
                {

                }
                return user;
            }
        }
        public bool IsValidUser(int UserId, string Username, string ControllName, string ActionName, int OrganizationId, string SessionID)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                if (ActionName != "LoadQuickView" && ControllName != "Home")
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



        public User GetUserInfo(int? Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT [UserId]
                ,[UserName]
                ,[UserEmail]
                ,[UserPassword]
                ,[UserSalt]
                ,[UserRole]
                ,[Signature]
                ,DesignationId
                ,Amount
                FROM[dbo].[User] U Where U.UserId=" + Id;
                return connection.Query<User>(sql).FirstOrDefault();
            }
        }

        public IEnumerable<User> GetUserAndModuleInfoList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT [UserId]
                ,[UserName]
                ,[UserEmail]
                ,[UserPassword]
                ,[UserSalt]
                ,[UserRole]
                , ModuleNames = STUFF((SELECT ',' + M.ModuleName AS[text()]
                FROM Module M join ModuleVsUser MU

                on M.ModuleId = MU.ModuleId
                WHERE
                MU.UserId = U.UserId
                FOR XML PATH('')
                ), 1, 1,'')
                FROM[dbo].[User] U ";
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
        public IEnumerable<ERPAlerts> GetAlerts(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"select A.AlertId, A.AlertName, HasPermission = case when EU.RowId is null then 0 else 1 end,
                " + UserId.ToString() + @" UserId 
                from ERPAlerts A
                left join ERPAlertsVsUser EU on A.AlertId = EU.AlertId and EU.UserId = " + UserId.ToString();
                    return connection.Query<ERPAlerts>(query);
                }
                catch (Exception)
                {
                    return new List<ERPAlerts>();
                }
            }
        }
        public IEnumerable<ERPGraphs> GetGraphs(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select A.GraphId, A.GraphName, HasPermission = case when EU.RowId is null then 0 else 1 end,
                " + UserId.ToString() + @" UserId 
                from ERPGraphs A
                left join ERPGraphsVsUser EU on A.GraphId = EU.GraphId and EU.UserId = " + UserId.ToString();

                return connection.Query<ERPGraphs>(sql);
            }
        }

        public IEnumerable<CompanyVsUser> GetCompanyPermissions(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select mstAccCompany.cmpCode,mstAccCompany.cmpName,isPermission  = case when CU.CompanyVsUserId is null then 0 else 1 end, 
                             " + UserId.ToString() + @" UserId
                                from mstAccCompany
                                left join CompanyVsUser CU on mstAccCompany.cmpCode=CU.cmpCode and CU.UserId = " + UserId.ToString();
                return connection.Query<CompanyVsUser>(sql);
            }
        }

        public int UpdateSignature(string fileName, int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"UPDATE [User] SET [Signature] = @fileName WHERE UserId = @id";
                return connection.Execute(query, new { fileName = fileName, id = id });
            }
        }

        public IEnumerable<FormPermission> GetFormPermissions(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT F.FormId FROM Forms F
                                INNER JOIN UserVsForms UF ON F.FormId = UF.FormId 
                                WHERE UserId = @UserId";
                return connection.Query<FormPermission>(query, new { UserId = UserId });
            }
        }

        public IEnumerable<FormsVsUser> GetFormsVsUser(int UserId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                F.FormId,
	                                F.FormName,
	                                F.ModuleId,
	                                M.ModuleName,
	                                CASE WHEN UserId IS NULL THEN 0 ELSE 1 END AS hasPermission
                                FROM Forms F
	                                LEFT JOIN UserVsForms UF ON F.FormId = UF.FormId AND UserId = @UserId
	                                INNER JOIN Module M ON F.ModuleId = M.ModuleId";
                return connection.Query<FormsVsUser>(query, new { UserId = UserId });
            }
        }
    }
}
