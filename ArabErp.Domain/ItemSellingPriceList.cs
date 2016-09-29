using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ItemSellingPriceList
    {
       public ItemSellingPriceList()
        {
            ItemSellingPriceLists = new List<ItemSellingPrice>();
        }

       public List<ItemSellingPrice> ItemSellingPriceLists { get; set; }
    }
}
