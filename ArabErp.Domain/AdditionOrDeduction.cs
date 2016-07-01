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
        public string AddDedRefNo { get; set; }
        //[Required]
        public string AddDedName { get; set; }
        //[Required]
        public string AddDedType { get; set; }
        public string AddDedRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
