using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class ProjectCompletionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable PendingForCompletion(int OrganizationId, string saleorder = "")
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT 
	                                    SO.SaleOrderId,
	                                    SO.SaleOrderRefNo,
	                                    CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                    C.CustomerName,
	                                    SO.TotalAmount,
	                                    CONVERT(VARCHAR, SO.EDateDelivery, 106) EDateDelivery
                                    FROM SaleOrder SO
	                                    INNER JOIN Customer C ON SO.CustomerId = C.CustomerId
										LEFt JOIN ProjectCompletion PC ON SO.SaleOrderId = PC.SaleOrderId
                                    WHERE SO.isProjectBased = 1
	                                    AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'
	                                    AND SO.SaleOrderApproveStatus = 1
	                                    AND SO.OrganizationId = @OrganizationId
	                                    AND SO.isActive = 1
	                                    AND SO.SaleOrderRefNo LIKE '%'+@saleorder+'%'
										AND PC.SaleOrderId IS NULL
                                    ORDER BY SO.EDateDelivery DESC, SO.CreatedDate DESC";
                    return connection.Query<SaleOrder>(query, new { OrganizationId = OrganizationId, saleorder = saleorder }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //        public JobCardCompletion GetJobCardDetails(int SaleOrderId)
        //        {
        //            try
        //            {
        //                using (IDbConnection connection = OpenConnection(dataConnection))
        //                {
        //                    string query = @"SELECT 
        //	                                    T1.EmployeeId,
        //	                                    SUM(T1.ActualHours) TotalHours
        //                                    INTO #TOTAL
        //                                    FROM JobCardDailyActivityTask T1
        //	                                    INNER JOIN JobCardDailyActivity T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
        //							            INNER JOIN JobCard T3 ON T2.JobCardId = T3.JobCardId
        //                                    WHERE T3.SaleOrderId = @SaleOrderId
        //                                    GROUP BY T1.EmployeeId
        //
        //                                    SELECT DISTINCT
        //							            JC.JobCardNo,
        //	                                    M.JobCardTaskName,
        //							            JT.SlNo,
        //	                                    EMP.EmployeeName,
        //	                                    DAT.JobCardTaskId,
        //	                                    ISNULL(T.TotalHours, 0) ActualHours,
        //							            JT.Hours
        //	                                    --(SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity /*WHERE JobCardId = @JobCardId*/ ORDER BY JobCardDailyActivityDate) StartDate,
        //	                                    --(SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity /*WHERE JobCardId = @JobCardId*/ ORDER BY JobCardDailyActivityDate DESC) EndDate
        //                                    FROM JobCardTask JT
        //	                                    INNER JOIN JobCardTaskMaster M ON JT.JobCardTaskMasterId = M.JobCardTaskMasterId
        //	                                    LEFT JOIN JobCardDailyActivity DA ON JT.JobCardId = DA.JobCardId
        //	                                    LEFT JOIN JobCardDailyActivityTask DAT ON DA.JobCardDailyActivityId = DAT.JobCardDailyActivityId AND M.JobCardTaskMasterId = DAT.JobCardTaskId
        //	                                    LEFT JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
        //	                                    LEFT JOIN #TOTAL T ON EMP.EmployeeId = T.EmployeeId
        //							            INNER JOIN JobCard JC ON DA.JobCardId = JC.JobCardId
        //                                    --WHERE JT.JobCardId = @JobCardId;
        //						            WHERE JC.SaleOrderId = @SaleOrderId
        //                                    DROP TABLE #TOTAL;";
        //                    JobCardCompletion j = new JobCardCompletion();
        //                    j.JobCardTask = connection.Query<JobCardCompletionTask>(query, new { SaleOrderId = SaleOrderId }).ToList();
        //                    return j;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }

        public IEnumerable<ItemBatch> GetSerialNos(int SaleOrderId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT
	                                IB.ItemBatchId,
	                                I.ItemName,
	                                SerialNo
                                FROM JobCard JC
                                INNER JOIN ItemBatch IB ON JC.SaleOrderItemId = IB.SaleOrderItemId
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                INNER JOIN Item I ON GI.ItemId = I.ItemId
                                WHERE JC.SaleOrderId = @SaleOrderId";
                    return connection.Query<ItemBatch>(query, new { SaleOrderId = SaleOrderId }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string InsertProjectCompletion(ProjectCompletion model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"INSERT INTO ProjectCompletion
                                    (
                                        ProjectCompletionRefNo,
	                                    ProjectCompletionDate,
	                                    ChillerTemperature,
	                                    ChillerDimension,
	                                    ChillerCondensingUnit,
	                                    ChillerEvaporator,
	                                    ChillerRefrigerant,
	                                    ChillerQuantity,
	                                    FreezerTemperature,
	                                    FreezerDimension,
	                                    FreezerCondensingUnit,
	                                    FreezerEvaporator,
	                                    FreezerRefrigerant,
	                                    FreezerQuantity,
	                                    SaleOrderId,
	                                    OrganizationId,
	                                    CreatedDate,
	                                    CreatedBy,
                                        isActive
                                    )
                                    VALUES
                                    (
                                        @ProjectCompletionRefNo,
	                                    @ProjectCompletionDate,
	                                    @ChillerTemperature,
	                                    @ChillerDimension,
	                                    @ChillerCondensingUnit,
	                                    @ChillerEvaporator,
	                                    @ChillerRefrigerant,
	                                    @ChillerQuantity,
	                                    @FreezerTemperature,
	                                    @FreezerDimension,
	                                    @FreezerCondensingUnit,
	                                    @FreezerEvaporator,
	                                    @FreezerRefrigerant,
	                                    @FreezerQuantity,
	                                    @SaleOrderId,
	                                    @OrganizationId,
	                                    @CreatedDate,
	                                    @CreatedBy,
                                        1
                                    );
                                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    model.ProjectCompletionRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 30, true, txn);
                    model.ProjectCompletionId = connection.Query<int>(query, model, txn).First();
                    InsertItemBatch(model, connection, txn);
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", typeof(ProjectCompletion).Name, model.ProjectCompletionId.ToString(), model.OrganizationId.ToString());
                    txn.Commit();
                    return model.ProjectCompletionRefNo;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public int InsertItemBatch(ProjectCompletion model, IDbConnection connection, IDbTransaction txn)
        {
            string query = "UPDATE ItemBatch SET ProjectCompletionId = " + model.ProjectCompletionId + @", WarrantyStartDate = @WarrantyStartDate, WarrantyExpireDate = @WarrantyExpireDate
                             WHERE ItemBatchId = @ItemBatchId";
            int id = 0;
            foreach (var item in model.ItemBatches)
            {
                id = connection.Execute(query, item, txn);
                if (id == 0) throw new Exception("Updated row count returned as 0");
            }
            return id;
        }

        public ProjectCompletion GetProjectDetails(int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT 
	                                    ProjectName,
	                                    ColdRoomLocation Location,
	                                    C.CustomerName
                                    FROM QuerySheet QS
	                                    INNER JOIN SalesQuotation SQ ON QS.QuerySheetId = SQ.QuerySheetId
	                                    INNER JOIN SaleOrder SO ON SQ.SalesQuotationId = SO.SalesQuotationId
	                                    INNER JOIN Customer C ON SQ.CustomerId = C.CustomerId
                                    WHERE SO.SaleOrderId = @SaleOrderId";
                    return connection.Query<ProjectCompletion>(query, new { SaleOrderId = id }).First();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IEnumerable GetPreviousList(string project, string saleorder, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT 
	                                    PC.ProjectCompletionId,
	                                    PC.ProjectCompletionRefNo,
	                                    CONVERT(VARCHAR, PC.ProjectCompletionDate, 106) ProjectCompletionDate,
	                                    SO.SaleOrderRefNo,
	                                    CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                    C.CustomerName
                                    FROM ProjectCompletion PC
	                                    INNER JOIN SaleOrder SO ON PC.SaleOrderId = So.SaleOrderId
	                                    INNER JOIN Customer C ON SO.CustomerId = C.CustomerId
                                    WHERE PC.OrganizationId = @OrganizationId AND PC.isActive = 1
                                    AND SO.SaleOrderRefNo LIKE '%'+@saleorder+'%'
                                    AND PC.ProjectCompletionRefNo LIKE '%'+@project+'%'
                                    ORDER BY PC.ProjectCompletionDate DESC, PC.CreatedDate DESC";
                    return connection.Query<ProjectCompletion>(query, new { OrganizationId = OrganizationId, project = project, saleorder = saleorder }).ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public ProjectCompletion GetProjectCompletion(int ProjectCompletionId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT
	                                    PC.*,
                                        SO.SaleOrderId,
	                                    SO.SaleOrderRefNo,
	                                    SO.SaleOrderDate,
	                                    C.CustomerName,
	                                    QS.ProjectName,
	                                    QS.ColdRoomLocation Location
                                    FROM ProjectCompletion PC
	                                    INNER JOIN SaleOrder SO ON PC.SaleOrderId = SO.SaleOrderId
	                                    INNER JOIN Customer C ON SO.CustomerId = C.CustomerId
	                                    INNER JOIN SalesQuotation SQ ON SO.SalesQuotationId = SQ.SalesQuotationId
	                                    INNER JOIN QuerySheet QS ON SQ.QuerySheetId = QS.QuerySheetId 
                                        WHERE ProjectCompletionId = @ProjectCompletionId AND PC.OrganizationId = @OrganizationId";
                    return connection.Query<ProjectCompletion>(query, new { OrganizationId = OrganizationId, ProjectCompletionId = ProjectCompletionId }).First();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public List<ItemBatch> GetSerialNosByProjectCompletioId(int ProjectCompletionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT
	                                    IB.SerialNo,
	                                    I.ItemName,
	                                    CONVERT(VARCHAR, IB.WarrantyStartDate, 106) WarrantyStartDate,
	                                    CONVERT(VARCHAR, IB.WarrantyExpireDate, 106) WarrantyExpireDate
                                    FROM ItemBatch IB
	                                    INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
	                                    INNER JOIN Item I ON GI.ItemId = I.ItemId
                                    WHERE IB.ProjectCompletionId = @ProjectCompletionId";
                    return connection.Query<ItemBatch>(query, new { ProjectCompletionId = ProjectCompletionId }).ToList();
                }
                catch
                {
                    return new List<ItemBatch>();
                }
            }
        }
    }
}
