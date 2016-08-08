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

namespace ArabErp.DAL
{
    public class JobOrderCompletionRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<JobOrderPending> GetPendingJobOrder(int? isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if ((isProjectBased ?? 0) == 0)
                {
                    query += " select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, V.VehicleModelName";
                    query += " from JobCard J";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId";
                    query += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId ";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " inner join VehicleModel V on V.VehicleModelId = W.VehicleModelId ";
                    query += " where ISNULL(J.JodCardCompleteStatus,0) <> 1 and J.isProjectBased = 0";
                }
                else
                {
                    query += " select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, '' VehicleModelName";
                    query += " from JobCard J";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId";
                    query += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId ";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " where ISNULL(J.JodCardCompleteStatus,0) <> 1 and J.isProjectBased = 1";
                }
                
                
                return connection.Query<JobOrderPending>(query);
            }
        }

        public JobCardCompletion GetJobCardCompletion(int JobCardId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if (isProjectBased == 0)
                {
                    query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,";
                    query += " V.VehicleModelId, V.VehicleModelName, W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks";
                    query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " inner join VehicleModel V on V.VehicleModelId = W.VehicleModelId";
                    query += " where J.JobCardId = " + JobCardId.ToString() + "  and J.isProjectBased = 0";
                }
                else
                {
                    query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,";
                    query += " 0 VehicleModelId, '' VehicleModelName, W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks";
                    query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " where J.JobCardId = " + JobCardId.ToString() + " and J.isProjectBased = 1";
                }
                
                var jobcard = connection.Query<JobCardCompletion>(query).FirstOrDefault();

                query = string.Empty;
                query = "select J.SlNo, JT.JobCardTaskMasterId, JT.JobCardTaskName, E.EmployeeId, E.EmployeeName, J.TaskDate, 0 ActualHours, 0 Existing";
                query += " from JobCardTask J inner join JobCardTaskMaster JT on J.JobCardTaskMasterId = JT.JobCardTaskMasterId";
                query += " inner join Employee E on E.EmployeeId = J.EmployeeId";
                query += " where J.JobCardId = " + JobCardId.ToString();

                var tasks = connection.Query<JobCardCompletionTask>(query);

                jobcard.JobCardTask = new List<JobCardCompletionTask>();

                foreach (JobCardCompletionTask item in tasks)
                {
                    jobcard.JobCardTask.Add(item);
                }
                return jobcard;
            }
        }

        public int UpdateJobCardCompletion(JobCardCompletion jobcard, string CreatedBy)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "update JobCard set JodCardCompleteStatus = 1, JodCardCompletedDate='" + jobcard.JobCardCompletedDate.ToString("dd-MMM-yyyy") + "', WarrentyPeriod = '"+ jobcard.WarrentyPeriod.ToString("dd/MMM/yyyy") +"' where jobCardId=" + jobcard.JobCardId.ToString();
                connection.Query(query);
                int i = 0;
                foreach (var item in jobcard.JobCardTask)
                {
                    if(item.Existing == 1)
                    {
                        query = string.Empty;
                        query = @"insert  into JobCardTask(JobCardId,JobCardTaskMasterId,SlNo,EmployeeId,TaskDate,Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values 
                        (" + jobcard.JobCardId.ToString() + "," + item.JobCardTaskMasterId + "," + i.ToString() + "," + item.EmployeeId.ToString() + "," + item.TaskDate.ToString("dd/MMM/yyyy") + "," + item.ActualHours + "," + item.ActualHours + ",NULL,GETDATE(),NULL); SELECT CAST(SCOPE_IDENTITY() as int)";
                        connection.Query(query);
                        i++;
                    }
                    else
                    {
                        query = string.Empty;
                        query = "update JobCardTask set ActualHours = " + item.ActualHours.ToString() + " where SlNo = " + item.SlNo.ToString() + " and JobCardId = " + jobcard.JobCardId.ToString() + ";";
                        connection.Query(query);
                    }
                }
                InsertLoginHistory(dataConnection, CreatedBy, "Update", "Job Card Completion", id.ToString(), "0");
            }
            return id;
        }
    }
}
