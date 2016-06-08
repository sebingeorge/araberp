using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class BayStatusReportRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<BayStatus> GetBayStatusReport()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " with Job as (select distinct JobCardId, JobCardDate, JobCardNo, BayId, SaleOrderId,SaleOrderItemId, ";
                sql += " ISNULL(JodCardCompleteStatus,0) Complete, WorkDescriptionId, InPassId from JobCard where ISNULL(JodCardCompleteStatus,0) = 0) ";
                sql += " select distinct B.BayId, B.BayName, Occupied = case when Job.Complete IS NULL then 'No' else 'Yes' end, Job.JobCardDate, ";
                sql += " Job.JobCardNo, I.RegistrationNo ChasisNoRegNo, V.VehicleModelName, UnitName,W.WorkDescr WorkDescription, SO.EDateDelivery, ";
                sql += " C.CustomerName, SO.EDateArrival ";
                sql += " from Bay B left join Job on B.BayId = Job.BayId ";
                sql += " left join JobCardTask JT on JT.JobCardId = Job.JobCardId ";
                sql += " left join SaleOrderItem SI on SI.SaleOrderItemId = Job.SaleOrderItemId ";
                sql += " left join SaleOrder SO on SO.SaleOrderId = SI.SaleOrderId ";
                sql += " left join VehicleModel V on V.VehicleModelId = SI.VehicleModelId ";
                sql += " left join Unit U on U.UnitId = SI.UnitId left join Customer C on C.CustomerId = SO.CustomerId";
                sql += " left join VehicleInPass I on I.VehicleInPassId = Job.InPassId";
                sql += " left join WorkDescription W on W.WorkDescriptionId = Job.WorkDescriptionId";


                return connection.Query<BayStatus>(sql);
            } 
        }
    }
}
