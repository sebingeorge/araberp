using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class PurchaseRequest
    {
        public int PurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        [Required]
        public DateTime PurchaseRequestDate { get; set; }
        [Required]
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public string SaleOrderRefNo { get; set; }
        public string CustomerOrderRef { get; set; }
        public string CustomerName { get; set; }
        public string SpecialRemarks { get; set; }
        [Required]
        [ValidateDateGreaterThan("PurchaseRequestDate")]
        public DateTime RequiredDate { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        [Required]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }

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
        public int cmpCode { get; set; }
        public string CompanyName { get; set; }
        public string Image1 { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedUsersig { get; set; }
        public List<PurchaseRequestItem>items { get; set; }
        public bool isUsed { get; set; }
    }
}
