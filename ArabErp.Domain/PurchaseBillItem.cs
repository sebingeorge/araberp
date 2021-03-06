﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class PurchaseBillItem
    {
        public int PurchaseBillItemId { get; set; }
        public int PurchaseBillId { get; set; }
        public string ItemName { get; set; }
        public string GRNNoDate { get; set; }
        public int GRNItemId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal TotAmount { get; set; }
        public bool isActive { get; set; }
        public int OrganizationId { get; set; }
    }
}
