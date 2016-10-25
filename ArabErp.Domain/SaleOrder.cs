using System;
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
        [Required]
        public DateTime SaleOrderDate { get; set; }
        public int? SalesQuotationId { get; set; }
        public string QuotationNoDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        [Required]
        public string CustomerOrderRef { get; set; }
        [Required]
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
     
        public decimal? TotalAmount { get; set; }
        public decimal? TotalDiscount { get; set; }
        public int? CommissionAgentId { get; set; }
        public decimal? CommissionAmount { get; set; }
        public float? CommissionPerc { get; set; }
        [Required]
        public int SalesExecutiveId { get; set; }

        [Required]
        [ValidateDateGreaterThan("SaleOrderDate")]
        [Display(Name = "Expected Date of Arrival")]
        public DateTime EDateArrival { get; set; }

        [Required]
        [ValidateDateGreaterThan("SaleOrderDate")]
        [Display(Name = "Expected Date of Delivery")]
        public DateTime EDateDelivery { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public int? VehicleModelId { get; set; }

        public string SaleOrderHoldReason { get; set; }
        public DateTime SaleOrderHoldDate { get; set; }
        [Required]
        public DateTime SaleOrderReleaseDate { get; set; }
        [Required]
        public List<SaleOrderItem> Items { get; set; }
        public List<SalesQuotationMaterial> Materials { get; set; }
        public bool Select { get; set; }
        public int? Ageing { get; set; }
        public int isProjectBased { get; set; }
        public int isAfterSales { get; set; }
        public string WorkDescription { get; set; }
        public int? Remaindays { get; set; }
        public string WorkRequestPaymentApproved { get; set; }
        public string EmployeeName { get; set; }
        public string CommissionAgentName { get; set; }
        public bool isUsed { get; set; }

        //Organization

        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
    }
}

