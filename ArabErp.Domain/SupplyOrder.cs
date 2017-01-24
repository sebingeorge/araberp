using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class SupplyOrder
    {
        public SupplyOrder()
        {
            SupplyOrderItems = new List<SupplyOrderItem>();
        }
        public int SupplyOrderId { get; set; }
        [Required]
        [Display(Name="Supply Order Date")]
        public DateTime SupplyOrderDate { get; set; }
        [Required]
        public int PurchaseRequestId { get; set; }
        public int SupplierId { get; set; }
        public string SupplyOrderNo { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
        [Required]
        [ValidateDateGreaterThan("SupplyOrderDate")]
        public DateTime RequiredDate { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CreatedBy { get; set; }
        public int ApprovedBy { get; set; }
        public string CreatedUsersig { get; set; }
        public string ApprovedUser { get; set; }
        public string ApprovedUsersig { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public int OrganizationId { get; set; }
        public string SupplierName { get; set; }
        public string SoNoWithDate { get; set; }

        public string CreatedUser { get; set; }
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


        public string SupDoorNo { get; set; }

        //public string SupStreet { get; set; }

        public string SupState { get; set; }

        //public int SupCountry { get; set; }
        public string SupCountryName { get; set; }
        public string SupPostBoxNo { get; set; }
        public string SupPhone { get; set; }
        public string SupFax { get; set; }
        public string SupEmail { get; set; }
        public string CreatedDes { get; set; }
        public string ApprovedDes { get; set; }
        public decimal NetDiscount { get; set; }
        public string DiscountRemarks { get; set; }
        public decimal NetAmount { get; set; }
        public List<SupplyOrderItem> SupplyOrderItems { get; set; }
        public bool isUsed { get; set; }
        // public string SoNoWithDate
        // {
        //     get
        //     {
        //         return string.Format("{0},{1}", SupplyOrderId, SupplyOrderDate.ToShortDateString());
        //     }
        //     set
        //     {

        //     }
        //}

    }
}
