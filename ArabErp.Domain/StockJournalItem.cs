using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockJournalItem 
    {
       public int? StockJournalItemId { get; set; }
       public int? StockJournalId { get; set; }
       public int? SlNo { get; set; }
         [Required]
       public int? ItemId { get; set; }
       public string PartNo { get; set; }
         [Required]
       public float? Quantity { get; set; }
       public string StockJournalRefno { get; set; }
       public int? StockPointId { get; set; }
       public string Remarks { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int? OrganizationId { get; set; }


    }
}
