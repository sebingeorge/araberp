﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class SalesInvoice
    {
        public int SalesInvoiceId { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
       
        public string CurrentDate { get; set; }
        public string SalesInvoiceNo { get; set; }
        [Required]
        public string SalesInvoiceRefNo { get; set; }
       
        public DateTime? SalesInvoiceDate { get; set; }
        public string Customer { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerOrderRef { get; set; }
        public string WorkDescription { get; set; }
        public string VehicleRegChasisNo { get; set; } 
        public int JobCardId { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string CurrencySymbol { get; set; }
     
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string SaleOrderRefNoWithDate { get; set; }
        public decimal? Addition { get; set; }
        public decimal? Deduction { get; set; }
        public string AdditionRemarks { get; set; }
        public string DeductionRemarks { get; set; }
        public List<SalesInvoiceItem> SaleInvoiceItems { get; set; }
    }
}
