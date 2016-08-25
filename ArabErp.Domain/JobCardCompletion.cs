using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class JobCardCompletion
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public string WorkDescr { get; set; }
        public int? WorkDescriptionId { get; set; }
        public DateTime WarrentyPeriod { get; set; }
        public DateTime JobCardCompletedDate { get; set; }
        public string SpecialRemarks { get; set; }
        public List<JobCardCompletionTask> JobCardTask { get; set; }
        public int? isProjectBased { get; set; }
    }
    public class JobCardCompletionTask
    {
        public int? SlNo { get; set; }
        public int? JobCardTaskMasterId { get; set; }
        public string JobCardTaskName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime TaskDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ActualHours { get; set; }
        public int Existing { get; set; }
    }
}
