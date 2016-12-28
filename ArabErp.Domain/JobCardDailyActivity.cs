using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class JobCardDailyActivity
    {
        public int JobCardDailyActivityId { get; set; }
        public string JobCardDailyActivityRefNo { get; set; }
        [Required]
        public DateTime JobCardDailyActivityDate { get; set; }
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string Remarks { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string RegistrationNo { get; set; }
        public string ChassisNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public int isProjectBased { get; set; }
        public bool IsUsed { get; set; }
        
        public List<JobCardDailyActivityTask> JobCardDailyActivityTask { get; set; }

        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        [Required]
        public decimal ActualHours { get; set; }
        public string Tasks { get; set; }
    }
}
