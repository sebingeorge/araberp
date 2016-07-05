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

       public bool IsFieldExists(string connStrName, string TableName, string FieldName, string Value, string UniqueIDField, int? Id)
        {
            using (IDbConnection connection = OpenConnection(connStrName))
           {
               string sql = String.Format("select count(*) from {0} where {1} = {2}", TableName, FieldName, Value);
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
 