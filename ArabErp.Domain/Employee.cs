using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class Employee
    {
        public int? EmployeeId { get; set; }
        public string EmployeeRefNo { get; set; }
        public string EmployeeName { get; set; }
        public int? GenderId { get; set; }
        public int? DesignationId { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public int? TaskId { get; set; }
        public decimal Hourlycost { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
       

        public List<Designation> Designations { get; set; }
    }

}
