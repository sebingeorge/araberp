using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class VehicleInPassRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertVehicleInPass(VehicleInPass objVehicleInPass)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO VehicleInPass(
                            VehicleInPassNo,
                            SaleOrderItemId,
                            SaleOrderId,
                            RegistrationNo,
                            VehicleInPassDate,
                            EmployeeId,
                            Remarks,
                            CreatedBy,
                            CreatedDate,
                            OrganizationId,
                            isActive)
                         VALUES(
                            @VehicleInPassNo,
                            @SaleOrderItemId,
                            (SELECT SaleOrderId FROM SaleOrderItem WHERE SaleOrderItemId = @SaleOrderItemId),
                            @RegistrationNo,
                            @VehicleInPassDate,
                            @EmployeeId,
                            @Remarks,
                            @CreatedBy,
                            @CreatedDate,
                            @OrganizationId,
                            1
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";

                var id = connection.Query<int>(sql, objVehicleInPass).Single();
                return id;
            }
        }


        public VehicleInPass GetVehicleInPass(int VehicleInPassId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where VehicleInPassId=@VehicleInPassId";

                var objVehicleInPass = connection.Query<VehicleInPass>(sql, new
                {
                    VehicleInPassId = VehicleInPassId
                }).First<VehicleInPass>();

                return objVehicleInPass;
            }
        }

        public List<VehicleInPass> GetVehicleInPasss()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from VehicleInPass
                        where isActive=1";

                var objVehicleInPasss = connection.Query<VehicleInPass>(sql).ToList<VehicleInPass>();

                return objVehicleInPasss;
            }
        }



        public int DeleteVehicleInPass(Unit objVehicleInPass)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete VehicleInPass  OUTPUT DELETED.VehicleInPassId WHERE VehicleInPassId=@VehicleInPassId";


                var id = connection.Execute(sql, objVehicleInPass);
                return id;
            }
        }
        /// <summary>
        /// Return all sale order items that are not in vehicle in-pass
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<PendingSO> PendingVehicleInpass(int customerId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingSO>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE CustomerId = @customerId AND ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1
                    SELECT SaleOrderId, SaleOrderItemId, WorkDescriptionId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT SaleOrderItemId INTO #VEHICLE_INPASS FROM VehicleInPass WHERE ISNULL(isActive, 1) = 1;
                    SELECT WorkDescriptionId, WorkDescr INTO #WORK FROM WorkDescription WHERE ISNULL(isActive, 1) = 1;
                    SELECT VehicleModelId, VehicleModelName, VehicleModelDescription INTO #MODEL FROM VehicleModel WHERE ISNULL(isActive, 1) = 1;

                    SELECT SO.SaleOrderId, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SaleOrderDate, 106) SaleOrderRefNo, SOI.SaleOrderItemId, WorkDescr, VehicleModelName+' - '+VehicleModelDescription VehicleModelName FROM #SALE SO
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #VEHICLE_INPASS VI ON SOI.SaleOrderItemId = VI.SaleOrderItemId
                    LEFT JOIN #WORK W ON SOI.WorkDescriptionId = W.WorkDescriptionId
                    LEFT JOIN #MODEL M ON SOI.VehicleModelId = M.VehicleModelId
                    WHERE VI.SaleOrderItemId IS NULL;

                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #VEHICLE_INPASS;
                    DROP TABLE #MODEL;
                    DROP TABLE #WORK;
                    DROP TABLE #SALE;", new { customerId = customerId}).ToList();
            }
        }
        public PendingSO GetSaleOrderItemDetails(int saleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<PendingSO>(@"SELECT SaleOrderId, SaleOrderRefNo, CustomerId, SaleOrderDate INTO #SALE FROM SaleOrder WHERE ISNULL(isActive, 1) = 1 AND ISNULL(SaleOrderApproveStatus, 0) = 1
                    SELECT SaleOrderId, SaleOrderItemId, WorkDescriptionId, VehicleModelId INTO #SALE_ITEM FROM SaleOrderItem WHERE ISNULL(isActive, 1) = 1;
                    SELECT WorkDescriptionId, WorkDescr INTO #WORK FROM WorkDescription WHERE ISNULL(isActive, 1) = 1;
                    SELECT VehicleModelId, VehicleModelName, VehicleModelDescription INTO #MODEL FROM VehicleModel WHERE ISNULL(isActive, 1) = 1;

                    SELECT SO.SaleOrderId, SO.SaleOrderRefNo + ' - ' + CONVERT(VARCHAR, SaleOrderDate, 106) SaleOrderRefNo, SOI.SaleOrderItemId, WorkDescr, VehicleModelName+' - '+VehicleModelDescription VehicleModelName, C.CustomerName FROM #SALE SO
                    LEFT JOIN #SALE_ITEM SOI ON SO.SaleOrderId = SOI.SaleOrderId
                    LEFT JOIN #WORK W ON SOI.WorkDescriptionId = W.WorkDescriptionId
                    LEFT JOIN #MODEL M ON SOI.VehicleModelId = M.VehicleModelId
					LEFT JOIN Customer C ON SO.CustomerId = C.CustomerId
                    WHERE SOI.SaleOrderItemId = @saleOrderItemId;

                    DROP TABLE #SALE_ITEM;
                    DROP TABLE #MODEL;
                    DROP TABLE #WORK;
                    DROP TABLE #SALE;", new { saleOrderItemId = saleOrderItemId }).Single();
            }
        }

        public IEnumerable<VehicleInPass> GetAllVehicleInpass(int id,int cusid,int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"select VehicleInPassId,VehicleInPassNo,VehicleInPassDate,SaleOrderRefNo,SaleOrderDate,RegistrationNo,CustomerName from VehicleInPass V
                               inner join SaleOrder S ON S.SaleOrderId=V.SaleOrderId
                               inner join Customer C ON C.CustomerId=S.CustomerId where V.isActive=1 and V.OrganizationId = @OrganizationId and  V.VehicleInPassId = ISNULL(NULLIF(@id, 0), V.VehicleInPassId)
                               and S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId)";
                return connection.Query<VehicleInPass>(qry, new { OrganizationId = OrganizationId, id = id, cusid = cusid}).ToList();

            }
        }
    }
}