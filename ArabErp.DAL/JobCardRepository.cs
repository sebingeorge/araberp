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
        public IEnumerable<PendingSO> GetPendingSO(int isProjectBased, int OrganizationId, int? isService)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                query += @" select SI.SaleOrderItemId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName, W.WorkDescr WorkDescription,IsPaymentApprovedForJobOrder, ISNULL(VIP.RegistrationNo, '')RegistrationNo,ISNULL(VIP.ChassisNo, '') ChassisNo,DATEDIFF(DAY, S.SaleOrderDate, GETDATE()) Ageing, DATEDIFF(DAY, GETDATE(), S.EDateDelivery) Remaindays,S.isService
                  from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId
                  inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId
                  LEFT join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                  left join VehicleModel V on V.VehicleModelId = W.VehicleModelId
                  left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId ";
                if (isProjectBased == 1)
                {
                    query += " LEFT JOIN VehicleInPass VIP ON SI.SaleOrderItemId = VIP.SaleOrderItemId ";
                }
                else
                {
                    query += " INNER JOIN VehicleInPass VIP ON SI.SaleOrderItemId = VIP.SaleOrderItemId ";
                }
                query += @" where J.SaleOrderItemId is null and S.SaleOrderApproveStatus = 1 ";
                query += " and S.isActive=1 and S.SaleOrderApproveStatus=1 and S.SaleOrderHoldStatus IS NULL and S.OrganizationId = " + OrganizationId.ToString() + "";
                query += " and S.isProjectBased = " + isProjectBased.ToString();
                query += " and S.isService = " + isService.ToString();
                query += " ORDER BY S.EDateDelivery DESC, S.SaleOrderDate DESC";
                return connection.Query<PendingSO>(query);
            }
        }
        public JobCard GetJobCardDetails(int SoItemId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if (isProjectBased == 0)
                {
                    query = @"SELECT S.SaleOrderId, SI.SaleOrderItemId,
                    GETDATE() JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName,
                    ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, 
                    0 GoodsLanded, 0 BayId, W.FreezerUnitId FreezerUnitId, FU.ItemName FreezerUnitName, W.BoxId BoxId, 
                    B.ItemName BoxName, 
                    VI.RegistrationNo + CASE WHEN ISNULL(VI.RegistrationNo, '') <> '' AND ISNULL(VI.ChassisNo, '') <> '' THEN ' - ' END + VI.ChassisNo RegistrationNo, 
                    VI.VehicleInPassId InPassId, S.isService, S.isProjectBased, S.SaleOrderRefNo,S.SpecialRemarks
                    FROM SaleOrder S 
                    INNER JOIN Customer C ON S.CustomerId = C.CustomerId
                    INNER JOIN SaleOrderItem SI ON SI.SaleOrderId = S.SaleOrderId
                    INNER JOIN WorkDescription W ON W.WorkDescriptionId = SI.WorkDescriptionId
                    LEFT JOIN VehicleModel V ON V.VehicleModelId = W.VehicleModelId
					LEFT JOIN VehicleInPass VI ON SI.SaleOrderItemId = VI.SaleOrderItemId
				    LEFT JOIN Item FU ON W.FreezerUnitId = FU.ItemId
                    LEFT JOIN Item B ON W.BoxId = B.ItemId
                    WHERE SI.SaleOrderItemId = " + SoItemId.ToString();
                }
                else
                {
                    query = @"SELECT 
	                            S.SaleOrderId,
                                S.SaleOrderRefNo,
	                            SI.SaleOrderItemId,
	                            GETDATE() JobCardDate,
	                            C.CustomerId,
	                            C.CustomerName,
	                            S.CustomerOrderRef,
	                            ''ChasisNoRegNo,
	                            NULL WorkDescriptionId,
	                            '' as WorkDescription,
	                            '' WorkShopRequestRef, 
	                            0 GoodsLanded,
	                            NULL BayId,
	                            NULL FreezerUnitId,
	                            NULL BoxId,
	                            '' BoxName,
	                            S.isService,
	                            S.isProjectBased,

								STUFF((SELECT ', '+T2.ItemName + ', '+ T3.ItemName FROM SaleOrderItemUnit T1
									LEFT JOIN Item T2 ON T1.CondenserUnitId = T2.ItemId
									LEFT JOIN Item T3 ON T1.EvaporatorUnitId = T3.ItemId
									WHERE T1.SaleOrderItemId = SI.SaleOrderItemId FOR XML PATH('')), 1, 2, '') Units,
								STUFF((SELECT ', ' + T2.ItemName FROM SaleOrderItemDoor T1
									INNER JOIN Item T2 ON T1.DoorId = T2.ItemId
									WHERE T1.SaleOrderItemId = SI.SaleOrderItemId FOR XML PATH('')), 1, 2, '') Doors
                            FROM SaleOrder S 
                            INNER JOIN Customer C ON S.CustomerId = C.CustomerId
                            INNER JOIN SaleOrderItem SI ON SI.SaleOrderId = S.SaleOrderId

                            WHERE SI.SaleOrderItemId = @id";// +SoItemId.ToString();
                }
                JobCard jobcard = connection.Query<JobCard>(query, new { id = SoItemId }).FirstOrDefault();
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
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCard.OrganizationId, (objJobCard.isService == 1 ? 34 : 16), true, trn);
                    objJobCard.JobCardNo = internalId.ToString();
                    int id = 0;
                    string sql = @"insert  into JobCard(JobCardNo,JobCardDate,SaleOrderId,InPassId,WorkDescriptionId,FreezerUnitId,BoxId,BayId,SpecialRemarks,RequiredDate,EmployeeId,CreatedBy,CreatedDate,OrganizationId, SaleOrderItemId,isProjectBased, isService, Complaints) Values 
                                                       (@JobCardNo,@JobCardDate,@SaleOrderId,@InPassId,NULLIF(@WorkDescriptionId, 0),@FreezerUnitId,@BoxId,@BayId,@SpecialRemarks,@RequiredDate,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId,@SaleOrderItemId,@isProjectBased, @isService, @Complaints);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, objJobCard, trn).Single();

                    int i = 0; ;
                    foreach (var item in objJobCard.JobCardTasks)
                    {
                        item.JobCardId = id;
                        item.SlNo = i;
                        JobCardTaskRepository repo = new JobCardTaskRepository();
                        var taskid = repo.InsertJobCardTask(item, connection, trn);
                        if (taskid == 0) throw new Exception();
                        i++;
                    }
                    InsertLoginHistory(dataConnection, objJobCard.CreatedBy, "Create", "Job Card", id.ToString(), "0");
                    trn.Commit();
                    return objJobCard.JobCardNo;
                }
                catch (Exception)
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
                IDbTransaction txn = connection.BeginTransaction();
                string sql = @"UPDATE JobCard SET
	                                [JobCardDate] = @JobCardDate,
	                                BayId = @BayId,
	                                SpecialRemarks=@SpecialRemarks,
	                                RequiredDate=@RequiredDate,
	                                EmployeeId=@EmployeeId,
                                    Complaints = @Complaints
                                WHERE JobCardId = @JobCardId;
                                --DELETE FROM JobCardTask WHERE JobCardId = @JobCardId;";
                try
                {
                    var id = connection.Execute(sql, objJobCard, txn);

                    #region get the list of tasks in use
                    var usedTasks = objJobCard.JobCardTasks.Where(x => x.isTaskUsed).Select(x => x.JobCardTaskId).ToList();
                    #endregion

                    #region delete tasks that are not in use
                    sql = @"DELETE FROM JobCardTask WHERE JobCardId = @jobcard AND JobCardTaskId NOT IN @usedTasks";
                    connection.Execute(sql, new { jobcard = objJobCard.JobCardId, usedTasks = usedTasks }, txn);
                    #endregion

                    #region insert tasks that are not in use
                    var taskList = objJobCard.JobCardTasks.Where(x => !x.isTaskUsed || x.JobCardTaskId == 0).Select(x => x).ToList();
                    foreach (var item in taskList)
                    {
                        item.JobCardId = objJobCard.JobCardId;
                        item.JobCardTaskId = item.JobCardTaskMasterId;
                        if (new JobCardTaskRepository().InsertJobCardTask(item, connection, txn) == 0)
                            throw new Exception("Some error occured while saving jobcard task");
                    }
                    #endregion

                    //int i = 0;
                    //foreach (var item in objJobCard.JobCardTasks)
                    //{

                    //    //insert tasks that are not used
                    //    item.JobCardId = objJobCard.JobCardId;
                    //    if (new JobCardTaskRepository().InsertJobCardTask(item, connection, txn) == 0)
                    //        throw new Exception("Some error occured while saving jobcard task");

                    //    //item.SlNo = i;
                    //    JobCardTaskRepository repo = new JobCardTaskRepository();
                    //    if (item.JobCardTaskId == 0)
                    //    {
                    //        //insert jobcard task
                    //        i++;

                    //    }
                    //    else
                    //    {
                    //        //update jobcard task
                    //        i++;
                    //        sql = @"";
                    //    }
                    //}

                    InsertLoginHistory(dataConnection, objJobCard.CreatedBy, "Update", "Job Card", id.ToString(), objJobCard.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
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

        public IEnumerable<Bay> GetBayList(int IsService)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Bay>("select BayId, BayName from Bay where BayId not in (select isnull(BayId,0)BayId from JobCard where ISNULL(JodCardCompleteStatus,0) = 0) AND ISNULL(Bay.isActive, 1) = 1 --and  ISNULL(Bay.IsService,0)=@IsService", new { IsService = IsService });
            }
        }

        /// <summary>
        /// Gets all bays that are free, considering the given job card's bay as free
        /// </summary>
        /// <param name="JobCardId"></param>
        /// <returns></returns>
        public IEnumerable<Bay> GetBayList1(int JobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Bay>("select BayId, BayName from Bay where BayId not in (select isnull(BayId,0)BayId from JobCard where ISNULL(JodCardCompleteStatus,0) = 0 AND JobCardId != @JobCardId) AND ISNULL(Bay.isActive, 1) = 1", new { JobCardId = JobCardId });
            }
        }

        public IEnumerable<Employee> GetEmployeeList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Employee>("select * from Employee WHERE isActive=1 ");
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
                return connection.Query<FreezerUnit>("SELECT ItemId FreezerUnitId,ItemName FreezerUnitName FROM Item WHERE FreezerUnit=1");
            }
        }
        public IEnumerable<Box> GetBoxes()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Box>("SELECT ItemId BoxId,ItemName BoxName FROM Item WHERE Box=1");
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
	                            ISNULL(F.ItemName, '') FreezerUnitName,
	                            ISNULL(B.ItemName, '') BoxName
                                FROM JobCard J
                                INNER JOIN SaleOrder SO ON J.SaleOrderId = SO.SaleOrderId
                                LEFT JOIN Item F ON J.FreezerUnitId = F.ItemId
                                LEFT JOIN Item B ON J.BoxId = B.ItemId
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
                if (jc != null)
                {
                    sql = string.Empty;
                    sql = "select * from JobCardTask where JobCardId = " + JobCardId.ToString();
                    var tasks = connection.Query<JobCardTask>(sql);

                    if (JobCardTaskId != null)
                    {
                        var t = from a in tasks where a.JobCardTaskId == (JobCardTaskId ?? 0) select a;
                        foreach (var item in t)
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
        public IEnumerable<JobCard> GetAllJobCards(int ProjectBased, int service, int id, int cusid, int OrganizationId, DateTime? from, DateTime? to, string RegNo = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"SELECT JobCardId,JobCardNo,JobCardDate,CustomerName,S.CustomerId, I1.ItemName FreezerUnitName, I2.ItemName BoxName, E.EmployeeName,
                               ISNULL(ChassisNo,'')ChasisNo,ISNULL(RegistrationNo,'')RegistrationNo
                               FROM JobCard J 
                               INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                               INNER JOIN Customer C ON C.CustomerId=S.CustomerId
                               LEFT JOIN VehicleInPass V ON VehicleInPassId=J.InPassId
							   LEFT JOIN ITEM I1 ON J.FreezerUnitId = I1.ItemId
							   LEFT JOIN ITEM I2 ON J.BoxId = I2.ItemId
							   LEFT JOIN Employee E ON J.EmployeeId = E.EmployeeId
                               WHERE J.isActive=1 and J.OrganizationId = @OrganizationId and  J.JobCardId = ISNULL(NULLIF(@id, 0), J.JobCardId) 
                               AND S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId)  AND J.JobCardDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) 
                               AND ISNULL(@to, GETDATE()) and J.isProjectBased = @ProjectBased and J.isService=@service  
                               AND (ISNULL(V.RegistrationNo, '') LIKE '%'+@RegNo+'%' OR ISNULL(V.ChassisNo, '') LIKE '%'+@RegNo+'%')
                               ORDER BY J.JobCardDate DESC, J.CreatedDate DESC";
                return connection.Query<JobCard>(qry, new { id = id, cusid = cusid, from = from, to = to, OrganizationId = OrganizationId, ProjectBased = ProjectBased, service = service, RegNo = RegNo }).ToList();

            }
        }

        public JobCard GetJobCardHD(int JobCardId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sq;
                string qu;
                string sql = @"select isservice,isProjectBased from JobCard  where JobCardId=@JobCardId";
                var objJobcard = connection.Query<JobCard>(sql, new { JobCardId = JobCardId, OrganizationId = OrganizationId }).First<JobCard>();
                if (objJobcard.isService == 0)
                {
                    sq = @"SELECT O.*,J.JobCardId,JobCardNo,JobCardDate,
                              C.CustomerName Customer,
								--V.ChassisNo + CASE WHEN ISNULL(V.RegistrationNo, '') <> '' AND ISNULL(V.ChassisNo, '') <> '' THEN ' - ' END + V.RegistrationNo ChasisNo,
                                ISNULL(V.RegistrationNo, '') + CASE WHEN ISNULL(V.RegistrationNo, '') <> '' AND ISNULL(V.ChassisNo, '') <> '' THEN ' - ' ELSE '' END + ISNULL(V.ChassisNo, '') RegistrationNo, 
                                VM.VehicleModelName,
								US.UserName CreatedUser,US.Signature CreatedUsersig,DI.DesignationName CreatedDes,
                                J.Complaints, J.isProjectBased";
                    sq += objJobcard.isProjectBased == 0 ? 
                        ", U.ItemName FreezerUnitName, UI.ItemName BoxName" : 
                        @",STUFF((SELECT ', '+T2.ItemName + ', '+ T3.ItemName FROM SaleOrderItemUnit T1
		                LEFT JOIN Item T2 ON T1.CondenserUnitId = T2.ItemId
		                LEFT JOIN Item T3 ON T1.EvaporatorUnitId = T3.ItemId
		                WHERE T1.SaleOrderItemId = J.SaleOrderItemId), 1, 2, '') FreezerUnitName,
                        STUFF((SELECT ', ' + T2.ItemName FROM SaleOrderItemDoor T1
		                INNER JOIN Item T2 ON T1.DoorId = T2.ItemId
		                WHERE T1.SaleOrderItemId = J.SaleOrderItemId), 1, 2, '') BoxName";
                    sq += @" FROM JobCard J
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
							    inner join Organization O ON O.OrganizationId=J.OrganizationId
								LEFT JOIN VehicleInPass V ON V.VehicleInPassId=J.InPassId
								LEFT JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
								LEFT JOIN Item U ON U.ItemId=W.FreezerUnitId
								LEFT JOIN Item UI ON UI.ItemId=W.BoxId
								LEFT JOIN VehicleModel VM ON VM.VehicleModelId=W.VehicleModelId
								left join [User] US ON US.[UserId]=J.CreatedBy
								left join Designation DI ON DI.DesignationId=US.DesignationId
                                WHERE J.JobCardId=@JobCardId";

                    var objJobCardId = connection.Query<JobCard>(sq, new
                    {
                        JobCardId = JobCardId,
                        OrganizationId = OrganizationId
                    }).First<JobCard>();

                    return objJobCardId;
                }
                else
                {
                    qu = @"SELECT O.*,J.JobCardId,JobCardNo,JobCardDate,
                              C.CustomerName Customer,U.ItemName FreezerUnitName,
								v.RegistrationNo RegistrationNo,VM.VehicleModelName,UI.ItemName BoxName,
								SE.Complaints,
								concat(SE.BoxMake,CASE WHEN (ISNULL(LTRIM(RTRIM(SE.BoxMake)),'') = '' OR ISNULL(LTRIM(RTRIM(SE.BoxNo)), '') = '') THEN '' ELSE ' / ' END ,SE.BoxNo) BoxName,
								concat(SE.FreezerMake,CASE WHEN (ISNULL(LTRIM(RTRIM(SE.FreezerMake)),'') = '' OR ISNULL(LTRIM(RTRIM(SE.FreezerModel)), '') = '') THEN '' ELSE ' / ' END ,SE.FreezerModel) FreezerUnitName,
								SE.VehicleMake VehicleModelName,SE.ServiceEnquiryId,US.UserName CreatedUser,US.Signature CreatedUsersig,DI.DesignationName CreatedDes
                                FROM JobCard J
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
							    inner join Organization O ON O.OrganizationId=J.OrganizationId
                                LEFT JOIN Item U ON U.ItemId=J.FreezerUnitId
								LEFT JOIN Item UI ON UI.ItemId=J.BoxId
								LEFT JOIN VehicleInPass V ON V.VehicleInPassId=J.InPassId
								LEFT JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
								LEFT JOIN VehicleModel VM ON VM.VehicleModelId=W.VehicleModelId
								left join ServiceEnquiry SE ON SE.ServiceEnquiryId=S.ServiceEnquiryId
								left join [User] US ON US.[UserId]=J.CreatedBy
								left join Designation DI ON DI.DesignationId=US.DesignationId
                                WHERE J.JobCardId=@JobCardId";
                }

                var objJobCard = connection.Query<JobCard>(qu, new
                {
                    JobCardId = JobCardId,
                    OrganizationId = OrganizationId

                }).First<JobCard>();

                return objJobCard;

            }

        }
        public JobCard GetJobCardDetails2(int JobCardId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                //if (isProjectBased == 0)
                //{
                query = @"select S.SaleOrderId, SI.SaleOrderItemId,
                    JC.JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef, V.VehicleModelName,
                    ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, 
                    0 GoodsLanded, 0 BayId, W.FreezerUnitId FreezerUnitId, FU.ItemName FreezerUnitName, W.BoxId BoxId, B.ItemName BoxName, 
                    ISNULL(VI.RegistrationNo, '') + CASE WHEN ISNULL(VI.RegistrationNo, '') <> '' AND ISNULL(VI.ChassisNo, '') <> '' THEN ' - ' ELSE '' END + ISNULL(VI.ChassisNo, '') RegistrationNo, 
                    VI.VehicleInPassId InPassId, S.isProjectBased,
					JC.JobCardId, JC.JobCardNo, JC.BayId, CONVERT(VARCHAR, JC.RequiredDate, 106) RequiredDate, JC.EmployeeId,s.isService, 
                    JC.SpecialRemarks, JC.Complaints,

					STUFF((SELECT ', '+T2.ItemName + ', '+ T3.ItemName FROM SaleOrderItemUnit T1
						LEFT JOIN Item T2 ON T1.CondenserUnitId = T2.ItemId
						LEFT JOIN Item T3 ON T1.EvaporatorUnitId = T3.ItemId
						WHERE T1.SaleOrderItemId = SI.SaleOrderItemId FOR XML PATH('')), 1, 2, '') Units,
					STUFF((SELECT ', ' + T2.ItemName FROM SaleOrderItemDoor T1
						INNER JOIN Item T2 ON T1.DoorId = T2.ItemId
						WHERE T1.SaleOrderItemId = SI.SaleOrderItemId FOR XML PATH('')), 1, 2, '') Doors
                    
                    from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId
                    inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId
                    LEFT join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                    LEFT join VehicleModel V on V.VehicleModelId = W.VehicleModelId
					LEFT JOIN VehicleInPass VI ON SI.SaleOrderItemId = VI.SaleOrderItemId
					LEFT JOIN Item FU ON W.FreezerUnitId = FU.ItemId
					LEFT JOIN Item B ON W.BoxId = B.ItemId
                    INNER JOIN JobCard JC ON SI.SaleOrderItemId = JC.SaleOrderItemId
					WHERE JC.JobCardId = @JobCardId";
                //}
                //else
                //    query = "select S.SaleOrderId, SI.SaleOrderItemId,";
                //    query += " GETDATE() JobCardDate, C.CustomerId, C.CustomerName, S.CustomerOrderRef,";
                //    query += " ''ChasisNoRegNo, W.WorkDescriptionId, W.WorkDescr as WorkDescription, '' WorkShopRequestRef, ";
                //    query += " 0 GoodsLanded, 0 BayId, 0 FreezerUnitId, W.BoxId BoxId, B.BoxName";
                //    query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId";
                //    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                //    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId LEFT JOIN Box B ON W.BoxId = B.BoxId";
                //    query += " where SI.SaleOrderItemId = " + SoItemId.ToString();
                //}


                JobCard jobcard = connection.Query<JobCard>(query, new { JobCardId = JobCardId }, txn).FirstOrDefault();

                #region old query 3.1.2017 5.47p
                //                query = @"SELECT
                //	                            JobCardTaskMasterId JobCardTaskId,
                //	                            EmployeeId,
                //	                            CONVERT(VARCHAR, TaskDate, 106) TaskDate,
                //	                            SlNo,
                //	                            [Hours]
                //                            FROM JobCardTask
                //                            WHERE JobCardId = @JobCardId"; 
                #endregion

                query = @"SELECT DISTINCT
                                JT.JobCardTaskId,
	                            JobCardTaskMasterId,
	                            JT.EmployeeId,
	                            CONVERT(VARCHAR, TaskDate, 106) TaskDate,
	                            SlNo,
	                            [Hours],
	                            CASE WHEN DT.JobCardTaskId IS NULL THEN 0 ELSE 1 END isTaskUsed
                            FROM JobCardTask JT
	                            LEFT JOIN JobCardDailyActivityTask DT ON JT.JobCardTaskId = DT.JobCardTaskId
                            WHERE JobCardId = @JobCardId";

                jobcard.JobCardTasks = connection.Query<JobCardTask>(query, new { JobCardId = JobCardId }, txn).ToList();

                query = @"SELECT ISNULL(JodCardCompleteStatus, 0) FROM JobCard WHERE JobCardId=@JobCardId";
                jobcard.IsUsed = Convert.ToBoolean(connection.Query<int>(query, new { JobCardId = jobcard.JobCardId }, txn).First());
                if (jobcard.IsUsed) return jobcard;

                //                try
                //                {
                //                    query = @"DELETE FROM JobCardTask WHERE JobCardId = @JobCardId;
                //                              DELETE FROM JobCard WHERE JobCardId = @JobCardId;";
                //                    connection.Query<JobCardTask>(query, new { JobCardId = JobCardId }, txn).ToList();
                //                    txn.Rollback();
                //                    jobcard.IsTaskUsed = false;
                //                }
                //                catch
                //                {
                //                    txn.Rollback();
                //                    jobcard.IsTaskUsed = true;
                //                }

                return jobcard;
            }
        }

        public string DeleteJobCard(int JobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM JobCardTask WHERE JobCardId = @JobCardId;
                              DELETE FROM JobCard OUTPUT deleted.JobCardNo WHERE JobCardId = @JobCardId;";
                    string output = connection.Query<string>(query, new { JobCardId = JobCardId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public JobCard GetOrganization(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from organization where OrganizationId=@OrganizationId";
                var objJobCardId = connection.Query<JobCard>(sql, new
                {
                    OrganizationId = OrganizationId
                }).First<JobCard>();

                return objJobCardId;
            }
        }

        public IEnumerable GetTasksForFreezerAndBox(int? SaleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                                FZR_JTM.JobCardTaskMasterId, FZR_JTM.JobCardTaskName
                                FROM SaleOrder SO
                                INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN ItemVsTasks I_FZR ON WD.FreezerUnitId = I_FZR.ItemId
                                INNER JOIN JobCardTaskMaster FZR_JTM ON I_FZR.JobCardTaskMasterId = FZR_JTM.JobCardTaskMasterId
                                WHERE SOI.SaleOrderItemId = @id
                                UNION
                                SELECT 
	                                BOX_JTM.JobCardTaskMasterId, BOX_JTM.JobCardTaskName
                                FROM SaleOrder SO
                                INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN ItemVsTasks I_BOX ON WD.BoxId = I_BOX.ItemId
                                INNER JOIN JobCardTaskMaster BOX_JTM ON I_BOX.JobCardTaskMasterId = BOX_JTM.JobCardTaskMasterId
                                WHERE SOI.SaleOrderItemId = @id";
                return connection.Query<JobCardTaskMaster>(query, new { id = SaleOrderItemId }).ToList();
            }
        }

        public object GetJobCardsOnHold(string jobcard, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                JC.JobCardId,
	                                JC.JobCardNo,
	                                JC.JobCardDate,
	                                Bay.BayName,
	                                VM.VehicleModelName,
	                                [User].UserName HeldBy,
	                                JC.HeldOn
                                FROM JobCard JC
	                                INNER JOIN Bay ON JC.BayId = Bay.BayId
	                                INNER JOIN [User] ON JC.HeldBy = [User].UserId
	                                INNER JOIN SaleOrderItem SOI ON JC.SaleOrderItemId = SOI.SaleOrderItemId
	                                INNER JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                                WHERE ISNULL(JC.isOnHold, 0) = 1
                                    AND JC.JobCardNo LIKE '%'+@jobcard+'%'";
                return connection.Query<JobCard>(query, new { jobcard = jobcard }).ToList();
            }
        }

        public bool ReleaseJobCard(int jobcard, int ReleasedBy)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"IF EXISTS(
		                                SELECT * FROM JobCard
		                                WHERE BayId = (SELECT BayId FROM JobCard 
						                                WHERE JobCardId = @jobcard)
		                                AND ISNULL(JobCard.JodCardCompleteStatus, 0) = 0)
                                    BEGIN
		                                    SELECT 0;
                                    END
                                    ELSE
                                    BEGIN
	                                    UPDATE JobCard SET isOnHold = NULL, ReleasedOn = GETDATE(), ReleasedBy = @ReleasedBy, 
                                        JodCardCompleteStatus = NULL, JodCardCompletedDate = NULL
	                                    WHERE JobCardId = @jobcard;
	                                    SELECT 1;
                                    END";
                    return connection.Query<bool>(query, new { jobcard = jobcard, ReleasedBy = ReleasedBy }).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}