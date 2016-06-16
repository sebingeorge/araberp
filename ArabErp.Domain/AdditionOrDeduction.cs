using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class AdditionOrDeduction
    {
        public int? AddDedId { get; set; }
        [Required]
        public string AddDedName { get; set; }
        [Required]
        public int? AddDedType { get; set; }
        public string AddDedRemarks { get; set; }
    }
}
