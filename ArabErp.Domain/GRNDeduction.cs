using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class GRNDeduction
    {
        public int GRNId { get; set; }
        public int? DeductionId { get; set; }
        public decimal Deduction { get; set; }
    }
}
