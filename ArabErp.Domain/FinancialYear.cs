using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class FinancialYear
    {
        public int? FyId { get; set; }
        public string FyName { get; set; }
        public DateTime? FyStartDate { get; set; }
        public DateTime? FyEndDate { get; set; }
    }
}
