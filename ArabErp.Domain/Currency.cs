using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
    public class Currency
    {
        
        public int CurrencyId { get; set; }
        [Required]
        public string CurrencyRefNo { get; set; }
         [Required]
        public string CurrencyName { get; set; }
         [Required]
        public string Elementary { get; set; }
         [Required]
        public string CurrencyExRate { get; set; }
        public int CurrencySymbolId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string SymbolName { get; set; }
        public List<Symbol> Symbols { get; set; }
    }
    
    public class Symbol
{
        public int SymbolId { get; set; }
        public string SymbolName { get; set; }
}

}
