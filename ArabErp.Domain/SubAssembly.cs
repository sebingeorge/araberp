using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SubAssembly : StockCreation
    {
        public bool isSubAssembly { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int WorkingHours { get; set; }

        public string EmployeeName { get; set; }
    }
}
