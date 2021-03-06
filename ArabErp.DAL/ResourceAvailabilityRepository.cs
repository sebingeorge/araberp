﻿using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class ResourceAvailabilityRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public List<EmployeeResourceAvailability> GetEmployeeAvailability(int month, int year, int isProject, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                                EMP.EmployeeId,
	                                EMP.EmployeeName,
									EC.EmpCategoryName,
	                                CONVERT(VARCHAR, JT.TaskDate, 106) TaskDate,
	                                JC.JobCardNo,
	                                --CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                JTM.JobCardTaskName,
	                                JC.isProjectBased,
									JT.[Hours]
                                FROM JobCard JC
	                                INNER JOIN JobCardTask JT ON JC.JobCardId = JT.JobCardId
	                                INNER JOIN JobCardTaskMaster JTM ON JT.JobCardTaskMasterId = JTM.JobCardTaskMasterId
	                                RIGHT JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
									LEFT JOIN EmployeeCategory EC ON EMP.CategoryId = EC.EmpCategoryId
                                WHERE ISNULL(JC.JodCardCompleteStatus, 0) = 0
                                AND MONTH(JT.TaskDate) = ISNULL(@month, MONTH(GETDATE()))
                                AND YEAR(JT.TaskDate) = ISNULL(@year, MONTH(GETDATE()))
                                AND JC.OrganizationId = @OrganizationId
                                AND JC.isProjectBased = @isProject
                                ORDER BY EMP.EmployeeName;";

                return connection.Query<EmployeeResourceAvailability>(query, new { month = month, year = year, isProject = isProject, OrganizationId = OrganizationId }).ToList();
            }
        }
    }
}
