using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PurchaseRequestRegister
    {
        public string PurchaseRequestNo { get; set; }
        public DateTime PurchaseRequestDate { get; set; }
        public string ItemName { get; set; }
        public string Remarks { get; set; }
        public decimal ReqQty { get; set; }
        public decimal GRNQTY { get; set; }
        public int SettledQty { get; set; }
        public decimal BALQTY { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
    }
}
