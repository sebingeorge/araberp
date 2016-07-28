using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class DeliveryChallan
    {
        public int DeliveryChallanId { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        public int JobCardId { get; set; }
        public string RegistrationNo { get; set; }
        public DateTime DeliveryChallanDate { get; set; }
        public int EmployeeId { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }

        public List<ItemBatch> ItemBatches { get; set; }
    }
}
