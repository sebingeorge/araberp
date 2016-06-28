﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class SaleOrder
    {
        public int SaleOrderId { get; set; }
     
        public string SaleOrderRefNo { get; set; }
    
        public DateTime SaleOrderDate { get; set; }
        [Required]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string WorkDescription { get; set; }
         [Required]
        public string CustomerOrderRef { get; set; }
        public int? CurrencyId { get; set; }

        public string CurrencyName { get; set; }
        public string SpecialRemarks { get; set; }
        public int PaymentTermsId { get; set; }
        public string PaymentTermsRemarks { get; set; }
        public string DeliveryTerms { get; set; }
        [Required]
        public decimal? TotalAmount { get; set; }
        public decimal? TotalDiscount { get; set; }
        public int CommissionAgentId { get; set; }
        public decimal? CommissionAmount { get; set; }
        public float? CommissionPerc { get; set; }
        [Required]
        public int SalesExecutiveId { get; set; }
        public DateTime EDateArrival { get; set; }
         [Required]
        public DateTime EDateDelivery { get; set; }
         [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public int? VehicleModelId { get; set; }
        public int SalesQuotationId { get; set; }

        [Required]

        public string SaleOrderHoldReason { get; set; }
        public DateTime SaleOrderHoldDate { get; set; }
        [Required]
        public DateTime SaleOrderReleaseDate { get; set; }
        [Required]
        public List<SaleOrderItem> Items { get; set; }
        public bool Select { get; set; }
        public int? Ageing { get; set; }
        public string SoNoWithDate
        {
            get
         {
             return string.Format("{0},{1}", SaleOrderRefNo, SaleOrderDate.ToShortDateString());
         }
         set
         {
             
         }
}
        }
    }

