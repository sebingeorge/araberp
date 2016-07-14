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
             string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,DoorNo +','+ Street+','+State CustomerAddress,SO.SaleOrderDate SaleOrderDate,
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

             string query = @"SELECT s.SaleOrderId ,S.SaleOrderItemId, W.WorkDescr WorkDescription,v.VehicleModelName,S.Quantity,U.UnitName,S.Rate,S.Discount,S.Amount
                            FROM SaleOrderItem S LEFT JOIN Unit U on S.UnitId=U.UnitId
                            LEFT JOIN WorkDescription W on S.WorkDescriptionId=W.WorkDescriptionId
                            Left JOIN VehicleModel V on S.VehicleModelId=V.VehicleModelId
                            WHERE S.SaleOrderId=@SaleOrderId";
             return connection.Query<ProformaInvoiceItem>(query,
             new { SaleOrderId = SaleOrderId }).ToList();
         }
     }
     public Currency GetCurrencyFrmOrganization(int OrganizationId)
     {
         using (IDbConnection connection = OpenConnection(dataConnection))
         {
             string sql = @"select S.SymbolName,C.CurrencyName from Organization O 
                             inner join Currency C on C.CurrencyId = O.CurrencyId
                             inner join Symbol S on C.CurrencySymbolId = S.SymbolId
                             where O.OrganizationId=@OrganizationId";
             var objCurrency = connection.Query<Currency>(sql, new
             {
                 OrganizationId = OrganizationId
             }).First<Currency>();
             return objCurrency;
         }
     }
    }
}
