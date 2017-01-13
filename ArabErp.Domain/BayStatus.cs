using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class BayStatus
    {
        public int BayId { get; set; }
        public string BayName { get; set; }
        public string Occupied { get; set; }
        public DateTime? JobCardDate { get; set; }
        public string JobCardNo { get; set; }
        public int? JobCardId { get; set; }
        public string ChasisNoRegNo { get; set; }
        public string VehicleModelName { get; set; }
        public string UnitName { get; set; }
        public string WorkDescription { get; set; }
        public DateTime? EDateDelivery { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EDateArrival { get; set; }
        public bool Status { get; set; }
        public string TaskNames { get; set; }
        public List<JobCardDailyActivityTask> Tasks { get; set; }
    }
}
