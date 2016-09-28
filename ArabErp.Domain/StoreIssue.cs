using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StoreIssue
    {
        public int StoreIssueId { get; set; }
        public string StoreIssueRefNo { get; set; }
        public DateTime StoreIssueDate { get; set; }
        public int StockpointId { get; set; }
        public string CustomerName { get; set; }
        public string WONODATE { get; set; }
        public string SONODATE { get; set; }
        public int WorkShopRequestId { get; set; }
        public DateTime RequiredDate { get; set; }
        public int EmployeeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string Remarks { get; set; }
        public List<StoreIssueItem> Items { get; set; }
    }
}
