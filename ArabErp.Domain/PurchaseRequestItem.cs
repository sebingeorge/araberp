using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class PurchaseRequestItem
    {
        public int PurchaseRequestItemId { get; set; }
        public int PurchaseRequestId { get; set; }
        public int? SlNo { get; set; }
        public string ItemName { get; set; }
        public int? ItemId { get; set; }
        public int? WRRequestQty { get; set; }
        public int? MinLevel { get; set; }
        public decimal? CurrentStock { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public decimal? Quantity { get; set; }
        public string UnitName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
