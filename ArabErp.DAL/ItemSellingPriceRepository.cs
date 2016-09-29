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
                                    CategoryName,ItemGroupName,ItemSubGroupName,UnitName,
	                             ISP.SellingPrice
                                FROM 
                                Item I 
                                INNER JOIN ItemCategory ON itmCatId=ItemCategoryId
                               INNER JOIN ItemGroup G ON I.ItemGroupId=G.ItemGroupId
                               INNER JOIN ItemSubGroup S ON I.ItemSubGroupId=S.ItemSubGroupId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                LEFT JOIN ItemSellingPrice ISP ON I.ItemId = ISP.ItemId
                                WHERE 
                                 I.isActive=1 AND I.OrganizationId = @OrganizationId
                                  order by ItemName;";

                var objItemSellingPrices = connection.Query<ItemSellingPrice>(sql, new { OrganizationId = OrganizationId }).ToList<ItemSellingPrice>();

                return objItemSellingPrices;
            }
        }
    }
}
