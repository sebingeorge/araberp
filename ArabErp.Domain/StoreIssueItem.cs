using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StoreIssueItem
    {
        public int StoreIssueItemId { get; set; }
        public int WorkShopRequestItemId { get; set; }
        public int StoreIssueId { get; set; }
        public decimal? IssuedQuantity { get; set; }
        public bool isActive { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal? CurrentIssuedQuantity { get; set; }
        public decimal PendingQuantity { get; set; }
        public decimal StockQuantity { get; set; }
    }
}
