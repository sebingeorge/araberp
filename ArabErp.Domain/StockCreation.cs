using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockCreation
    {
        public int StockCreationId { get; set; }
        public string StockCreationRefNo { get; set; }
        public DateTime StockCreationDate { get; set; }

        public List<StockCreationFinishedGood> FinishedGoods { get; set; }
        public List<StockCreationConsumedItem> ConsumedItems { get; set; }

        public int OrganizationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        public int FinishedStockpointId { get; set; }
        [Required]
        public int ConsumedStockpointId { get; set; }

        public string Finished { get; set; }
        public string Consumed { get; set; }
    }
}
