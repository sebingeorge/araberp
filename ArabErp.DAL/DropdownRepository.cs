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
        /// Return all active Suppliers
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> SupplierDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT SupplierId Id,SupplierName Name FROM Supplier WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all active Suppliers which in grn
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> GrnSupplierDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT distinct S.SupplierId Id,SupplierName Name FROM Supplier S INNER JOIN GRN G ON G.SupplierId=S.SupplierId WHERE ISNULL(S.isActive, 1) = 1").ToList();
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
        /// <summary>
        /// Returns all suppliers who have pending/incomplete supply order(s)
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> SupplierDropdown1()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT DISTINCT S.SupplierId Id, S.SupplierName Name FROM SupplyOrderItem SOI
                                                    INNER JOIN SupplyOrder SO ON SOI.SupplyOrderId = SO.SupplyOrderId
                                                    INNER JOIN Supplier S ON SO.SupplierId = S.SupplierId
                                                    LEFT JOIN GRNItem GI ON SOI.SupplyOrderItemId = GI.SupplyOrderItemId
                                                    WHERE GI.SupplyOrderItemId IS NULL OR GI.Quantity < SOI.OrderedQty").ToList();
            }
        }
        /// <summary>
        /// Return all active Payment Terms
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> PaymentTermsDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT PaymentTermsId Id, PaymentTermsName Name FROM PaymentTerms WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Return all additions
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> AdditionDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT AddDedId Id, AddDedName Name FROM AdditionDeduction WHERE AddDedType = 1").ToList();
            }
        }
        /// <summary>
        /// Return all deductions
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> DeductionDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT AddDedId Id, AddDedName Name FROM AdditionDeduction WHERE AddDedType = 2").ToList();
            }
        }
        /// <summary>
        /// Return all Active and Approved QuotationNo
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> QuotationNoDropdown(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT SalesQuotationId Id, QuotationRefNo Name FROM SalesQuotation WHERE ISNULL(isActive, 1) = 1 
                AND ISNULL(IsQuotationApproved,0)=1 and isProjectBased = " + isProjectBased.ToString() + " AND SalesQuotationId not in (select SalesQuotationId from SaleOrder where SalesQuotationId is not null)").ToList();
            }
        }
        /// <summary>
        /// Return Quotation number in Sale Order
        /// </summary>
        /// <param name="isProjectBased"></param>
        /// <returns></returns>
        public List<Dropdown> QuotationInSaleOrderDropdown(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>(@"SELECT  S.SalesQuotationId Id, QuotationRefNo Name FROM SaleOrder S
                                           INNER JOIN SalesQuotation SQ ON S.SalesQuotationId=SQ.SalesQuotationId WHERE S.isProjectBased = " + isProjectBased.ToString() + "").ToList();
            }
        }

    }
}
