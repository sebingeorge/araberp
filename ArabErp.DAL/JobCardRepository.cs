﻿using System;
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
        public IEnumerable<PendingSO> GetPendingSO(int isProjectBased, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                query += " select SI.SaleOrderItemId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName, W.WorkDescr WorkDescription,IsPaymentApprovedForJobOrder, ISNULL(VIP.RegistrationNo, '-')RegistrationNo,DATEDIFF(DAY, S.SaleOrderDate, GETDATE()) Ageing, DATEDIFF(DAY, GETDATE(), S.EDateDelivery) Remaindays ";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId ";
                query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId ";
                query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                query += " left join VehicleModel V on V.VehicleModelId = W.VehicleModelId ";
                query += " left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId ";
                query += " LEFT JOIN VehicleInPass VIP ON SI.SaleOrderItemId = VIP.SaleOrderItemId ";
                query += " where J.SaleOrderItemId is null and S.SaleOrderApproveStatus = 1 ";
                query += " and S.isActive=1 and S.SaleOrderApproveStatus=1 and S.SaleOrderHoldStatus IS NULL and S.OrganizationId = " + OrganizationId.ToString() + "";
                query += " and S.isProjectBased = " + isProjectBased.ToString();
                query += " ORDER BY S.EDateDelivery DESC, S.SaleOrderDate DESC";
                return connection.Query<PendingSO>(query);
            }
        }
        public JobCard GetJobCardDetails(int SoItemId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if(isProjectBased == 0)
                {
                    query = @"select S.SaleOrderId, SI.SaleOrderItemId,
                    GETDATE() JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName,
                    ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, 
                    0 GoodsLanded, 0 BayId, W.FreezerUnitId FreezerUnitId, FU.FreezerUnitName, W.BoxId BoxId, B.BoxName, ISNULL(VI.RegistrationNo, '-') RegistrationNo, VI.VehicleInPassId InPassId
                    from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId
                    inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId
                    inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                    inner join VehicleModel V on V.VehicleModelId = W.VehicleModelId
					LEFT JOIN VehicleInPass VI ON SI.SaleOrderItemId = VI.SaleOrderItemId
					LEFT JOIN FreezerUnit FU ON W.FreezerUnitId = FU.FreezerUnitId
					LEFT JOIN Box B ON W.BoxId = B.BoxId
                    where SI.SaleOrderItemId = " + SoItemId.ToString();
                }
                else
                {
                    query = "select S.SaleOrderId, SI.SaleOrderItemId,";
                    query += " GETDATE() JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef,";
                    query += " ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, ";
                    query += " 0 GoodsLanded, 0 BayId, 0 FreezerUnitId, W.BoxId BoxId, B.BoxName";
                    query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId LEFT JOIN Box B ON W.BoxId = B.BoxId";
                    query += " where SI.SaleOrderItemId = " + SoItemId.ToString();
                }
                

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



        public string SaveJobCard(JobCard jc)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var id = InsertJobCard(jc);
                return id;
            }
        }

        public string InsertJobCard(JobCard objJobCard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                objJobCard.CreatedDate = DateTime.Now;
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCard.OrganizationId, 16, true, trn);
                    objJobCard.JobCardNo = internalId.ToString();
                    int id = 0;
                    string sql = @"insert  into JobCard(JobCardNo,JobCardDate,SaleOrderId,InPassId,WorkDescriptionId,FreezerUnitId,BoxId,BayId,SpecialRemarks,RequiredDate,EmployeeId,CreatedBy,CreatedDate,OrganizationId, SaleOrderItemId,isProjectBased) Values 
                                                       (@JobCardNo,@JobCardDate,@SaleOrderId,@InPassId,@WorkDescriptionId,@FreezerUnitId,@BoxId,@BayId,@SpecialRemarks,@RequiredDate,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId,@SaleOrderItemId,@isProjectBased);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, objJobCard, trn).Single();

                    int i = 0; ;
                    foreach (var item in objJobCard.JobCardTasks)
                    {
                        item.JobCardId = id;
                        item.SlNo = i;
                        JobCardTaskRepository repo = new JobCardTaskRepository();
                        var taskid = repo.InsertJobCardTask(item, connection, trn);
                        i++;
                    }
                    InsertLoginHistory(dataConnection, objJobCard.CreatedBy, "Create", "Job Card", id.ToString(), "0");
                    trn.Commit();
                    return objJobCard.JobCardNo;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    return "";
                }

            }
        }

        public int UpdateJobCard(JobCard objJobCard)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE JobCard SET JobCardNo = @JobCardNo ,JobCardDate = @JobCardDate ,SaleOrderId = @SaleOrderId ,ChasisNoRegNo = @ChasisNoRegNo,WorkDescription = @WorkDescription,FreezerUnitId = @FreezerUnitId,BoxId = @BoxId,BayId = @BayId,SpecialRemarks = @SpecialRemarks  OUTPUT INSERTED.JobCardId  WHERE JobCardId = @JobCardId";


                var id = connection.Execute(sql, objJobCard);
                InsertLoginHistory(dataConnection, objJobCard.CreatedBy, "Update", "Job Card", id.ToString(), "0");
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
                return connection.Query<Bay>("select BayId, BayName from Bay where BayId not in (select isnull(BayId,0)BayId from JobCard where ISNULL(JodCardCompleteStatus,0) = 0)");
            }
        }

        public IEnumerable<Employee> GetEmployeeList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Employee>("select * from Employee");
            }
        }
        public IEnumerable<JobCardTaskMaster> GetWorkVsTask(int workId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<JobCardTaskMaster>("select * from JobCardTaskMaster J inner join WorkVsTask W on J.JobCardTaskMasterId = W.JobCardTaskMasterId where WorkDescriptionId = " + workId.ToString());
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

        /// <summary>
        /// Return details of a jobcard such as date, sale order no and date, box name, freezer name
        /// </summary>
        /// <param name="jobCardId"></param>
        /// <returns></returns>
        public Consumption GetJobCardDetails1(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
									J.JobCardId,
	                                CONVERT(VARCHAR, J.JobCardDate, 106) JobCardDate,
	                                ISNULL(SO.SaleOrderRefNo, '') + ' - ' + CONVERT(VARCHAR, SO.SaleOrderDate, 106) SONoDate,
	                                ISNULL(F.FreezerUnitName, '') FreezerUnitName,
	                                ISNULL(B.BoxName, '') BoxName
                                FROM JobCard J
                                INNER JOIN SaleOrder SO ON J.SaleOrderId = SO.SaleOrderId
                                INNER JOIN FreezerUnit F ON J.FreezerUnitId = F.FreezerUnitId
                                INNER JOIN Box B ON J.BoxId = B.BoxId
                                WHERE JobCardId = @jobCardId";
                return connection.Query<Consumption>(query, new { jobCardId = jobCardId }).Single();
            }
        }

        //public void Dispose()
        //{
        //    connection.Dispose();
        //}
        public JobCard GetDetailsById(int JobCardId, int? JobCardTaskId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select * from JobCard where JobCardId = " + JobCardId.ToString();
                JobCard jc = connection.Query<JobCard>(sql).Single();
                jc.JobCardTasks = new List<JobCardTask>();
                if(jc!= null)
                {
                    sql = string.Empty;
                    sql = "select * from JobCardTask where JobCardId = " + JobCardId.ToString();
                    var tasks = connection.Query<JobCardTask>(sql);

                    if(JobCardTaskId != null)
                    {
                        var t = from a in tasks where a.JobCardTaskId == (JobCardTaskId ?? 0) select a;
                        foreach(var item in t)
                        {
                            jc.JobCardTasks.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in tasks)
                        {
                            jc.JobCardTasks.Add(item);
                        }
                    }                    
                }
                return jc;
            }
        }
        public IEnumerable<JobCard> GetAllJobCards( int ProjectBased,int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"select JobCardId,JobCardNo,JobCardDate,CustomerName,S.CustomerId from JobCard J inner join SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                               inner join Customer C ON C.CustomerId=S.CustomerId
                               where J.isActive=1 and J.OrganizationId = @OrganizationId and  J.JobCardId = ISNULL(NULLIF(@id, 0), J.JobCardId) and S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId)  AND J.JobCardDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) and J.isProjectBased = @ProjectBased 
                                ORDER BY J.JobCardDate DESC, J.CreatedDate DESC";
                return connection.Query<JobCard>(qry, new { id = id, cusid = cusid, from = from, to = to, OrganizationId = OrganizationId, ProjectBased = ProjectBased }).ToList();

            }
        }
    }
}