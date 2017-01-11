using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PurchaseBillPostingList
    {
        public int PurchaseBillId { get; set; }
        public string PurchaseBillRefNo { get; set; }
        public DateTime PurchaseBillDate { get; set; }
        public string SupplierName { get; set; }
        public string PurchaseBillNoDate { get; set; }
        public decimal PurchaseBillAmount { get; set; }
        public string CurrencyName { get; set; }
        public int IsSelected { get; set; }
        public string PostStatus { get; set; }
    }
    public class PendingPurchaseBillsForPosting
    {
        public List<PurchaseBillPostingList> PurchaseBillPostingList { get; set; }
    }
}
