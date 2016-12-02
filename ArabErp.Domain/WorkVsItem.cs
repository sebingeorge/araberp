using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class WorkVsItem
    {
        public int? SlNo { get; set; }
        public int WorkVsItemId { get; set; }
        public int ItemId { get; set; }
        public string UoM { get; set; }
        public int WorkDescriptionId { get; set; }
        public decimal Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string ItemName { get; set; }
       
    }
}
