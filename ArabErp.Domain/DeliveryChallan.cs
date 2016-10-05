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
        public DateTime TransportWarrantyExpiryDate { get; set; }
        public int EmployeeId { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public bool IsUsed { get; set; }
        public string EmployeeName { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string Customer { get; set; }
        public string CustomerOrderRef { get; set; }
        public string SONODATE { get; set; }
        public string WorkDescr { get; set; }
        public string VehicleModel { get; set; }
        public string PaymentTerms { get; set; }
        public List<ItemBatch> ItemBatches { get; set; }

        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
    }
}
