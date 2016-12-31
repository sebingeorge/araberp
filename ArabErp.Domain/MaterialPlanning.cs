﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class MaterialPlanning
    {
       
        public string ItemName { get; set; }
        public int? ItemId { get; set; }
        public int? WRQTY { get; set; }
        public int? MinLevel { get; set; }
        public int? CurrentStock { get; set; }
        public string PartNo { get; set; }
        public decimal? WRPndIssQty { get; set; }
        public decimal? TotalQty { get; set; }
       public decimal InTransitQty { get; set; }
        public decimal PendingPRQty { get; set; }
        public decimal ShortorExcess { get; set; }
        public string UnitName { get; set; }
        public string batch { get; set; }
        public decimal? IssuedQty { get; set; }
        public int? SlNo { get; set; }
      
       
    }
}
