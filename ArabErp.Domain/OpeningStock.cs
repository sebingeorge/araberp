using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class OpeningStock
    {
        public int? OpeningStockId { get; set; }
        [Required]
        public int? stockpointId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public List<OpeningStockItem> OpeningStockItem { get; set; }
    }
   public class OpeningStockItem
    {
        public int? SlNo { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal? Quantity { get; set; }
        public bool isUsed { get; set; }
        public int OpeningStockId { get; set; }
        public string PartNo { get; set; }
    }
}
