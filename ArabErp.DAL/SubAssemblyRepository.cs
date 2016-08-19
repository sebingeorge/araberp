using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Collections;
using System.Data.SqlClient;

namespace ArabErp.DAL
{
    public class SubAssemblyRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public string ConnectionString()
        {
            return dataConnection;
        }

        public string CreateSubAssembly(SubAssembly model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.StockCreationRefNo = "SUB/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(SubAssembly).Name, "0", 1);

                    int id = Create(model, connection, txn);

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Sub-assembly", id.ToString(), model.OrganizationId.ToString());
                    txn.Commit();
                    return model.StockCreationRefNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        private int Create(SubAssembly model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                #region Inserting into [StockCreation] head
                string query = @"INSERT INTO StockCreation
                                    (
	                                    StockCreationRefNo,
	                                    StockCreationDate,
                                        isSubAssembly,
                                        EmployeeId,
                                        WorkingHours,
                                        CreatedBy,
                                        CreatedDate,
                                        OrganizationId,
                                        ConsumedStockpointId,
                                        FinishedStockpointId
                                    )
                                    VALUES
                                    (
                                        @StockCreationRefNo,
	                                    @StockCreationDate,
	                                    1,
                                        @EmployeeId,
                                        @WorkingHours,
                                        @CreatedBy,
                                        @CreatedDate,
                                        @OrganizationId,
                                        @ConsumedStockpointId,
                                        @FinishedStockpointId
                                    );
                                    SELECT CAST(SCOPE_IDENTITY() AS INT)";

                int id = connection.Query<int>(query, model, txn).First();
                #endregion

                #region Inserting Finished Goods + Stock Updation
                foreach (var item in model.FinishedGoods)
                {
                    item.StockCreationId = id;
                    var i = new StockCreationFinishedGoodsRepository().InsertFinishedGoods(item, connection, txn);
                    new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                    {
                        OrganizationId = model.OrganizationId,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate,
                        StockPointId = model.FinishedStockpointId,
                        StockType = typeof(SubAssembly).Name,
                        StockInOut = "IN",
                        stocktrnDate = System.DateTime.Today,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        StocktrnId = id,
                        StockUserId = model.StockCreationRefNo
                    }, connection, txn);
                }
                #endregion

                #region Inserting Consumed Items + Stock Updation
                foreach (var item in model.ConsumedItems)
                {
                    item.StockCreationId = id;
                    var i = new StockCreationConsumedItemsRepository().InsertConsumedItems(item, connection, txn);
                    new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                    {
                        OrganizationId = model.OrganizationId,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate,
                        StockPointId = model.FinishedStockpointId,
                        StockType = typeof(SubAssembly).Name,
                        StockInOut = "OUT",
                        stocktrnDate = System.DateTime.Today,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity * (-1),
                        StocktrnId = id,
                        StockUserId = model.StockCreationRefNo
                    }, connection, txn);
                }
                #endregion

                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable GetSubAssemblies(int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SC.StockCreationId,
	                                SC.StockCreationRefNo,
									CONVERT(VARCHAR, SC.StockCreationDate, 106) StockCreationDate,
									EMP.EmployeeName,
									SC.WorkingHours,

	                                STUFF((SELECT ', ' + CAST(I.ItemName+'('+CAST(T1.Quantity AS VARCHAR(MAX)) +')' AS VARCHAR(MAX)) [text()]
	                                FROM StockCreationFinishedGoods T1 INNER JOIN StockCreation T2 on T1.StockCreationId = T2.StockCreationId
	                                INNER JOIN Item I ON T1.ItemId = I.ItemId
	                                WHERE T1.StockCreationId = SC.StockCreationId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') Finished,

	                                STUFF((SELECT ', ' + CAST(I.ItemName+'('+CAST(T1.Quantity AS VARCHAR(MAX)) +')' AS VARCHAR(MAX)) [text()]
	                                FROM StockCreationConsumedItems T1 INNER JOIN StockCreation T2 on T1.StockCreationId = T2.StockCreationId
	                                INNER JOIN Item I ON T1.ItemId = I.ItemId
	                                WHERE T1.StockCreationId = SC.StockCreationId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') Consumed

                                FROM StockCreation SC
									LEFT JOIN Employee EMP ON SC.EmployeeId = EMP.EmployeeId
								WHERE SC.OrganizationId = 1
                                AND SC.isSubAssembly = 1
								AND SC.isActive = 1 
								ORDER BY SC.StockCreationDate DESC, SC.CreatedDate DESC";
                return connection.Query<SubAssembly>(query, new { OrganizationId = organizationId }).ToList();
            }
        }

        public SubAssembly GetSubAssembly(int id, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region Get data from [StockCreation] head
                string query = @"SELECT
	                                StockCreationId,
	                                StockCreationRefNo,
	                                StockCreationDate,
	                                isSubAssembly,
	                                EmployeeId,
	                                WorkingHours,
                                    ConsumedStockpointId,
                                    FinishedStockpointId
                                FROM StockCreation
                                WHERE StockCreationId = @id
                                AND OrganizationId = @organizationId
                                AND isActive = 1";
                SubAssembly model = connection.Query<SubAssembly>(query, new { id=id, OrganizationId = organizationId }).First();
                #endregion

                #region Get data from [StockCreationConsumedItems]
                query = @"SELECT
	                            ConsumedItemsId,
	                            StockCreationId,
	                            ItemId,
	                            Quantity,
	                            Rate
                            FROM StockCreationConsumedItems
                            WHERE StockCreationId = @id
                            AND isActive = 1";
                model.ConsumedItems = connection.Query<StockCreationConsumedItem>(query, new { id = id }).ToList(); 
                #endregion

                #region Get data from [StockCreationFinishedGoods]
                query = @"SELECT
	                            FinishedGoodsId,
	                            StockCreationId,
	                            ItemId,
	                            Quantity,
	                            Rate
                            FROM StockCreationFinishedGoods
                            WHERE StockCreationId = @id
                            AND isActive = 1";
                model.FinishedGoods = connection.Query<StockCreationFinishedGood>(query, new { id = id }).ToList(); 
                #endregion

                return model;
            }
        }

        public string UpdateSubAssembly(SubAssembly model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    //if (Delete(model, connection, txn) <= 0) return "";

                    //Create(model, connection, txn);

                    //InsertLoginHistory(dataConnection, model.CreatedBy, "Update", "Sub-assembly", model.StockCreationId.ToString(), model.OrganizationId.ToString());
                    //txn.Commit();
                    return model.StockCreationRefNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        private int Delete(SubAssembly model, IDbConnection connection, IDbTransaction txn)
        {
            string query;
            int id = 0;
            try
            {
                query = @"DELETE FROM StockCreationConsumedItems WHERE StockCreationId = @id;
                          DELETE FROM StockCreationFinishedGoods WHERE StockCreationId = @id;
                          DELETE FROM StockCreation OUTPUT deleted.StockCreationId WHERE StockCreationId = @id";
                id = connection.Query<int>(query, new { id = model.StockCreationId }, txn).First();
            }
            catch (SqlException sx)
            {
                if (sx.Errors[0].Number == 547)
                {
                    return 0;
                }
            }
            return id;
        }
    }
}
