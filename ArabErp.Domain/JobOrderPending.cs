using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class JobOrderPending
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string CustomerName { get; set; }
        public string VehicleModelName { get; set; }
        public string RegistrationNo { get; set; }
        public string ChasisNo { get; set; }
    }
}
