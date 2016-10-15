using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingTasksForCompletion
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string RegistrationNo { get; set; }
        public string WorkDescr { get; set; }
        public string BayName { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Engineer { get; set; }
        public bool isProjectBased { get; set; }
        public decimal EstimatedHours { get; set; }
        public DateTime TaskDate { get; set; }
        public string JobCardTaskName { get; set; }
        public string EmployeeName { get; set; }
    }
}
