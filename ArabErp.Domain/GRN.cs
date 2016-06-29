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
        public decimal Addition { get; set; }
        public decimal Deduction { get; set; }
        public int? AdditionId { get; set; }
        public int? DeductionId { get; set; }
        public decimal? GrandTotal { get; set; }
        public int CurrencyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int ReceivedBy { get; set; }
        public string ReceivedByName { get; set; }
        public bool isDirectPurchaseGRN { get; set; }
        public List<GRNItem> Items { get; set; }
    }
}
