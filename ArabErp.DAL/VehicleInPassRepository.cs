﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class VehicleInPassRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertVehicleInPass(VehicleInPass objVehicleInPass)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = @"SELECT SaleOrderItemId FROM VehicleInPass WHERE SaleOrderItemId = @id";
                    if (connection.Query<int>(sql, new { id = objVehicleInPass.SaleOrderItemId }, txn).FirstOrDefault() != 0)
                        throw new DuplicateNameException();

                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objVehicleInPass.OrganizationId, 15, true, txn);

                    objVehicleInPass.VehicleInPassNo = internalId;

                    sql = @"INSERT INTO VehicleInPass(
                            VehicleInPassNo,
                            SaleOrderItemId,
                            SaleOrderId,
                            RegistrationNo,
                            ChassisNo,
                            VehicleInPassDate,
                            EmployeeId,
                            Remarks,
                            CreatedBy,
                            CreatedDate,
                            OrganizationId,
                            isActive)
                         VALUES(
                            @VehicleInPassNo,
                            @SaleOrderItemId,
                            (SELECT SaleOrderId FROM SaleOrderItem WHERE SaleOrderItemId = @SaleOrderItemId),
                            @RegistrationNo,
                            @ChassisNo,
                            @VehicleInPassDate,
                            @EmployeeId,
                            @Remarks,
                            @CreatedBy,
                            @CreatedDate,
                            @OrganizationId,
                            1
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT)VehicleInPassId";


                    var id = connection.Query<int>(sql, objVehicleInPass, txn).Single();

                    InsertLoginHistory(dataConnection, objVehicleInPass.CreatedBy, "Create", "Vehicle Inpass", id.ToString(), "0");
                    txn.Commit();

                    return id;
                }
                catch (DuplicateNameException)
                {
                    txn.Rollback();
                    throw;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return id1;
                }
            }
        }

        //        InsertLoginHistory(dataConnection, objVehicleInPass.CreatedBy, "Create", "Vehicle Inpass", id.ToString(), "0");
        //        return id;
        //    }
        //}


        public VehicleInPass GetVehicleInPass(int VehicleInPassId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where VehicleInPassId=@VehicleInPassId";

                var objVehicleInPass = connection.Query<VehicleInPass>(sql, new
                {
                    VehicleInPassId = VehicleInPassId
                }).First<VehicleInPass>();

                return objVehicleInPass;
            }
        }
      
        
        public List<VehicleInPass> GetVehicleInPasss()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where isActive=1";

                var objVehicleInPasss = connection.Query<VehicleInPass>(sql).ToList<VehicleInPass>();

                return objVehicleInPasss;
            }
        }

        public int UpdateVehicleInPass(VehicleInPass objVehicleInPass)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string sql = @" UPDATE
                                VehicleInPass SET VehicleInPassNo=VehicleInPassNo,SaleOrderItemId=@SaleOrderItemId,
                                RegistrationNo=@RegistrationNo,ChassisNo=@ChassisNo,VehicleInPassDate=@VehicleInPassDate,EmployeeId=@EmployeeId,
                                Remarks=@Remarks,CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId
                                WHERE VehicleInPassId = @VehicleInPassId;";
                try
                {
                    var id = connection.Execute(sql, objVehicleInPass, txn);

                    int i = 0;

                    InsertLoginHistory(dataConnection, objVehicleInPass.CreatedBy, "Update", "Vehicle InPass", id.ToString(), objVehicleInPass.OrganizationId.ToString());
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


        public String DeleteVehicleInPass(int VehicleInPassId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = @"Delete VehicleInPass  OUTPUT DELETED.VehicleInPassNo WHERE VehicleInPassId=@VehicleInPassId";

                    string output = connection.Query<string>(sql, new { VehicleInPassId = VehicleInPassId }, txn).First();
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
        /// <summary>
        /// Return all sale order items that are not in vehicle in-pass
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<PendingSO> PendingVehicleInpass(int customerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingSO>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE CustomerId = @customerId AND ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1 AND isProjectBased=0 
                    SELECT SaleOrderId, SaleOrderItemId, WorkDescriptionId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT SaleOrderItemId INTO #VEHICLE_INPASS FROM VehicleInPass WHERE ISNULL(isActive, 1) = 1;
                    SELECT WorkDescriptionId, WorkDescr  INTO #WORK FROM WorkDescription WHERE ISNULL(isActive, 1) = 1;
                    SELECT VehicleModelId, VehicleModelName, VehicleModelDescription INTO #MODEL FROM VehicleModel WHERE ISNULL(isActive, 1) = 1;

                    SELECT SO.SaleOrderId, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SaleOrderDate, 106) SaleOrderRefNo, SOI.SaleOrderItemId, WorkDescr WorkDescription, VehicleModelName+' - '+VehicleModelDescription VehicleModelName FROM #SALE SO
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #VEHICLE_INPASS VI ON SOI.SaleOrderItemId = VI.SaleOrderItemId
                    LEFT JOIN #WORK W ON SOI.WorkDescriptionId = W.WorkDescriptionId
                    LEFT JOIN #MODEL M ON SOI.VehicleModelId = M.VehicleModelId
                    WHERE VI.SaleOrderItemId IS NULL;

                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #VEHICLE_INPASS;
                    DROP TABLE #MODEL;
                    DROP TABLE #WORK;
                    DROP TABLE #SALE;", new { customerId = customerId }).ToList();
            }
        }
        public IEnumerable<PendingSO> PendingVehicleInpassforAlert(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingSO>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE  ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1 AND isProjectBased=0 and OrganizationId=@OrganizationId
                    SELECT SaleOrderId, SaleOrderItemId, WorkDescriptionId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT SaleOrderItemId INTO #VEHICLE_INPASS FROM VehicleInPass WHERE ISNULL(isActive, 1) = 1;
                    SELECT WorkDescriptionId, WorkDescr  INTO #WORK FROM WorkDescription WHERE ISNULL(isActive, 1) = 1;
                    SELECT VehicleModelId, VehicleModelName, VehicleModelDescription INTO #MODEL FROM VehicleModel WHERE ISNULL(isActive, 1) = 1;

                    SELECT SO.SaleOrderId, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SaleOrderDate, 106) SaleOrderRefNo, SOI.SaleOrderItemId, WorkDescr WorkDescription, VehicleModelName+' - '+VehicleModelDescription VehicleModelName FROM #SALE SO
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #VEHICLE_INPASS VI ON SOI.SaleOrderItemId = VI.SaleOrderItemId
                    LEFT JOIN #WORK W ON SOI.WorkDescriptionId = W.WorkDescriptionId
                    LEFT JOIN #MODEL M ON SOI.VehicleModelId = M.VehicleModelId
                    WHERE VI.SaleOrderItemId IS NULL;

                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #VEHICLE_INPASS;
                    DROP TABLE #MODEL;
                    DROP TABLE #WORK;
                    DROP TABLE #SALE;", new {OrganizationId = OrganizationId}).ToList();
            }
        }
        public PendingSO GetSaleOrderItemDetails(int saleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingSO>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1
                    SELECT SaleOrderId, SaleOrderItemId, WorkDescriptionId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT WorkDescriptionId, WorkDescr INTO #WORK FROM WorkDescription WHERE ISNULL(isActive, 1) = 1;
                    SELECT VehicleModelId, VehicleModelName, VehicleModelDescription INTO #MODEL FROM VehicleModel WHERE ISNULL(isActive, 1) = 1;

                    SELECT SO.SaleOrderId, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SaleOrderDate, 106) SaleOrderRefNo, SOI.SaleOrderItemId, WorkDescr WorkDescription, VehicleModelName+' - '+VehicleModelDescription VehicleModelName, C.CustomerName FROM #SALE SO
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #WORK W ON SOI.WorkDescriptionId = W.WorkDescriptionId
                    LEFT JOIN #MODEL M ON SOI.VehicleModelId = M.VehicleModelId
					LEFT JOIN Customer C ON SO.CustomerId = C.CustomerId
                    WHERE SOI.SaleOrderItemId = @saleOrderItemId;

                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #MODEL;
                    DROP TABLE #WORK;
                    DROP TABLE #SALE;", new { saleOrderItemId = saleOrderItemId }).Single();
            }
        }

        public IEnumerable<VehicleInPass> GetPreviousList(int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"select VehicleInPassId,VehicleInPassNo,VehicleInPassDate,SaleOrderRefNo,SaleOrderDate,RegistrationNo,CustomerName from VehicleInPass V
                               inner join SaleOrder S ON S.SaleOrderId=V.SaleOrderId
                               inner join Customer C ON C.CustomerId=S.CustomerId where V.isActive=1 and V.OrganizationId = @OrganizationId and  V.VehicleInPassId = ISNULL(NULLIF(@id, 0), V.VehicleInPassId)
                               and S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId) AND V.VehicleInPassDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                ORDER BY V.VehicleInPassDate DESC, V.VehicleInPassNo DESC";
                return connection.Query<VehicleInPass>(qry, new { OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from }).ToList();

            }
        }

        public VehicleInPass GetVehicleInPassHD(int VehicleInPassId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT VehicleInPassId,VehicleInPassNo,VehicleInPassDate,C.CustomerName,
                                CONCAT(S.SaleOrderRefNo,' - ',convert(varchar(15),S.SaleOrderDate,106))SONODATE,
                                VM.VehicleModelName,W.WorkDescr,RegistrationNo,EmployeeName,V.Remarks,E.EmployeeId,
                                S.SaleOrderId,SI.SaleOrderItemId,ISNULL(J.InPassId,0)InPassId
                                FROM VehicleInPass V
                                INNER JOIN SaleOrder S ON S.SaleOrderId=V.SaleOrderId
                                INNER JOIN SaleOrderItem SI ON SI.SaleOrderId=S.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
                                INNER JOIN VehicleModel VM ON VM.VehicleModelId=SI.VehicleModelId
                                INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                                INNER JOIN Employee E ON E.EmployeeId=V.EmployeeId
                                LEFT JOIN JobCard J ON J.InPassId=V.VehicleInPassId
                                WHERE VehicleInPassId=@VehicleInPassId";

                var objVehicleInPass = connection.Query<VehicleInPass>(sql, new
                {
                    VehicleInPassId = VehicleInPassId
                }).First<VehicleInPass>();

                return objVehicleInPass;
            }
        }
        public int id1 { get; set; }




        public IEnumerable<VehicleInPass> GetVehicleInpassRegister(string InstallType, string CustomerName, string RegNo,string status)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"SELECT T1.* 
                               INTO #TEMP1 FROM 
                               (SELECT DISTINCT SOI.SaleOrderItemId,
                               JM.JobCardTaskMasterId, JM.JobCardTaskName
                               FROM SaleOrder SO
                               INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                               INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                               INNER JOIN ItemVsTasks IT ON IT.ItemId = WD.FreezerUnitId
                               INNER JOIN JobCardTaskMaster JM ON JM.JobCardTaskMasterId=IT.JobCardTaskMasterId
                               UNION 
                               SELECT DISTINCT SOI.SaleOrderItemId,
                               JM.JobCardTaskMasterId, JM.JobCardTaskName
                               FROM SaleOrder SO
                               INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                               INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                               INNER JOIN ItemVsTasks BOX ON BOX.ItemId = WD.BoxId
                               INNER JOIN JobCardTaskMaster JM ON JM.JobCardTaskMasterId=BOX.JobCardTaskMasterId
                               )T1

                               select DISTINCT V.VehicleInPassNo,C.CustomerName,V.ChassisNo,V.RegistrationNo,I.ItemName FreezerUnitName,I.ItemName BoxName,S.isService,
                               STUFF((SELECT ', ' + CAST(I.ItemName AS VARCHAR(MAX)) [text()]
                               FROM Item I inner join SaleOrderMaterial SM on I.ItemId=SM.ItemId
                               WHERE SM.SaleOrderId = S.SaleOrderId
                               FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ')  Accessories,J.JobCardNo,VM.VehicleModelName,S.EDateDelivery,
                               Task = case when S.isService =1 then 
                               
                               STUFF((SELECT ', ' + CAST(JM.JobCardTaskName AS VARCHAR(MAX)) [text()]
                               FROM WorkVsTask WD inner join JobCardTaskMaster JM on JM.JobCardTaskMasterId=WD.JobCardTaskMasterId
                               WHERE WD.WorkDescriptionId = W.WorkDescriptionId
                               FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ')
                               ELSE  
                               STUFF((SELECT ', ' + CAST(T.JobCardTaskName AS VARCHAR(MAX)) [text()]
                               FROM #TEMP1 T inner join JobCardTaskMaster J on T.JobCardTaskMasterId=J.JobCardTaskMasterId
                               WHERE SI.SaleOrderItemId = T.SaleOrderItemId
                               FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ')
                               END,
                               case when D.JobCardId is not null then 'Delivered'
                               when Isnull(J.JodCardCompleteStatus,0)=1  then 'Completed'
                               when Isnull(J.JodCardCompleteStatus,0)=0 and JA.JobCardId is not null   then 'Work in Progress'  
                               when Isnull(J.JodCardCompleteStatus,0)=0 and JA.JobCardId is null   then 'Work not Started' 
                               when J.InPassId is null then 'Pending JC Creation'
                               end as Status 
                               INTO #TEMP2       
                               from VehicleInPass V
                               INNER JOIN  SaleOrder S on V.SaleOrderId=S.SaleOrderId
                               INNER JOIN SaleOrderItem SI on SI.SaleOrderId=S.SaleOrderId
                               INNER JOIN Customer C on C.CustomerId=S.CustomerId
                               INNER JOIN WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                               INNER JOIN VehicleModel VM on VM.VehicleModelId= SI.VehicleModelId
                               LEFT JOIN JobCard J on J.InPassId=V.VehicleInPassId
                               LEFT JOIN JobCardDailyActivity JA ON JA.JobCardId=J.JobCardId
                               LEFT JOIN DeliveryChallan D ON D.JobCardId=J.JobCardId
                               LEFT JOIN Item I ON I.ItemId=W.FreezerUnitId
                               LEFT JOIN Item IT ON IT.ItemId=W.BoxId
                               LEFT JOIN #TEMP1 T ON T.SaleOrderItemId=SI.SaleOrderItemId

                               SELECT * INTO #TEMP3 FROM  #TEMP2 
                               SELECT * FROM #TEMP3
                               WHERE ISNULL(#TEMP3.isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(#TEMP3.isService, 0) END
                               AND ISNULL(#TEMP3.CustomerName,'') LIKE '%'+@CustomerName+'%'  
                               AND (ISNULL(#TEMP3.ChassisNo, '') LIKE '%'+@RegNo+'%' OR ISNULL(#TEMP3.RegistrationNo, '') LIKE '%'+@RegNo+'%')
                               AND ISNULL(#TEMP3.Status,'') LIKE '%'+@status+'%'";


                return connection.Query<VehicleInPass>(sql, new { InstallType = InstallType, CustomerName = CustomerName, RegNo = RegNo, status = status }).ToList();
            }
        }
    }
}