using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SaleOrderItemRepository : BaseRepository
    {

        static string dataConnection = GetConnectionString("arab");

        public int InsertSaleOrderItem(SaleOrderItem objSaleOrderItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into SaleOrderItem(SaleOrderId,SlNo,WorkDescriptionId,VehicleModelId,Remarks,PartNo,Quantity,UnitId,Rate,Discount,Amount) 
                                                    Values (@SaleOrderId,@SlNo,@WorkDescriptionId,@VehicleModelId,@Remarks,@PartNo,@Quantity,@UnitId,@Rate,@Discount,@Amount);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSaleOrderItem, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
            
        }


        public SaleOrderItem GetSaleOrderItem(int SaleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderItem
                        where SaleOrderItemId=@SaleOrderItemId";

                var objSaleOrderItem = connection.Query<SaleOrderItem>(sql, new
                {
                    SaleOrderItemId = SaleOrderItemId
                }).First<SaleOrderItem>();

                return objSaleOrderItem;
            }
        }

        public List<SaleOrderItem> GetSaleOrderItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderItem
                        where isActive=1";

                var objSaleOrderItems = connection.Query<SaleOrderItem>(sql).ToList<SaleOrderItem>();

                return objSaleOrderItems;
            }
        }

        public int UpdateSaleOrderItem(SaleOrderItem objSaleOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SaleOrderItem SET SlNo = @SlNo ,WorkDescriptionId = @WorkDescriptionId ,Remarks = @Remarks,PartNo = @PartNo,Quantity = @Quantity,UnitId = @UnitId,Rate = @Rate,Discount = @Discount  OUTPUT INSERTED.SaleOrderItemId  WHERE SaleOrderItemId = @SaleOrderItemId";


                var id = connection.Execute(sql, objSaleOrderItem);
                return id;
            }
        }

        public int DeleteSaleOrderItem(Unit objSaleOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SaleOrderItem  OUTPUT DELETED.SaleOrderItemId WHERE SaleOrderItemId=@SaleOrderItemId";


                var id = connection.Execute(sql, objSaleOrderItem);
                return id;
            }
        }

        public List<Dropdown> FillWorkDesc()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select WorkDescriptionId Id ,WorkDescr Name from WorkDescription").ToList();
            }
        }
        public List<Dropdown> FillWorkDescForProject()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select WorkDescriptionId Id ,WorkDescr Name from WorkDescription where isProjectBased = 1").ToList();
            }
        }
        public List<Dropdown> FillUnit()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select UnitId Id,UnitName Name from Unit").ToList();
            }
        }
        public List<Dropdown> FillVehicle()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select VehicleModelId Id,VehicleModelName Name from VehicleModel").ToList();
            }
        }
    }
}
