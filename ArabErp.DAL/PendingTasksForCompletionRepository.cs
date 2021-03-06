﻿using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class PendingTasksForCompletionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable GetPendingTasks(int OrganizationId, string saleorder = "", string jobcard = "", string jobcarddate = "",
                                           string engineer = "", string task = "", string technician = "", string InstallType="")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                DateTime d;
                try
                {
                    d = Convert.ToDateTime(jobcarddate);
                }
                catch (Exception)
                {
                    d = DateTime.MinValue;
                }
                string query = @"
               
                SELECT
	                JC.JobCardId,
	                JobCardNo,
	                CONVERT(VARCHAR, JobCardDate, 106) JobCardDate,
	                SO.SaleOrderRefNo,
	                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                --SaleOrderItemId,
	                VP.RegistrationNo,
	                WD.WorkDescr,
	                --I1.ItemName,
	                --I2.ItemName,
	                Bay.BayName,
	                CONVERT(VARCHAR, RequiredDate, 106) RequiredDate,
	                EMP1.EmployeeName Engineer,
	                JC.isProjectBased,

	                JT.[Hours] EstimatedHours,
	                CONVERT(VARCHAR, JT.TaskDate, 106) TaskDate,

	                JTM.JobCardTaskName,
	                EMP.EmployeeName,C.CustomerName,isnull(VI.ChassisNo,'')ChassisNo,isnull(VI.RegistrationNo,'')RegistrationNo

                     FROM JobCard JC
	                INNER JOIN JobCardTask JT ON JC.JobCardId = JT.JobCardId
	                INNER JOIN JobCardTaskMaster JTM ON JT.JobCardTaskMasterId = JTM.JobCardTaskMasterId
	                INNER JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
	                INNER JOIN Employee EMP1 ON JC.EmployeeId = EMP1.EmployeeId
	                INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
	                LEFT JOIN VehicleInPass VP ON JC.InPassId = VP.VehicleInPassId
	                INNER JOIN WorkDescription WD ON JC.WorkDescriptionId = WD.WorkDescriptionId
	                LEFT JOIN Item I1 ON JC.FreezerUnitId = I1.ItemId
	                LEFT JOIN Item I2 ON JC.BoxId = I2.ItemId
	                LEFT JOIN Bay ON JC.BayId = Bay.BayId
					Left JOIN Customer C ON C.CustomerId=SO.CustomerId
					Left JOIN VehicleInPass VI ON VI.VehicleInPassId=JC.InPassId
                    WHERE ISNULL(JC.JodCardCompleteStatus, 0) = 0
	                AND	JC.OrganizationId = @OrganizationId
					AND SO.SaleOrderRefNo LIKE '%'+@saleorder+'%'
					AND JC.JobCardNo LIKE '%'+@jobcard+'%' " +
                    (d != DateTime.MinValue ? "AND JC.JobCardDate = ISNULL(@jobcarddate, JC.JobCardDate) " : "") +
                    @"AND EMP1.EmployeeName LIKE '%'+@engineer+'%'
					AND JTM.JobCardTaskName LIKE '%'+@task+'%'
					AND EMP.EmployeeName LIKE '%'+@technician+'%'
                    AND  ISNULL(JC.isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(JC.isService, 0) END
	            ORDER BY SO.SaleOrderRefNo, JC.RequiredDate DESC";

                return connection.Query<PendingTasksForCompletion>(query, new
                {
                    OrganizationId = OrganizationId,
                    saleorder = saleorder,
                    jobcard = jobcard,
                    jobcarddate = d,
                    engineer = engineer,
                    task = task,
                    technician = technician,
                    InstallType=InstallType
                }).ToList();
            }
        }

        public IEnumerable<PendingTasksForCompletion> GetPendingTasksDTPrint(int OrganizationId, string saleorder = "", string jobcard = "", string jobcarddate = "",
                                           string engineer = "", string task = "", string technician = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                DateTime d;
                try
                {
                    d = Convert.ToDateTime(jobcarddate);
                }
                catch (Exception)
                {
                    d = DateTime.MinValue;
                }
                string query = @"
               SELECT
	                JC.JobCardId,
	                JobCardNo,
	                CONVERT(VARCHAR, JobCardDate, 106) JobCardDate,
	                SO.SaleOrderRefNo,
	                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                --SaleOrderItemId,
	                VP.RegistrationNo,
	                WD.WorkDescr,
	                --I1.ItemName,
	                --I2.ItemName,
	                Bay.BayName,
	                CONVERT(VARCHAR, RequiredDate, 106) RequiredDate,
	                EMP1.EmployeeName Engineer,
	                JC.isProjectBased,

	                JT.[Hours] EstimatedHours,
	                CONVERT(VARCHAR, JT.TaskDate, 106) TaskDate,

	                JTM.JobCardTaskName,
	                EMP.EmployeeName

                FROM JobCard JC
	                INNER JOIN JobCardTask JT ON JC.JobCardId = JT.JobCardId
	                INNER JOIN JobCardTaskMaster JTM ON JT.JobCardTaskMasterId = JTM.JobCardTaskMasterId
	                INNER JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
	                INNER JOIN Employee EMP1 ON JC.EmployeeId = EMP1.EmployeeId
	                INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
	                LEFT JOIN VehicleInPass VP ON JC.InPassId = VP.VehicleInPassId
	                INNER JOIN WorkDescription WD ON JC.WorkDescriptionId = WD.WorkDescriptionId
	                LEFT JOIN Item I1 ON JC.FreezerUnitId = I1.ItemId
	                LEFT JOIN Item I2 ON JC.BoxId = I2.ItemId
	                LEFT JOIN Bay ON JC.BayId = Bay.BayId
                WHERE ISNULL(JC.JodCardCompleteStatus, 0) = 0
	                AND	JC.OrganizationId = @OrganizationId
					AND SO.SaleOrderRefNo LIKE '%'+@saleorder+'%'
					AND JC.JobCardNo LIKE '%'+@jobcard+'%' " +
                    (d != DateTime.MinValue ? "AND JC.JobCardDate = ISNULL(@jobcarddate, JC.JobCardDate) " : "") +
                    @"AND EMP1.EmployeeName LIKE '%'+@engineer+'%'
					AND JTM.JobCardTaskName LIKE '%'+@task+'%'
					AND EMP.EmployeeName LIKE '%'+@technician+'%'
	            ORDER BY SO.SaleOrderRefNo, JC.RequiredDate DESC";

                return connection.Query<PendingTasksForCompletion>(query, new
                {
                    OrganizationId = OrganizationId,
                    saleorder = saleorder,
                    jobcard = jobcard,
                    jobcarddate = d,
                    engineer = engineer,
                    task = task,
                    technician = technician
                }).ToList();
            }
        }
    }
}
