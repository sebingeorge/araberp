using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class DeliveryChallan
    {
        public int DeliveryChallanId { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        public int JobCardId { get; set; }
        public string RegistrationNo { get; set; }
        public DateTime DeliveryChallanDate { get; set; }
        public DateTime TransportWarrantyExpiryDate { get; set; }
        public int EmployeeId { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public bool IsUsed { get; set; }
        public string EmployeeName { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string Customer { get; set; }
        public string CustomerOrderRef { get; set; }
        public string SONODATE { get; set; }
        public string WorkDescr { get; set; }
        public string VehicleModel { get; set; }
        public string PaymentTerms { get; set; }
        public string CDoorNo { get; set; }
        public string CStreet { get; set; }
        public string CCountry { get; set; }
        public string SaleRefNo { get; set; }
        public string ChassisNo { get; set; }
        public string QuotationRefNo { get; set; }
        public int FreezerId { get; set; }
        public string FreezerName { get; set; }
        public string FreezerPartNo { get; set; }
        public int BoxId { get; set; }
        public string BoxName { get; set; }
        public string BoxPartNo { get; set; }
        public string SupplyOrderNo { get; set; }
        public DateTime SupplyOrderDate { get; set; }


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
        public string LPONo { get; set; }
        public DateTime LPODate { get; set; }
        public string CreatedUser { get; set; }
        public string CreatedUsersig { get; set; }
        public string printdes { get; set; }
        public List<ItemBatch> ItemBatches { get; set; }

        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }

        public string PrintDescription { get; set; }
    }
}
