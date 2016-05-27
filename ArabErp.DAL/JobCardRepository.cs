using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;

namespace ArabErp
{


    public class JobCardRepository : BaseRepository
    {

        static string dataConnection = GetConnectionString("arab");

  
        //private SqlConnection connection;

     //   private SqlConnection connection => _connection ?? (_connection = ConnectionManager.connection);

        //public JobCardRepository ()
        //{

        //    if (connection==null)
        //    {
        //        connection = ConnectionManager.connection;
        //    }
                
        //}

        public string GetJobNumber(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string jobnumber = "";

                var jobs = connection.Query<JobCard>("select * from JobCard where JobCardId=" + id);

                if (jobs.Count() > 0)
                {
                    jobnumber = jobs.First().JobCardNo;
                }

                return jobnumber;

            }
        }



        public string SaveJobCard(JobCard jc)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                connection.Execute("Insert into JobCard (JobCardNo) Values ('" + jc.JobCardNo + "')");
                var rtn = "Saved " + jc.JobCardNo;
                return rtn;
            }
        }

        public int InsertJobCard(JobCard objJobCard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into JobCard(JobCardNo,JobCardDate,SaleOrderId,ChasisNoRegNo,WorkDescription,FreezerUnitId,BoxId,BayId,SpecialRemarks,RequiredDate,EmployeeId,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardNo,@JobCardDate,@SaleOrderId,@ChasisNoRegNo,@WorkDescription,@FreezerUnitId,@BoxId,@BayId,@SpecialRemarks,@RequiredDate,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCard).Single();
                return id;
            }
        }

        public int UpdateJobCard(JobCard objJobCard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE JobCard SET JobCardNo = @JobCardNo ,JobCardDate = @JobCardDate ,SaleOrderId = @SaleOrderId ,ChasisNoRegNo = @ChasisNoRegNo,WorkDescription = @WorkDescription,FreezerUnitId = @FreezerUnitId,BoxId = @BoxId,BayId = @BayId,SpecialRemarks = @SpecialRemarks  OUTPUT INSERTED.JobCardId  WHERE JobCardId = @JobCardId";


                var id = connection.Execute(sql, objJobCard);
                return id;
            }
        }

        public void SaveItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                connection.Insert<Item>(objItem);
            }
        }

        public void UpdateItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                connection.Update<Item>(objItem);
            }
        }

        public void DeleteItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                connection.Delete<Item>(objItem);
            }
        }
        public Item GetItem(int ItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                Item objItem = connection.Get<Item>(ItemId);
                return objItem;
            }
        }

        public IEnumerable<Item> GetAllItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IEnumerable<Item> list = connection.GetList<Item>();
                return list;
            }
        }

        public IEnumerable<Item> GetGroup1Items()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var predicate = Predicates.Field<Item>(i => i.ItemGroupId, Operator.Eq, 4);
                IEnumerable<Item> list = connection.GetList<Item>(predicate);
                return list;
            }
        }
        public IEnumerable<Item> GetGroup1andSubGroup1Items()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var pg = new PredicateGroup { Operator = GroupOperator.And, Predicates = new List<IPredicate>() };
                pg.Predicates.Add(Predicates.Field<Item>(i => i.ItemGroupId, Operator.Eq, 4));
                pg.Predicates.Add(Predicates.Field<Item>(i => i.ItemSubGroupId, Operator.Eq, 1));

                IEnumerable<Item> list = connection.GetList<Item>(pg);

                return list;
            }
        }

        class MyQueryResultValue { public long Value { get; set; } }

        public bool TestMyQueryResultValue()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                long Myparam = 12345;
                var result = connection.Query<MyQueryResultValue>(@" 
            declare @mytemp table(Value bigint)
            insert @mytemp  values (@Myparam)
            select * from @mytemp "
                , new { Myparam }).Single();

                return result.Value.Equals(Myparam);
            }
        }


        //public void Dispose()
        //{
        //    connection.Dispose();
        //}
    }
}