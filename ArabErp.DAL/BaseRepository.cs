using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
   public class BaseRepository : IDisposable
    {
        protected SqlConnection connection;
        public BaseRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
