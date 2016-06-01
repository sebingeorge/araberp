using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Domain
{
    public class JobCard
    {
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime? JobCardDate { get; set; }
        public int SaleOrderId { get; set; }
        public string ChasisNoRegNo { get; set; }
        public string WorkDescription { get; set; }
        public int FreezerUnitId { get; set; }
        public int BoxId { get; set; }
        public int BayId { get; set; }
        public string SpecialRemarks { get; set; }
        public DateTime? RequiredDate { get; set; }
        public int EmployeeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int JodCardCompleteStatus { get; set; }
        public DateTime JodCardCompletedDate { get; set; }
        public List<JobCardTask> JobCardTasks { get; set; }

    }
}