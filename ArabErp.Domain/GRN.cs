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
        public int SupplierId { get; set; }
        public int WareHouseId { get; set; }
        public string SupplierDCNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
