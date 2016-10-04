using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace ArabErp.Domain
{
   public class QuerySheet
    {
        public int QuerySheetId { get; set; }
        public string QuerySheetRefNo { get; set; }
         [Required]
        public DateTime QuerySheetDate { get; set; }
              public string ProjectName { get; set; }
        public string ContactPerson { get; set; }
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid Phone number")]
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<QuerySheetItem> QuerySheetItems { get; set; }
        public List<ProjectCost> Items { get; set; }
        public bool isUsed { get; set; }
    }
    public class ProjectCost
    {
        public int CostingId { get; set; }
        public int QuerySheetId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }

    }
}
