using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Currency
    {
        public int CurrencyId { get; set; }
        public string CurrencyRefNo { get; set; }
        public string CurrencyName { get; set; }
        public string Elementary { get; set; }
        public string CurrencyExRate { get; set; }
        public int CurrencySymbolId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<Symbol> Symbols { get; set; }
    }
    
    public class Symbol
{
        public int SymbolId { get; set; }
        public string SymbolName { get; set; }
}
}
