using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class ItemBatch
    {
        public int ItemBatchId { get; set; }
        public int GRNItemId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public int? StoreIssueItemId { get; set; }
        public string SerialNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Remarks { get; set; }
        public string isDirect { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public int Ageing { get; set; }
        public string StockPointName { get; set; }
        public bool isSelected { get; set; }

    }
}
