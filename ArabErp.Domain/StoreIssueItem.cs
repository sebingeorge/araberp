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
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
