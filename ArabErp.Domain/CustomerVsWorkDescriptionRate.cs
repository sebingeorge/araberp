using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class CustomerVsWorkDescriptionRate
    {
        public int CustomerVsWorkRateId { get; set; }
        public int CustomerId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public bool? isActive { get; set; }
        public List<CustomerVsWorkRateItem> CustomerVsWorkRateItem { get; set; }
    }
    public class CustomerVsWorkRateItem
    {
        public int? SlNo { get; set; }
        public int? WorkDescriptionId { get; set; }
        public string WorkDescription { get; set; }
        public decimal? FixedRate { get; set; }

    }

}
