using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PurchaseBillPostingTransaction
    {
        public string Trans { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string Num { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public string Account { get; set; }
        public string Class { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
    }
}
