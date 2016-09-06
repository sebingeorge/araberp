using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SalesQuotationItem
    {
        public int SalesQuotationItemId { get; set; }
        public int SalesQuotationId { get; set; }
        public int? SlNo { get; set; }
        [Required]
        public int WorkDescriptionId { get; set; }
        public string WorkDescr { get; set; }
        public string VehicleModelName { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        [Required]
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public int ItemId { get; set; }
        public int RateType { get; set; }
    }
}
