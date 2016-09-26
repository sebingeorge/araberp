using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class EmployeeResourceAvailability
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmpCategoryName { get; set; }
        public DateTime TaskDate { get; set; }
        public string JobCardNo { get; set; }
        public string JobCardTaskName { get; set; }
        public bool isProjectBased { get; set; }
        public decimal Hours { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
