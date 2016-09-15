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


            string sql = @"insert  into SalesQuotationItem(SalesQuotationId,SlNo,WorkDescriptionId,Remarks,PartNo,Quantity,Rate,MaterialRate,Discount,Amount,OrganizationId,RateType) Values (@SalesQuotationId,@SlNo,@WorkDescriptionId,@Remarks,@PartNo,@Quantity,@Rate,@MaterialRate,@Discount,@Amount,@OrganizationId,@RateType);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSalesQuotationItem, trn).Single();
                return id;
            
        }

        public int InsertSalesQuotationMaterial(SalesQuotationMaterial objSalesQuotationMaterial, IDbConnection connection, IDbTransaction trn)
        {


            string sql = @"insert  into SalesQuotationMaterial(SalesQuotationId,ItemId,Quantity,Rate,isActive) Values (@SalesQuotationId,@ItemId,@Quantity,@Rate,1);
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