using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ProformaInvoiceRepository : BaseRepository
 {
     static string dataConnection = GetConnectionString("arab");

     public ProformaInvoice GetSaleOrderForPorforma(int SaleOrderId)
     {
         using (IDbConnection connection = OpenConnection(dataConnection))
         {

             string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,SO.SaleOrderDate SaleOrderDate,
                                SO.SaleOrderRefNo +','+ Replace(Convert(varchar,SaleOrderDate,106),' ','/') SaleOrderRefNo
                                FROM  SaleOrder SO  INNER JOIN Customer C  ON SO.CustomerId =C.CustomerId
                                WHERE SO.SaleOrderId =@SaleOrderId";
             var objSaleOrders = connection.Query<ProformaInvoice>(sql, new { SaleOrderId = SaleOrderId }).Single<ProformaInvoice>();

             return objSaleOrders;
         }
     }
     public List<ProformaInvoiceItem> GetPorformaInvoiceData(int SaleOrderId)
     {
         using (IDbConnection connection = OpenConnection(dataConnection))
         {

             string query = "SELECT I.ItemName,I.ItemId,I.PartNo,SUM(WI.Quantity)Quantity,UnitName from WorkDescription W INNER JOIN  WorkVsItem WI on W.WorkDescriptionId=WI.WorkDescriptionId";
             query += " INNER JOIN Item I ON WI.ItemId=I.ItemId INNER JOIN Unit U on U.UnitId =I.ItemUnitId  INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId";
             query += " WHERE SI.SaleOrderId=@SaleOrderId GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName ";

             return connection.Query<ProformaInvoiceItem>(query,
             new { SaleOrderId = SaleOrderId }).ToList();


         }
     }

    }
}
