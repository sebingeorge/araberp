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
        public DateTime StoreIssueDate { get; set; }
        public int WorkShopRequestId { get; set; }
        public int EmployeeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
