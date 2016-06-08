using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;
using System.Collections;

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
        public IEnumerable<PendingSO> GetPendingSO()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "select SI.SaleOrderItemId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId";
                query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                query += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                query += " left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId where J.SaleOrderItemId is null";
                return connection.Query<PendingSO>(query);
            }
        }
        public JobCard GetJobCardDetails(int SoItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "select S.SaleOrderId, SI.SaleOrderItemId,";
                query += " GETDATE() JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName,";
                query += " ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, ";
                query += " 0 GoodsLanded, 0 BayId, 0 FreezerUnitId, 0 BoxId";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId";
                query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                query += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                query += " where SI.SaleOrderItemId = " + SoItemId.ToString();

                JobCard jobcard = connection.Query<JobCard>(query).FirstOrDefault();
                return jobcard;
            }
        }
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



        public int SaveJobCard(JobCard jc)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var id = InsertJobCard(jc);
                return id;
            }
        }

        public int InsertJobCard(JobCard objJobCard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                objJobCard.CreatedDate = DateTime.Now;
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;
                    string sql = @"insert  into JobCard(JobCardNo,JobCardDate,SaleOrderId,InPassId,WorkDescriptionId,FreezerUnitId,BoxId,BayId,SpecialRemarks,RequiredDate,EmployeeId,CreatedBy,CreatedDate,OrganizationId, SaleOrderItemId) Values (@JobCardNo,@JobCardDate,@SaleOrderId,@InPassId,@WorkDescriptionId,@FreezerUnitId,@BoxId,@BayId,@SpecialRemarks,@RequiredDate,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId,@SaleOrderItemId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, objJobCard,trn).Single();

                    int i = 0; ;
                    foreach (var item in objJobCard.JobCardTasks)
                    {
                        item.JobCardId = id;
                        item.SlNo = i;
                        JobCardTaskRepository repo = new JobCardTaskRepository();
                        var taskid = repo.InsertJobCardTask(item,connection,trn);
                        i++;
                    }
                    trn.Commit();
                    return id;
                }
                catch(Exception ex)
                {
                    trn.Rollback();
                    return 0;
                }
                
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

        public IEnumerable<Bay> GetBayList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Bay>("select BayId, BayName from Bay where BayId not in (select BayId from JobCard where ISNULL(JodCardCompleteStatus,0) = 0)");
            }
        }

        public IEnumerable<Employee> GetEmployeeList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Employee>("select * from Employee");
            }
        }
        public IEnumerable<Task> GetWorkVsTask(int workId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Task>("select * from Task inner join WorkVsTask on TaskId = JobCardTaskMasterId where WorkDescriptionId = " + workId.ToString());
            }
        }
        public IEnumerable<FreezerUnit> GetFreezerUnits()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<FreezerUnit>("select * from FreezerUnit");
            }
        }
        public IEnumerable<Box> GetBoxes()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Box>("select * from Box");
            }
        }

        //public void Dispose()
        //{
        //    connection.Dispose();
        //}
    }
}