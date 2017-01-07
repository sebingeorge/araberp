using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class ServiceRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<ServiceRegister> GetServiceRegister(string Customer)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT C.CustomerName,VM.VehicleModelName,
                                CASE WHEN J.isService=1 THEN'Delivered' ELSE 'New Vechile' END SERVICE,
                                ISNULL(VI.RegistrationNo,'-')RegistrationNo,
                                CASE WHEN S.isService=0 THEN I.ItemName
                                ELSE SE.FreezerModel END Unit,
                                D.DeliveryChallanDate LASTSERVICE,
                                DATEADD(m,3,D.DeliveryChallanDate )NEXTESERVICE
                                FROM JobCard J
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                INNER JOIN SaleOrderItem SI ON SI.SaleOrderItemId=J.SaleOrderItemId
                                INNER JOIN VehicleModel VM ON VM.VehicleModelId=SI.VehicleModelId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
                                INNER JOIN VehicleInPass VI ON VI.SaleOrderId=S.SaleOrderId
                                INNER JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
                                INNER JOIN DeliveryChallan D ON D.JobCardId=J.JobCardId
                                LEFT JOIN Item I ON I.ItemId=W.FreezerUnitId
                                LEFT JOIN ServiceEnquiry SE ON SE.ServiceEnquiryId=S.ServiceEnquiryId
                                WHERE isnull(C.CustomerName,'') like '%'+@Customer+'%'
                                ORDER BY DATEADD(m,3,D.DeliveryChallanDate ) Asc";

                return connection.Query<ServiceRegister>(qry, new { Customer = Customer }).ToList();
            }
        }
    }
}
