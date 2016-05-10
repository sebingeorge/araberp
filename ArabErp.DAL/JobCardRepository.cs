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
using DapperExtensions;

namespace ArabErp
{


    public class JobCardRepository : IDisposable
    {
        public static string ConnectionString = System.Configuration.ConfigurationManager.
    ConnectionStrings["arab"].ConnectionString;

       
        //public static SqlConnection GetOpenConnection()
        //{
        //    var cs = ConnectionString;
           
        //    var connection = new SqlConnection(cs);
        //    connection.Open();
        //    return connection;
        //}

        //public static SqlConnection GetClosedConnection()
        //{
        //    var conn = new SqlConnection(ConnectionString);
        //    if (conn.State != ConnectionState.Closed) throw new InvalidOperationException("should be closed!");
        //    return conn;
        //}

        private SqlConnection connection;

     //   private SqlConnection connection => _connection ?? (_connection = ConnectionManager.connection);

        public JobCardRepository ()
        {
            connection = ConnectionManager.connection;
        }
 
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

        public void SaveItem(Item objItem)
        {
           
            connection.Insert<Item>(objItem);
        }

        public void UpdateItem(Item objItem)
        {

            connection.Update<Item>(objItem);
        }

        public void DeleteItem(Item objItem)
        {

            connection.Delete<Item>(objItem);
        }
        public Item GetItem(int ItemId)
        {

            Item objItem= connection.Get<Item>(ItemId);
            return objItem;
        }

        public IEnumerable<Item> GetAllItems()
        {
            IEnumerable<Item> list = connection.GetList<Item>();
            return list;
        }

        public IEnumerable<Item> GetGroup1Items()
        {

            var predicate = Predicates.Field<Item>(i => i.ItemGroupId, Operator.Eq, 4);
            IEnumerable<Item> list = connection.GetList<Item>(predicate);
            return list;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}