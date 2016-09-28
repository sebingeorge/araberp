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
  public  class ItemSellingPriceRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public List<ItemSellingPrice> GetItemSellingPrices(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                
                                    I.ItemId,
	                                I.ItemName,
	                                ISNULL(I.PartNo, '-') PartNo,
	                             ISP.SellingPrice
                                FROM 
                                Item I 
                                LEFT JOIN ItemSellingPrice ISP ON I.ItemId = ISP.ItemId
                                WHERE 
                                AND I.OrganizationId = @OrganizationId;";

                var objItemSellingPrices = connection.Query<ItemSellingPrice>(sql, new { OrganizationId = OrganizationId }).ToList<ItemSellingPrice>();

                return objItemSellingPrices;
            }
        }
    }
}
