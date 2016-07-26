using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingJobCardQC
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string JobCardDateString { get; set; }
        public int SaleOrderId { get; set; }
        public int SaleOrderItemId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleModelId { get; set; }
        public string CustomerName { get; set; }
        public string VehicleModelName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CurrentDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
