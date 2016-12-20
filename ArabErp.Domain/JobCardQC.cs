using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class JobCardQC
    {
        public int JobCardQCId { get; set; }
        public string JobCardQCRefNo { get; set; }
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JcDate { get; set; }
        public string JobCardQCNoDate { get; set; }
        public string JobCardNoDate { get; set; }
        public string Customer { get; set; }
        public string VehicleModel { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime JobCardQCDate { get; set; }
        public bool IsQCPassed { get; set; }

        public string QCPassed { get; set; }
        public bool IsUsed { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        [ValidateDateGreaterThan("JcDate")]
        public DateTime CurrentDate { get; set; }
        public DateTime JobCardDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string QCStatus { get; set; }
        public string ChassisNo { get; set; }
        public string Remarks { get; set; }


        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public bool isService { get; set; }
        public string PunchingNo { get; set; }
        public List<JobCardQCParam> JobCardQCParams { get; set; }
    }
}
