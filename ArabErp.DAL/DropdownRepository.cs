using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class DropdownRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Return all job cards waiting for completion
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> JobCardDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT JobCardId Id, JobCardNo Name FROM JobCard WHERE ISNULL(JodCardCompleteStatus, 0) = 0 AND ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all items that are active
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> ItemDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT ItemId Id, ItemName Name FROM Item WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all active employees
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> EmployeeDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT EmployeeId Id, EmployeeName Name FROM Employee WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all stockpoints
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> StockpointDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT StockPointId Id, StockPointName Name FROM Stockpoint WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all incomplete sale orders for vehicle in-pass (orders that are not entered in vehicle in-pass)
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public List<Dropdown> SaleOrderDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1;
                    SELECT SaleOrderId, SaleOrderItemId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT SaleOrderItemId INTO #VEHICLE_INPASS FROM VehicleInPass WHERE ISNULL(isActive, 1) = 1;
                    SELECT CustomerId, CustomerName INTO #CUS FROM Customer WHERE ISNULL(isActive, 1) = 1;

                    SELECT DISTINCT(SO.SaleOrderId) Id, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SO.SaleOrderDate, 106) + ' - ' + C.CustomerName Name FROM #SALE SO 
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #VEHICLE_INPASS VI ON SOI.SaleOrderItemId = VI.SaleOrderItemId
                    LEFT JOIN #CUS C ON SO.CustomerId = C.CustomerId
                    WHERE VI.SaleOrderItemId IS NULL;

                    DROP TABLE #SALE;
                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #VEHICLE_INPASS;
                    DROP TABLE #CUS;").ToList();
            }
        }
        /// <summary>
        /// Return all customers who have incomplete sale order
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> CustomerDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT SaleOrderId, CustomerId INTO #SALE FROM SaleOrder WHERE ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1;
                    SELECT SaleOrderId, SaleOrderItemId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT SaleOrderItemId INTO #VEHICLE_INPASS FROM VehicleInPass WHERE ISNULL(isActive, 1) = 1;
                    SELECT CustomerId, CustomerName INTO #CUS FROM Customer WHERE ISNULL(isActive, 1) = 1;

                    SELECT DISTINCT(SO.CustomerId) Id, C.CustomerName Name FROM #SALE SO 
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #VEHICLE_INPASS VI ON SOI.SaleOrderItemId = VI.SaleOrderItemId
                    LEFT JOIN #CUS C ON SO.CustomerId = C.CustomerId
                    WHERE VI.SaleOrderItemId IS NULL;

                    DROP TABLE #SALE;
                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #VEHICLE_INPASS;
                    DROP TABLE #CUS;").ToList();
            }
        }
        /// <summary>
        /// Returns all vechile in-pass registration number that are not in out-pass
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> VehicleInPassDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT SaleOrderItemId INTO #OUT_PASS FROM VehicleOutPass VO
                        INNER JOIN JobCard JC ON VO.JobCardId = JC.JobCardId;
                        SELECT VI.VehicleInPassId Id, RegistrationNo Name FROM VehicleInPass VI
                        LEFT JOIN #OUT_PASS OP ON VI.SaleOrderItemId = OP.SaleOrderItemId
                        WHERE OP.SaleOrderItemId IS NULL;
                        DROP TABLE #OUT_PASS;").ToList();
            }
        }
        /// <summary>
        /// Fill dropdown of item with with StockPointId Param
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> StockJournelItemsDropdown(int? StockPointId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"select ItemId Id,ItemName Name from item where ItemId in(select DISTINCT ItemId from StockUpdate where StockPointId=@StockPointId)", new { StockPointId = StockPointId }).ToList();
            }
        }
       
    }
}
