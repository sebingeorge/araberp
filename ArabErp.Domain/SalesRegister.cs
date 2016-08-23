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
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string WorkDescr { get; set; }
        public string CustomerName { get; set; }
        public int? Quantity { get; set; }
        public string UnitName { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public decimal? INVQTY { get; set; }
        public decimal? BALQTY { get; set; }
        public decimal? Perc { get; set; }
        public decimal? Target { get; set; }
        public decimal? Achieved { get; set; }

        public decimal? Varperc {get; set; }
        public string MonthName { get; set; }
        public decimal? Varience { get; set; }
        public decimal? Jan {get; set; }
        public decimal? Feb {get; set; }
        public decimal? Mar {get; set; }
        public decimal? Apr {get; set; }
        public decimal? May {get; set; }
        public decimal? Jun {get; set; }
        public decimal? Jul { get; set; }
        public decimal? Aug { get; set; }
        public decimal? Sep { get; set; }
        public decimal? Oct { get; set; }
        public decimal? Nov { get; set; }
        public decimal? Dece { get; set; }

      

      



    }
}
