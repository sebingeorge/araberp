using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PrintDescription
    {
        public int PrintDescriptionId { get; set; }
        public int DeliveryChallanId { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public decimal? Quantity { get; set; }
        public decimal PriceEach { get; set; }
        public decimal Amount { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int SaleOrderId { get; set; }
        public string InvoiceType { get; set; }
    }
}
