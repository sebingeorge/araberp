using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StoresIssuePreviousList
    {
        public int StoreIssueId { get; set; }
        public string StoreIssueRefNo { get; set; }
        public DateTime StoreIssueDate { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public DateTime WorkShopRequestDate { get; set; }
        public string EmployeeName { get; set; }
        public string Remarks { get; set; }
         public string CustomerName { get; set; }
         public string JobCardNo { get; set; }
        public string RegistrationNo{get;set;}
        public string ChassisNo { get; set; }
    }
}
