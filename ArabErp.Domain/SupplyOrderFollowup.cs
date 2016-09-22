using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SupplyOrderFollowup
    {
        public int SupplyOrderFollowupId { get; set; }
        public string SupplyOrderDate { get; set; }
        public string SupplyOrderNo { get; set; }

        public string SupplierName { get; set; }
        public string ItemName { get; set; }
        public int SupplyOrderItemId { get; set; }
        [DataType(DataType.Date)]
        public DateTime SupplyOrderFollowupDate { get; set; }
        public decimal OrderedQty { get; set; }
        public int Supplier { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ExpectedDate { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
