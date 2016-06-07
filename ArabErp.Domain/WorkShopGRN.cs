using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class WorkShopGRN
    {
        public WorkShopGRN()
        {
            WorkShopGRNItems = new List<WorkShopGRNItem>();
        }
        public int WorkShopGRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierDCNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public string SONoAndDate { get; set; }
        public string AdditionRemarks { get; set; }
        public string DeductionRemarks { get; set; }
        public decimal Deduction { get; set; }
        public decimal Addition { get; set; }
        public int CurrencyId { get; set; }
        public decimal GRNTotalAmount { get; set; }

        public List<WorkShopGRNItem> WorkShopGRNItems { get; set; }
    }
}
