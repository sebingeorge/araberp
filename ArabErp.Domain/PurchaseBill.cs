using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class PurchaseBill
    {
        public PurchaseBill()
        {
            PurchaseBillItems = new List<PurchaseBillItem>();
        }

        public int PurchaseBillId { get; set; }
        public int SupplierId { get; set; }
        public DateTime PurchaseBillDate { get; set; }
        public string Remarks { get; set; }
        public int EmployeeId { get; set; }
        public decimal PurchaseBillAmount { get; set; }
        public string AdditionRemarks { get; set; }
        public string DeductionRemarks { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? Addition { get; set; }
        public int? CurrencyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }

        public List<PurchaseBillItem> PurchaseBillItems { get; set; }
    }
}
