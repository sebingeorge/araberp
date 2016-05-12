using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;

namespace ArabErp
{


    public class JobCardRepository : IDisposable
    {
        
       
  
        private SqlConnection connection;

     //   private SqlConnection connection => _connection ?? (_connection = ConnectionManager.connection);

        public JobCardRepository ()
        {

            if (connection==null)
            {
                connection = ConnectionManager.connection;
            }
                
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
        public IEnumerable<Item> GetGroup1andSubGroup1Items()
        {
            var pg = new PredicateGroup { Operator = GroupOperator.And, Predicates = new List<IPredicate>() };
            pg.Predicates.Add(Predicates.Field<Item>(i => i.ItemGroupId, Operator.Eq, 4));
            pg.Predicates.Add(Predicates.Field<Item>(i => i.ItemSubGroupId, Operator.Eq, 1));

            IEnumerable<Item> list = connection.GetList<Item>(pg);

            return list;
        }

        class MyQueryResultValue { public long Value { get; set; } }

        public bool TestMyQueryResultValue()
        {
            long Myparam = 12345;
            var result = connection.Query<MyQueryResultValue>(@" 
            declare @mytemp table(Value bigint)
            insert @mytemp  values (@Myparam)
            select * from @mytemp "
            , new { Myparam }).Single();

           return result.Value.Equals(Myparam);
        }


        public void Dispose()
        {
            connection.Dispose();
        }
    }
}