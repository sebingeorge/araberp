using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class SalesRegister
    {
        public string SalesInvoiceRefNo { get; set; }
        public DateTime SalesInvoiceDate { get; set; }
        public string WorkDescr { get; set; }
        public string CustomerName { get; set; }
        public int? Quantity { get; set; }
        public string UnitName { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TotalAmount { get; set; }



    }
}
