﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class OpeningStockReport
    {
      public string ItemName { get; set; }
      public string ItemGroupName { get; set; }
      public string ItemSubGroupName { get; set; }
      public string PartNo { get; set; }
      public decimal Quantity { get; set; }
      public string UnitName { get; set; }
      public string StockUserId { get; set; }
      public string StockType { get; set; }
      public string StockInOut { get; set; }
      public decimal  INQTY{ get; set; }
      public decimal OUTQTY { get; set; }
      public decimal OPENINGSTOCK { get; set; }
      public int itmCatId { get; set; }
     // public int ItemId { get; set; }
      public string ItemId { get; set; }
    }
}
