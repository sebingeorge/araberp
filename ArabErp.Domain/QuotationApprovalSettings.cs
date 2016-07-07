using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class QuotationApprovalViewModel
    {
        public List<QuotationApprovalSettings> QuotationApprovalSettings{ get; set;}
        public List<QuotationApprovalAmountSettings> QuotationApprovalAmountSettings { get; set; }
    }
    public class QuotationApprovalSettings
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Approval1 { get; set; }
        public int Approval2 { get; set; }
        public int Approval3 { get; set; }
    }
}
