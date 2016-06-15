using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockJournal
    {
         public int?  StockJournalId {get;set;}
            [Required]
         public string  StockJournalRefno {get;set;}
         public int? StockPointId  {get;set;}
         public DateTime?  StockJournalDate {get;set;}
         public int? IssuedBy { get; set; }
         public string Remarks { get; set; }
         public string CreatedBy  {get;set;}
         public DateTime? CreatedDate  {get;set;}
         public int?  OrganizationId {get;set;}
         public List<StockJournalItem> StockJournelItems { get; set; }
    }
}
