using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingJC
    {
        public int JobCardId { get; set; }
        public string SaleOrderNoDate { get; set; }
        public string JobCardNoDate { get; set; }
        public string WorkDescr { get; set; }
        public string VehicleModel { get; set; }
        public string CustomerName { get; set; }
        public string RegistrationNo { get; set; }
        public int IsPaymentApprovedForDelivery { get; set; }
    }
}
