using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class WorkShopGRNItem
    {
        public int WorkShopGRNItemId { get; set; }
        public int WorkShopGRNId { get; set; }
        public int? SlNo { get; set; }
        public int ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public bool isActive { get; set; }
        public string Remarks { get; set; }
    }
}
