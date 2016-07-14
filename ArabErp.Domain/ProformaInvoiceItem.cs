using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class ProformaInvoiceItem
    {
         public int? ProformaInvoiceItemId { get; set; }
         public int? ProformaInvoiceId { get; set; }
         public int? SaleOrderItemId { get; set; }
         public int? SlNo { get; set; }
         public string WorkDescription { get; set; }
         public string VehicleModelName { get; set; }
         public string UnitName { get; set; }
         public int? Quantity { get; set; }
         public decimal? Rate { get; set; }
          public decimal? Discount { get; set; }
          public decimal Amount { get; set; }
       
        

      }
    }

