using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class DCReport
    {
        public string DeliveryChallanRefNo { get; set; }
        public DateTime? DeliveryChallanDate { get; set; }
        public string CustomerName { get; set; }
        public string RegistrationNo { get; set; }
        public string ChassisNo { get; set; }
        public string UnitSerialNo { get; set; }
        public string UnitName { get; set; }
        public string BoxName { get; set; }
        public string TailLiftName { get; set; }
        public string InstallationType { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal LabourCost { get; set; }
        public string JobCardNo { get; set; }
        public DateTime? JobCardDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptDate { get; set; }
    }
}
