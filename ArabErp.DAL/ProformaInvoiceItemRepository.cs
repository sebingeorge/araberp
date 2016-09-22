using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class ProformaInvoiceItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertProformaInvoiceItem(ProformaInvoiceItem objProInvoiceItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into ProformaInvoiceItem(ProformaInvoiceId,SaleOrderItemId,Rate,Discount,Amount) 
                                                    Values (@ProformaInvoiceId,@SaleOrderItemId,@Rate,@Discount,@Amount);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objProInvoiceItem, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<ProformaInvoiceItem> GetProformaInvoiceItemDT(int ProformaInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT V.VehicleModelName,W.WorkDescr WorkDescription,W.WorkDescriptionRefNo,ProformaInvoiceItemId,ProformaInvoiceId,P.SaleOrderItemId,S.Quantity,P.Rate,P.Discount,P.Amount,'job'UnitName
                                FROM ProformaInvoiceItem P
                                inner join SaleOrderItem S ON P.SaleOrderItemId=S.SaleOrderItemId
                                left join WorkDescription W on S.WorkDescriptionId=W.WorkDescriptionId
                                left join VehicleModel V on V.VehicleModelId=S.VehicleModelId
                                left join Unit u on u.UnitId=s.UnitId where ProformaInvoiceId =@ProformaInvoiceId";

                return connection.Query<ProformaInvoiceItem>(sql, new { ProformaInvoiceId = ProformaInvoiceId }).ToList();


            }
        }
    }
}


