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
 