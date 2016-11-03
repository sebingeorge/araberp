using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class GRN
    {
        public int GRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string Supplier { get; set; }
        public int SupplierId { get; set; }
        public string SONODATE { get; set; }
        public int SupplyOrderId { get; set; }
        public int StockPointId { get; set; }
        public string StockPointName { get; set; }
        public string SupplierDCNoAndDate { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public string VehicleNo { get; set; }
        public string GatePassNo { get; set; }
        public string SpecialRemarks { get; set; }
        public decimal? GrandTotal { get; set; }
        public int CurrencyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int ReceivedBy { get; set; }
        public string EmpReceivedBy { get; set; }

        public string ReceivedByName { get; set; }
        public bool isDirectPurchaseGRN { get; set; }
        public List<GRNItem> Items { get; set; }
        public List<GRNAddition> Additions { get; set; }
        public List<GRNDeduction> Deductions { get; set; }
        public string CurrencyName { get; set; }
        public bool isUsed { get; set; }
        public decimal Addition { get; set; }
        public decimal Deduction { get; set; }
       //org
        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string OrgCountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string OrganizationName { get; set; }
        public string OrgCurrency { get; set; }
        public string CompanyName { get; set; }
        public string Image1 { get; set; }
    }
}
