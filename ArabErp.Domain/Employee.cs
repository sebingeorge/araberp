using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Employee
    {
        public int? EmployeeId { get; set; }
        public string EmployeeRefNo { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        [Required]
        public int? GenderId { get; set; }
        [Required]
        public int? DesignationId { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        public int? LocationId { get; set; }
        public int? TaskId { get; set; }
        [Required]
        public decimal Hourlycost { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public string Gender { get; set; }
        public string DesignationName { get; set; }
        
        public List<Designation> Designations { get; set; }
    }

}
