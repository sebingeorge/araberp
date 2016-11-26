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
    public class MISReportsRepository : BaseRepository
    {
        string dataConnection = GetConnectionString("arab");
        public IEnumerable GetDCReport(int? month, int? year, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT 
	                                    JobCardId,
	                                    CAST(SUM(EMP.HourlyCost * ISNULL(JCT.ActualHours, 0)) AS DECIMAL(18,2)) LabourCost
                                    INTO #HourlyCost
                                    FROM JobCardTask JCT
                                    INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                                    GROUP BY JobCardId

                                    SELECT
	                                    JC.JobCardNo,
	                                    CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                    CUS.CustomerName,
	                                    VIP.RegistrationNo,
	                                    VIP.ChassisNo,
	                                    DC.DeliveryChallanRefNo,
	                                    CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
	                                    SI.SalesInvoiceRefNo InvoiceNo,
	                                    CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) InvoiceDate,
	                                    SI.TotalAmount Amount,
	                                    BOX.ItemName BoxName,
	                                    FREEZER.ItemName FreezerName,
	                                    IB.SerialNo UnitSerialNo,
	                                    CASE WHEN ISNULL(JC.isService, 0) = 1 THEN 'Service' ELSE 'New Installation' END InstallationType,
	                                    HC.LabourCost
                                    FROM JobCard JC
                                    INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
                                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                                    INNER JOIN VehicleInPass VIP ON JC.InPassId = VIP.VehicleInPassId
                                    LEFT JOIN DeliveryChallan DC ON JC.JobCardId = DC.JobCardId
                                    LEFT JOIN SalesInvoice SI ON JC.SaleOrderId = SI.SaleOrderId
                                    LEFT JOIN Item BOX ON JC.BoxId = BOX.ItemId
                                    LEFT JOIN Item FREEZER ON JC.FreezerUnitId = FREEZER.ItemId
                                    LEFT JOIN ItemBatch IB ON JC.SaleOrderItemId = IB.SaleOrderItemId
                                    LEFT JOIN #HourlyCost HC ON JC.JobCardId = HC.JobCardId
                                    WHERE JC.OrganizationId = @org
									AND MONTH(JC.JobCardDate) = ISNULL(@month, MONTH(GETDATE())) 
									AND YEAR(JC.JobCardDate) = ISNULL(@year, YEAR(GETDATE()))

                                    DROP TABLE #HourlyCost";
                    return connection.Query<DCReport>(query, new { org = OrganizationId, month = month, year = year });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
