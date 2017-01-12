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
        public string StockpointName { get; set; }
        public string CustomerName { get; set; }
        public string WONODATE { get; set; }
        public string SONODATE { get; set; }
        public int WorkShopRequestId { get; set; }
        public DateTime RequiredDate { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string Remarks { get; set; }


        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public string JobCardNo { get; set; }
        public string UserName { get; set; }
        public string Signature { get; set; }
        public List<StoreIssueItem> Items { get; set; }
    }
}
