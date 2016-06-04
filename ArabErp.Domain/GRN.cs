﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class GRN
    {
        public int GRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string Supplier { get; set; }
        public int SupplierId { get; set; }
        public string SONODATE { get; set; }
        public int SupplyId { get; set; }
        public int StockPointId { get; set; }
        public string SupplierDCNoAndDate { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<GRNItem> Items { get; set; }
    }
}
