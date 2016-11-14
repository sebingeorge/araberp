using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesQuotationItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSalesQuotationItem(SalesQuotationItem objSalesQuotationItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {
                objSalesQuotationItem.WorkDescriptionId = GetMatchingWorkDescription(objSalesQuotationItem, connection, trn);

                string sql = @"insert  into SalesQuotationItem(SalesQuotationId,SlNo,WorkDescriptionId,Remarks,PartNo,Quantity,Rate,Discount,Amount,OrganizationId,RateType) Values (@SalesQuotationId,@SlNo,@WorkDescriptionId,@Remarks,@PartNo,@Quantity,@Rate,@Discount,@Amount,@OrganizationId,@RateType);
                    SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSalesQuotationItem, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private int GetMatchingWorkDescription(SalesQuotationItem model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string query = @"SELECT
	                                    WorkDescriptionId
                                    FROM WorkDescription
                                    WHERE FreezerUnitId " + (model.FreezerUnitId == null ? "IS NULL" : "= @FreezerUnitId") + @"
	                                    AND BoxId " + (model.BoxId == null ? "IS NULL" : "= @BoxId");
                return connection.Query<int>(query, new { BoxId = model.BoxId, FreezerUnitId = model.FreezerUnitId }, txn).First();
            }
            catch (InvalidOperationException)
            {
                //when there is no matching work description
                return CreateMatchingWorkDescription(connection, txn, model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int CreateMatchingWorkDescription(IDbConnection connection, IDbTransaction txn, SalesQuotationItem model)
        {
            try
            {
                string freezerName = model.FreezerUnitId == null ? String.Empty : new ItemRepository().GetItem(model.FreezerUnitId ?? 0).ItemName,
                    boxName = model.BoxId == null ? String.Empty : new ItemRepository().GetItem(model.BoxId ?? 0).ItemName,

                    ref_no = "WD/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(WorkDescription).Name, "0", 1),

                    query = @"INSERT INTO WorkDescription (WorkDescriptionRefNo, FreezerUnitId, BoxId, WorkDescr,
							        WorkDescrShortName, isNewInstallation, CreatedDate, OrganizationId, isActive,
							        isProjectBased)
                                VALUES
                                (
	                                @WorkDescriptionRefNo, @FreezerUnitId, @BoxId, @WorkDescr,
							        @WorkDescrShortName, @isNewInstallation, @CreatedDate, @OrganizationId, @isActive,
							        @isProjectBased
                                )
                                SELECT CAST(SCOPE_IDENTITY() AS INT)";

                int? id = connection.Query<int>(query,
                    new
                    {
                        WorkDescriptionRefNo = ref_no,
                        FreezerUnitId = model.FreezerUnitId,
                        BoxId = model.BoxId,
                        WorkDescr = freezerName + (freezerName == String.Empty || boxName == String.Empty ? String.Empty : " + ") + boxName,
                        WorkDescrShortName = freezerName + (freezerName == String.Empty || boxName == String.Empty ? String.Empty : " + ") + boxName,
                        isNewInstallation = 1,
                        CreatedDate = System.DateTime.Today,
                        OrganizationId = model.OrganizationId,
                        isActive = 1,
                        isProjectBased = 0
                    }, txn).FirstOrDefault();

                if (id == null) throw new Exception(); else return id ?? 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertSalesQuotationMaterial(SalesQuotationMaterial objSalesQuotationMaterial, IDbConnection connection, IDbTransaction trn)
        {


            string sql = @"insert  into SalesQuotationMaterial(SalesQuotationId,ItemId,Quantity,Rate,Discount,Amount,isActive) Values (@SalesQuotationId,@ItemId,@Quantity,@Rate,@Discount,@Amount,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSalesQuotationMaterial, trn).Single();
            return id;

        }

        public SalesQuotationItem GetSalesQuotationItem(int SalesQuotationItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotationItem
                        where SalesQuotationItemId=@SalesQuotationItemId";

                var objSalesQuotationItem = connection.Query<SalesQuotationItem>(sql, new
                {
                    SalesQuotationItemId = SalesQuotationItemId
                }).First<SalesQuotationItem>();

                return objSalesQuotationItem;
            }
        }

        public List<SalesQuotationItem> GetSalesQuotationItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotationItem
                        where isActive=1";

                var objSalesQuotationItems = connection.Query<SalesQuotationItem>(sql).ToList<SalesQuotationItem>();

                return objSalesQuotationItems;
            }
        }



        public int DeleteSalesQuotationItem(Unit objSalesQuotationItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesQuotationItem  OUTPUT DELETED.SalesQuotationItemId WHERE SalesQuotationItemId=@SalesQuotationItemId";


                var id = connection.Execute(sql, objSalesQuotationItem);
                return id;
            }
        }


    }
}