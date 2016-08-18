using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SubAssembly : StockCreation
    {
        public bool isSubAssembly { get; set; }
        public int EmployeeId { get; set; }
        public int WorkingHours { get; set; }

        public string EmployeeName { get; set; }
    }
}
