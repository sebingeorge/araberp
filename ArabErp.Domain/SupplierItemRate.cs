using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
  public  class SupplierItemRate
    {
        public int SupplierItemRateId { get; set; }
        public int SupplierId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public bool? isActive { get; set; }
        public List<SupplierItemRateItem> SupplierItemRateItem { get; set; }
    }
        public class SupplierItemRateItem
            {
                public int? SlNo { get; set; }
                public int? ItemId { get; set; }
                public decimal? FixedRate { get; set; }

            }

}
