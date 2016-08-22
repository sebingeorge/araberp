using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Configuration;
using System.Data;

namespace ArabErp.DAL
{
   public class BaseRepository
    {

        public static string GetConnectionString(string connStrName)
        {
            return ConfigurationManager.ConnectionStrings[connStrName].ConnectionString;
        }
        public static IDbConnection OpenConnection(string connStrName)
        {
            IDbConnection connection = new SqlConnection(connStrName);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }
       /// <summary>
       /// To check the Field Duplicates
       /// </summary>
       /// <param name="connStrName">connection string to database</param>
       /// <param name="TableName">Name of table in Database</param>
       /// <param name="FieldName">Name of Column to be checked</param>
       /// <param name="Value">Value to be checked for duplication</param>
       /// <param name="UniqueIDField">Field Name of Primary Key Column</param>
       /// <param name="Id">Optional Parameter Give Primary key value if in Modify mode</param>
       /// <returns>Returns True if exists else false</returns>
       public bool IsFieldExists(string connStrName, string TableName, string FieldName, string Value, string UniqueIDField, int? Id)
        {
            using (IDbConnection connection = OpenConnection(connStrName))
           {
               string sql = String.Format("select count(*) from {0} where {1} = '{2}'", TableName, FieldName, Value);
                if(Id != null && Id != 0)
                {
                    sql += String.Format(" and {0} <> {1}",UniqueIDField, (Id??0));
                }

                int count = connection.Query<int>(sql).Single();
                if (count > 0)
                    return true;
                else
                    return false;
           }
        }
       /// <summary>
       /// To Insert Transaction History of transactions
       /// </summary>
       /// <param name="connStrName">Connection String</param>
       /// <param name="UserId">User ID of Logined User</param>
       /// <param name="Mode">Mode of transaction</param>
       /// <param name="Form">Controller Name</param>
       /// <param name="TransactionCode">Transaction ID of current transaction</param>
       /// <param name="OrganizationId">Current Company ID</param>
       public void InsertLoginHistory(string connStrName,string UserId, string Mode, string Form, string TransactionCode, string OrganizationId)
       {
           using (IDbConnection connection = OpenConnection(connStrName))
           {
               string sql = @"INSERT INTO [dbo].[TransactionHistory]
                           ([UserId]
                           ,[TransTime]
                           ,[Mode]
                           ,[Form]
                           ,[FormTransCode]
                           ,[IPAddress]
                           ,[OrganizationId]
                           ,[SessionID])
                     VALUES
                           ('" + UserId + "','" + DateTime.Now.ToString() +"','" + Mode +"','" + Form +"','" + TransactionCode +"','','','')";

               connection.Query(sql);
           }
       }

       
        //protected SqlConnection connection;
        //public BaseRepository()
        //{
        //    if (connection == null)
        //    {
        //        connection = ConnectionManager.connection;
        //    }
        //}

        //public void Dispose()
        //{
        //    connection.Dispose();
        //}
    }
}
 