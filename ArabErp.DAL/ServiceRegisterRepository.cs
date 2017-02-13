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

        public IEnumerable<ServiceRegister> GetMaintenanceRegisterData(DateTime? from, DateTime? to, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//              
                string qry = @"SELECT ServiceEnquiryRefNo,CONVERT(VARCHAR(10),ServiceEnquiryDate,(105))ServiceEnquiryDate,
                                CONVERT(Varchar(10),SE.CreatedDate,108)Time,
                                CONCAT(OrganizationName,' & ',O.Street)CompanyAddress,CONCAT(C.CustomerName,' & ',C.Phone)CustomerName,
                                Complaints,U.UserName
                                FROM ServiceEnquiry SE
                                INNER JOIN Organization O ON O.OrganizationId=SE.OrganizationId
                                INNER JOIN Customer C ON C.CustomerId=SE.CustomerId
                                INNER JOIN [User] U ON U.UserId=SE.CreatedBy
                                WHERE ServiceEnquiryDate BETWEEN  @from  AND @to  AND SE.OrganizationId=@OrganizationId
                                ORDER BY ServiceEnquiryDate";
                return connection.Query<ServiceRegister>(qry, new { from = from, to = to, OrganizationId = OrganizationId }).ToList();
            }
        }
    }

  }

