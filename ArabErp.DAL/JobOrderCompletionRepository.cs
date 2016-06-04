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

namespace ArabErp.DAL
{
    public class JobOrderCompletionRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<JobOrderPending> GetPendingJobOrder()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, V.VehicleModelName";
                query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                query += " inner join Customer C on S.CustomerId = C.CustomerId";
                query += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";

                return connection.Query<JobOrderPending>(query);
            }
        }

        public JobCardCompletion GetJobCardCompletion(int JobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,";
                query += " V.VehicleModelId, V.VehicleModelName, W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks";
                query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                query += " inner join Customer C on S.CustomerId = C.CustomerId";
                query += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                query += " inner join WorkDescription W on SI.WorkDescriptionId = W.WorkDescriptionId";
                query += " where J.JodCardCompletionStatus <> 1 and J.JobCardId = " + JobCardId.ToString();
                var jobcard = connection.Query<JobCardCompletion>(query).FirstOrDefault();

                query = string.Empty;
                query = "select J.SlNo, T.TaskId, T.TaskName, E.EmployeeId, E.EmployeeName, J.TaskDate, 0 ActualHours, 0 Existing";
                query += " from JobCardTask J inner join Task T on J.JobCardTaskId = T.TaskId";
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

        public int UpdateJobCardCompletion(JobCardCompletion jobcard)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "update JobCard set JodCardCompletionStatus = 1, JodCardCompletedDate="+ jobcard.JobCardCompletedDate.ToString("dd-MMM-yyyy") +" where jobCardId=" + jobcard.JobCardId.ToString();
                connection.Query(query);
                int i = 0;
                foreach (var item in jobcard.JobCardTask)
                {
                    if(item.Existing == 1)
                    {
                        query = string.Empty;
                        query = @"insert  into JobCardTask(JobCardId,JobCardTaskMasterId,SlNo,EmployeeId,TaskDate,Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values 
                        (" + jobcard.JobCardId.ToString() + "," + item.TaskId + "," + i.ToString() + "," + item.EmployeeId.ToString() + "," + item.TaskDate.ToString("dd/MMM/yyyy") + "," + item.ActualHours + "," + item.ActualHours + ",NULL,GETDATE(),NULL); SELECT CAST(SCOPE_IDENTITY() as int)";
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
            }
            return id;
        }
    }
}
