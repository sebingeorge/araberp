using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration.Assemblies;

#if SQLITE && (NET40 || NET45)
using SqliteConnection = System.Data.SQLite.SQLiteConnection;
#endif


using System.Data.SqlClient;

using Dapper;

using System.Data;
using ArabErp.Domain;
using ArabErp.DAL;

#if ENTITY_FRAMEWORK
using System.Data.Entity.Spatial;
using Microsoft.SqlServer.Types;
#endif
#if SQL_CE
using System.Data.SqlServerCe;
#endif

#if POSTGRESQL
using Npgsql;
#endif
#if SQLITE
#if NET40 || NET45
using System.Data.SQLite;
#else
using Microsoft.Data.Sqlite;
#endif
#endif
#if ASYNC
using System.Threading.Tasks;
#endif


namespace ArabErp
{


    public class JobCardRepository : IDisposable
    {
        public static string ConnectionString => System.Configuration.ConfigurationManager.
    ConnectionStrings["arab"].ConnectionString;

       
        public static SqlConnection GetOpenConnection()
        {
            var cs = ConnectionString;
           
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }

        public static SqlConnection GetClosedConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            if (conn.State != ConnectionState.Closed) throw new InvalidOperationException("should be closed!");
            return conn;
        }

        private SqlConnection _connection;

        private SqlConnection connection => _connection ?? (_connection = ConnectionManager.connection);


 
        public string GetJobNumber(int id)
        {

            string jobnumber="";
               
                var jobs = connection.Query<JobCard>("select * from JobCard where JobCardId=" + id );

            if (jobs.Count() > 0)
            {
                jobnumber= jobs.First().JobCardNo;
            }
           
                return jobnumber;
            
        }


        public List<Bay> getAllBays()
        {
            List<Bay> bays =connection.Query<Bay>("select BayId,BayName from Bay").ToList();
            return bays;
        }

        public string SaveJobCard(JobCard jc)
        {
            
            connection.Execute("Insert into JobCard (JobCardNo) Values ('" + jc.JobCardNo + "')");
            var rtn = "Saved " + jc.JobCardNo;
            return rtn;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}